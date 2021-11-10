using System.Collections.Generic;
using UnityEngine;
using Android.BLE.Commands;

namespace Android.BLE
{
    public class BleManager : MonoBehaviour
    {
        public static BleManager Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;
                else
                {
                    CreateBleManagerObject();
                    return _instance;
                }
            }
        }
        private static BleManager _instance;

        public static bool IsInitialized { get => _initialized; }
        private static bool _initialized = false;

        [SerializeField]
        private BleAdapter _adapter;

        [Tooltip("Use Initialize() if you want to Initialize manually")]
        public bool InitializeOnAwake = true;

        [Header("Logging")]
        [Tooltip("Logs all messages coming through the BleManager")]
        public bool LogAllMessages = false;

        [Tooltip("Passes messages through to the Unity Debug.Log system")]
        public bool UseUnityLog = true;
        [Tooltip("Passes messages through to Android's Logcat")]
        public bool UseAndroidLog = false;

        internal static AndroidJavaObject _bleLibrary = null;

        private readonly Queue<BleCommand> _commandQueue = new Queue<BleCommand>();
        private readonly List<BleCommand> _parrallelStack = new List<BleCommand>();

        private static BleCommand _activeCommand = null;
        private static float _activeTimer = 0f;

        private BleObject _receivedCommand = null;

        private void Awake()
        {
            _instance = this;

            if (InitializeOnAwake)
                Initialize();

            _adapter.OnMessageReceived += OnBleMessageReceived;
            _adapter.OnErrorReceived += OnErrorReceived;
        }

        private void Update()
        {
            _activeTimer += Time.deltaTime;

            if(_activeCommand != null && _activeTimer > _activeCommand.Timeout)
            {
                CheckForLog("Timed Out: " + _activeCommand + " - " + _activeCommand.Timeout);

                _activeTimer = 0f;
                _activeCommand.EndOnTimeout();

                if (_commandQueue.Count > 0)
                {
                    _activeCommand = _commandQueue.Dequeue();
                    _activeCommand?.Start();

                    if (_activeCommand != null)
                        CheckForLog("Executing new Command: " + _activeCommand.GetType().Name);
                }
                else
                    _activeCommand = null;
            }
        }

        public void Initialize()
        {
            if (!_initialized)
            {
                if (_instance == null)
                    CreateBleManagerObject();

                #region Adapter
                if (_adapter == null)
                {
                    _adapter = FindObjectOfType<BleAdapter>();
                    if (_adapter == null)
                    {
                        GameObject bleAdapter = new GameObject(typeof(BleAdapter).Name);
                        bleAdapter.transform.SetParent(Instance.transform);

                        _adapter = bleAdapter.AddComponent<BleAdapter>();
                    }
                }
                #endregion

                #region Android Library
                if (_bleLibrary == null)
                {
                    AndroidJavaClass librarySingleton = new AndroidJavaClass("com.velorexe.unityandroidble.UnityAndroidBLE");
                    _bleLibrary = librarySingleton.CallStatic<AndroidJavaObject>("getInstance");
                }
                #endregion
            }
        }

        public void DeInitialize()
        {
            foreach (BleCommand command in _parrallelStack)
                command.End();

            _bleLibrary?.Dispose();

            if (_adapter != null)
                Destroy(_adapter.gameObject);
        }

        private void OnBleMessageReceived(BleObject obj)
        {
            CheckForLog(JsonUtility.ToJson(obj, true));

            if(_activeCommand != null && _activeCommand.CommandReceived(obj))
            {
                _activeCommand.End();

                if (_commandQueue.Count > 0)
                {
                    _activeCommand = _commandQueue.Dequeue();
                    _activeCommand?.Start();

                    if (_activeCommand != null)
                        CheckForLog("Executing new Command: " + _activeCommand.GetType().Name);
                }
                else
                    _activeCommand = null;
            }

            for (int i = 0; i < _parrallelStack.Count; i++)
            {
                if (_parrallelStack[i].CommandReceived(obj))
                {
                    _parrallelStack[i].End();
                    _parrallelStack.RemoveAt(i);
                }
            }
        }

        private void OnErrorReceived(string errorMessage)
        {
            CheckForLog(errorMessage);
        }

        private static void CheckForLog(string logMessage)
        {
            if (Instance.UseUnityLog)
                Debug.LogWarning(logMessage);
            if (Instance.UseAndroidLog)
                AndroidLog(logMessage);
        }

        public static void AndroidLog(string message)
        {
            if (_initialized)
                _bleLibrary?.CallStatic("androidLog", message);
        }

        internal static void SendCommand(string command, params object[] parameters)
        {
            if(Instance.LogAllMessages)
                CheckForLog("Calling Command: " + command);
            _bleLibrary?.Call(command, parameters);
        }

        private static void CreateBleManagerObject()
        {
            GameObject managerObject = new GameObject();
            managerObject.name = "BleManager";

            managerObject.AddComponent<BleManager>();
        }

        private void OnDestroy() => DeInitialize();
    }
}
using System.Collections;
using UnityEngine;

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

        internal static AndroidJavaObject _bleLibrary;

        private void Awake()
        {
            _instance = this;

            if (InitializeOnAwake)
                Initialize();

            _adapter.OnMessageReceived += OnBleMessageReceived;
            _adapter.OnErrorReceived += OnErrorReceived;
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

        private void OnBleMessageReceived(BleObject obj)
        {
            if (LogAllMessages)
                AndroidLog(JsonUtility.ToJson(obj, true));
        }

        private void OnErrorReceived(string errorMessage)
        {
            CheckForLog(errorMessage);
        }

        private void CheckForLog(string logMessage)
        {
            if (UseUnityLog)
                Debug.LogWarning(logMessage);
            if (UseAndroidLog)
                AndroidLog(logMessage);
        }

        public void AndroidLog(string message)
        {
            if (_initialized)
                _bleLibrary?.CallStatic("androidLog", message);
        }

        internal void SendCommand(string command, params object[] parameters)
        {
            if (LogAllMessages)
                CheckForLog("Calling Command: " + command);
            _bleLibrary?.Call(command, parameters);
        }

        private static void CreateBleManagerObject()
        {
            GameObject managerObject = new GameObject();
            managerObject.name = "BleManager";

            managerObject.AddComponent<BleManager>();
        }
    }
}
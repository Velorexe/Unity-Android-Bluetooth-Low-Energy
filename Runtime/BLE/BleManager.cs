using Android.BLE.Commands;
using System.Collections.Generic;
using UnityEngine;

namespace Android.BLE
{
    /// <summary>
    /// The Singleton manager that handles all BLE interactions for the plugin
    /// </summary>
    public class BleManager : MonoBehaviour
    {
        /// <summary>
        /// Gets a Singleton instance of the <see cref="BleManager"/>
        /// or creates one if it doesn't exist.
        /// </summary>
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

        /// <summary>
        /// Will be <see langword="true"/> if the <see cref="BleManager"/> is initialized.
        /// </summary>
        public static bool IsInitialized { get => _initialized; }
        private static bool _initialized = false;

        [SerializeField]
        private BleAdapter _adapter;

        /// <summary>
        /// <see langword="true"/> if <see cref="Initialize"/> is called on Unity's <see cref="Awake"/>.
        /// </summary>
        [Tooltip("Use Initialize() if you want to Initialize manually")]
        public bool InitializeOnAwake = true;

        /// <summary>
        /// <see langword="true"/> if all interactions with the <see cref="BleManager"/> should be logged.
        /// </summary>
        [Header("Logging")]
        [Tooltip("Logs all messages coming through the BleManager")]
        public bool LogAllMessages = false;

        /// <summary>
        /// <see langword="true"/> if Android's log messages should be passed through <see cref="Debug.Log(object)"/>.
        /// </summary>
        [Tooltip("Passes messages through to the Unity Debug.Log system")]
        public bool UseUnityLog = true;

        /// <summary>
        /// <see langword="true"/> if Unity's log messages should be passed through LogCat.
        /// </summary>
        [Tooltip("Passes messages through to Android's Logcat")]
        public bool UseAndroidLog = false;

        /// <summary>
        /// The Java library's BleManager hook.
        /// </summary>
        internal static AndroidJavaObject _bleLibrary = null;

        /// <summary>
        /// Incoming queue of <see cref="BleCommand"/> that have yet to be processed.
        /// </summary>
        private readonly Queue<BleCommand> _commandQueue = new Queue<BleCommand>();

        /// <summary>
        /// The stack of parallel running <see cref="BleCommand"/>.
        /// </summary>
        private readonly List<BleCommand> _parrallelStack = new List<BleCommand>();

        /// <summary>
        /// The active non-parallel or continuous <see cref="BleCommand"/>.
        /// </summary>
        private static BleCommand _activeCommand = null;

        /// <summary>
        /// Timer to track the <see cref="_activeCommand"/>'s runtime.
        /// </summary>
        private static float _activeTimer = 0f;

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

            // Checks if the _activeCommand has timed out
            if (_activeCommand != null && _activeTimer > _activeCommand.Timeout)
            {
                CheckForLog("Timed Out: " + _activeCommand + " - " + _activeCommand.Timeout);

                // Resets timers and ends the current _activeCommand
                _activeTimer = 0f;
                _activeCommand.EndOnTimeout();

                if (_commandQueue.Count > 0)
                {
                    // Sets a new _activeCommand
                    _activeCommand = _commandQueue.Dequeue();
                    _activeCommand?.Start();

                    if (_activeCommand != null)
                        CheckForLog("Executing new Command: " + _activeCommand.GetType().Name);
                }
                else
                    _activeCommand = null;
            }
        }

        /// <summary>
        /// Initialized the <see cref="BleManager"/> instance.
        /// Sets up the Java Library hooks and prepares a <see cref="BleAdapter"/> to receive messages.
        /// </summary>
        public void Initialize()
        {
            if (!_initialized)
            {
                // Creates a new Singleton instance
                if (_instance == null)
                    CreateBleManagerObject();

                // Prepares a BleAdapter to receive messages
                #region Adapter
                if (_adapter == null)
                {
                    _adapter = FindObjectOfType<BleAdapter>();
                    if (_adapter == null)
                    {
                        GameObject bleAdapter = new GameObject(nameof(BleAdapter));
                        bleAdapter.transform.SetParent(Instance.transform);

                        _adapter = bleAdapter.AddComponent<BleAdapter>();
                    }
                }
                #endregion

                // Binds to the com.velorexe.unityandroidble.UnityAndroidBLE Singleton
                #region Android Library
                if (_bleLibrary == null)
                {
                    AndroidJavaClass librarySingleton = new AndroidJavaClass("com.velorexe.unityandroidble.UnityAndroidBLE");
                    _bleLibrary = librarySingleton.CallStatic<AndroidJavaObject>("getInstance");
                }
                #endregion
            }
        }

        /// <summary>
        /// Ends all currently running <see cref="BleCommand"/> and
        /// disposes of the Java library hooks
        /// </summary>
        public void DeInitialize()
        {
            foreach (BleCommand command in _parrallelStack)
                command.End();

            _bleLibrary?.Dispose();

            if (_adapter != null)
                Destroy(_adapter.gameObject);
        }

        /// <summary>
        /// Gets called when a new message is received by the <see cref="BleAdapter"/>.
        /// </summary>
        /// <param name="obj">The <see cref="BleObject"/> that's received from the Java library.</param>
        private void OnBleMessageReceived(BleObject obj)
        {
            CheckForLog(JsonUtility.ToJson(obj, true));

            // Checks if the _activeCommand consumes the BleObject
            if (_activeCommand != null && _activeCommand.CommandReceived(obj))
            {
                _activeCommand.End();

                // Queues a new _activeCommand if it has consumed the BleObject
                // Since the command is not continious or parallel, it should be cleared if it's purpose is fulfilled
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

            // Run through the parallel stack, remove the commands that have consumed the BleObject
            for (int i = 0; i < _parrallelStack.Count; i++)
            {
                if (_parrallelStack[i].CommandReceived(obj))
                {
                    _parrallelStack[i].End();
                    _parrallelStack.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Queues a new <see cref="BleCommand"/> to execute.
        /// </summary>
        /// <param name="command">The <see cref="BleCommand"/> that should be handled by the <see cref="BleManager"/>.</param>
        public void QueueCommand(BleCommand command)
        {
            CheckForLog("Queueing Command: " + command.GetType().Name);
            if (command.RunParallel || command.RunContiniously)
            {
                _parrallelStack.Add(command);
                command.Start();
            }
            else
            {
                if (_activeCommand == null)
                {
                    _activeTimer = 0f;

                    _activeCommand = command;
                    _activeCommand.Start();
                }
                else
                    _commandQueue.Enqueue(command);
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

        /// <summary>
        /// Calls a method from the Java library that matches the <paramref name="command"/>.
        /// </summary>
        /// <param name="command">The method name inside the Java library.</param>
        /// <param name="parameters">Any additional parameters that the Java method defines.</param>
        internal static void SendCommand(string command, params object[] parameters)
        {
            if (Instance.LogAllMessages)
                CheckForLog("Calling Command: " + command);
            _bleLibrary?.Call(command, parameters);
        }

        /// <summary>
        /// Creates a new <see cref="GameObject"/> instance for the <see cref="BleManager"/> to attach to.
        /// </summary>
        private static void CreateBleManagerObject()
        {
            GameObject managerObject = new GameObject();
            managerObject.name = "BleManager";

            managerObject.AddComponent<BleManager>();
        }

        private void OnDestroy() => DeInitialize();
    }
}
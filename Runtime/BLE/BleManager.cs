using System.Collections.Generic;
using UnityEngine;

namespace Android.BLE
{
    public class BleManager : MonoBehaviour, IBleNotify
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
                {
                    return _instance;
                }
                else
                {
                    CreateBleManagerObject();
                    return _instance;
                }
            }
        }
        private static BleManager _instance;

        [Tooltip("Initializes the BleManager on Unity's Awake. " +
            "Helpful if you directly want to start using BLE functionality. " +
            "Else, use the Initialize method instead.")]
        [SerializeField]
        private bool _initializeOnAwake = true;

        /// <summary>
        /// Adapter between the Java library and Unity.
        /// </summary>
        [SerializeField]
        private BleMessageAdapter _messageAdapter;

        /// <summary>
        /// A <see cref="Random"/> to create random Task ID's.
        /// </summary>
        private static readonly System.Random _random = new System.Random();

        /// <summary>
        /// The lists of callbacks to notify when a new message comes from the Java library.
        /// </summary>
        private readonly Dictionary<string, TaskDescription> _callbackNotifiers = new Dictionary<string, TaskDescription>();

        /// <summary>
        /// The Java library's object on which methods can be executed.
        /// </summary>
        private static AndroidJavaObject _javaBleManager = null;

        /// <summary>
        /// Callback to receive a notification once a device is found.
        /// </summary>
        private OnDeviceFound _onDeviceFound = null;

        void Awake()
        {
            _instance = this;

            if (_initializeOnAwake)
            {
                Initialize();
            }
        }

        /// <summary>
        /// Initializes the <see cref="BleManager"/> by getting the Java library and
        /// creating a <see cref="BleMessageAdapter"/> to receive notifications from.
        /// </summary>
        public void Initialize()
        {
            #region Adapter
            if (_messageAdapter == null)
            {
                _messageAdapter = FindObjectOfType<BleMessageAdapter>();
                if (_messageAdapter == null)
                {
                    GameObject bleAdapter = new GameObject(nameof(BleMessageAdapter));
                    bleAdapter.transform.SetParent(transform);

                    _messageAdapter = bleAdapter.AddComponent<BleMessageAdapter>();
                }
            }

            _messageAdapter.OnMessageReceived += OnBleMessageReceived;
            #endregion

            #region Android Library
            if (_javaBleManager == null)
            {
                AndroidJavaClass unityAndroidBle = new AndroidJavaClass("com.velorexe.unityandroidble.UnityAndroidBLE");
                _javaBleManager = unityAndroidBle.CallStatic<AndroidJavaObject>("getInstance");
            }
            #endregion
        }

        /// <summary>
        /// Searches for nearby BLE devices. Calls <see cref="OnDeviceFound"/> once a new
        /// device has been found.
        /// </summary>
        /// <param name="scanPeriod">The period of time in milliseconds to scan for.</param>
        /// <param name="onDeviceFound">The callback to notify you once a new Device has been found.</param>
        public void SearchForDevices(int scanPeriod, OnDeviceFound onDeviceFound)
        {
            _onDeviceFound = onDeviceFound;
            BleTask task = new BleTask("searchForBleDevices", scanPeriod);

            SendTask(task, this, runsContiniously: true);
        }

        /// <summary>
        /// Searches for nearby BLE devices with a filter. Calls <see cref="OnDeviceFound"/> once a new
        /// device has been found.
        /// </summary>
        /// <param name="scanPeriod">The period of time in milliseconds to scan for.</param>
        /// <param name="onDeviceFound">The callback to notify you once a new Device has been found.</param>
        /// <param name="deviceMac">The device's MAC address to filter on.</param>
        /// <param name="deviceName">The device's name to filter on.</param>
        /// <param name="serviceUuid">A service UUID to filter on.</param>
        public void SearchForDevicesWithFilter(
            int scanPeriod,
            OnDeviceFound onDeviceFound,
            string deviceMac = "",
            string deviceName = "",
            string serviceUuid = "")
        {
            _onDeviceFound = onDeviceFound;
            BleTask task = new BleTask("searchForBleDevicesWithFilter", scanPeriod, deviceMac, deviceName, serviceUuid);

            SendTask(task, this, runsContiniously: true);
        }

        /// <summary>
        /// Sends a task to the Java library to execute.
        /// Uses the generated ID to keep track of the task.
        /// </summary>
        /// <param name="task">The task that contains the method to execute on the Java side.</param>
        /// <param name="receiver">Once a notification with information comes back from the Java
        /// library, the class with this interface will receive the notification.</param>
        /// <param name="runsContiniously"><see langword="true"/> if the <see cref="BleTask"/> should not be receive any
        /// further notifications after the first one.</param>
        /// <returns>A randomly generated short Task ID to keep track off internally.</returns>
        internal string SendTask(BleTask task, IBleNotify receiver, bool runsContiniously = false)
        {
            string id = GenerateTaskId();

            List<object> parameters = new List<object> { id };
            parameters.AddRange(task.Parameters);

            Debug.Log("Queueing task with ID: " + id);

            _callbackNotifiers.Add(id, new TaskDescription(receiver, runsContiniously));
            _javaBleManager.Call(task.MethodDefinition, parameters.ToArray());

            return id;
        }

        /// <summary>
        /// Forcibly removes a <see cref="BleTask"/> from the callback stack.
        /// </summary>
        /// <param name="id">The <see cref="BleTask"/> ID of the task that needs to be removed</param>
        internal void RemoveTaskFromStack(string id)
        {
            if (!_callbackNotifiers.ContainsKey(id))
            {
                return;
            }

            _callbackNotifiers.Remove(id);
        }

        /// <summary>
        /// The callback attached to the <see cref="BleMessageAdapter"/> to receive messages.
        /// </summary>
        /// <param name="msg">The converted message from the Java library.</param>
        private void OnBleMessageReceived(BleMessage msg)
        {
            if (!_callbackNotifiers.ContainsKey(msg.ID))
            {
                Debug.LogError(
                    $"No OnBleMessage with ID {msg.ID} is in the BleManager's stack.");
                return;
            }

            // Executes the function tied to the ID
            _callbackNotifiers[msg.ID].Notifier.OnMessage(msg);
        }

        /// <summary>
        /// Generates a random Task ID.
        /// </summary>
        /// <returns>A randomly generated short Task ID.</returns>
        private string GenerateTaskId() => _random.Next().ToString("x");

        /// <summary>
        /// Creates a new <see cref="GameObject"/> instance for the <see cref="BleManager"/> to attach to.
        /// </summary>
        private static void CreateBleManagerObject()
        {
            BleManager manager = FindObjectOfType<BleManager>();

            if (manager == null)
            {
                GameObject managerObject = new GameObject();
                managerObject.name = nameof(BleManager);

                manager = managerObject.AddComponent<BleManager>();
            }
        }

        /// <summary>
        /// The <see cref="IBleNotify"/> method that executes for callbacks related to searching devices.
        /// </summary>
        /// <param name="msg">The converted message from the Java library.</param>
        public void OnMessage(BleMessage msg)
        {
            switch (msg.Command)
            {
                case "deviceFound":
                    BleDevice device = new BleDevice(msg.Device, msg.Name);
                    _onDeviceFound.Invoke(device);
                    break;
                case "searchStop":
                    RemoveTaskFromStack(msg.ID);
                    break;
            }
        }

        /// <summary>
        /// A small struct containing the <see cref="IBleNotify"/> for a callback, but
        /// also a boolean that reflects if the <see cref="BleTask"/> should be removed once a notification is passed.
        /// </summary>
        private struct TaskDescription
        {
            public IBleNotify Notifier;
            public bool RunContinously;

            public TaskDescription(IBleNotify notifier, bool runContiniously)
            {
                Notifier = notifier;
                RunContinously = runContiniously;
            }
        }
    }

    public delegate void OnDeviceFound(BleDevice device);
}

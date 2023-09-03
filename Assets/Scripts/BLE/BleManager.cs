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

        [SerializeField]
        private bool _initializeOnAwake = true;

        [SerializeField]
        private BleMessageAdapter _messageAdapter;


        private static readonly System.Random _random = new System.Random();

        private readonly Dictionary<string, TaskDescription> _callbackNotifiers = new Dictionary<string, TaskDescription>();

        private static AndroidJavaObject _javaBleManager = null;

        private OnDeviceFound _onDeviceFound = null;

        void Awake()
        {
            _instance = this;

            if (_initializeOnAwake)
            {
                Initialize();
            }
        }

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

        public void SearchForDevices(int scanPeriod, OnDeviceFound onDeviceFound)
        {
            _onDeviceFound = onDeviceFound;
            BleTask task = new BleTask("searchForBleDevices", scanPeriod);

            SendTask(task, this, runsContiniously: true);
        }


        public void SearchForDevicesWithFilter(
            int scanPeriod,
            OnDeviceFound onDeviceFound,
            string deviceUuid = "",
            string deviceName = "",
            string serviceUuid = "")
        {
            _onDeviceFound = onDeviceFound;
            BleTask task = new BleTask("searchForBleDevicesWithFilter", scanPeriod, deviceUuid, deviceName, serviceUuid);

            SendTask(task, this, runsContiniously: true);
        }

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

        internal void RemoveTaskFromStack(string id)
        {
            if (!_callbackNotifiers.ContainsKey(id))
            {
                return;
            }

            _callbackNotifiers.Remove(id);
        }

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

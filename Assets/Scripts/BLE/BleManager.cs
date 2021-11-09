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

        public bool InitializeOnAwake = false;

        public bool LogErrors
        {
            get => _logErrors;
            set
            {
                if (value)
                    _adapter.OnErrorReceived += OnErrorReceived;
                else
                    _adapter.OnErrorReceived -= OnErrorReceived;

                _logErrors = value;
            }
        }
        [SerializeField]
        private bool _logErrors;

        [SerializeField]
        private static BleAdapter _adapter;

        internal static AndroidJavaObject _bleLibrary;

        private void Awake()
        {
            _instance = this;

            if (InitializeOnAwake)
                Initialize();

            _adapter.OnMessageReceived += OnBleMessageReceived;
            if (LogErrors)
                _adapter.OnErrorReceived += OnErrorReceived;
        }

        public static void Initialize()
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
                if(_bleLibrary == null)
                {
                    AndroidJavaClass librarySingleton = new AndroidJavaClass("com.velorexe.unityandroidble.UnityAndroidBLE");
                    _bleLibrary = librarySingleton.CallStatic<AndroidJavaObject>("getInstance");
                }
                #endregion
            }
        }

        private static void OnBleMessageReceived(BleObject obj)
        {

        }

        private static void OnErrorReceived(string errorMessage) => Debug.LogWarning(errorMessage);

        private static void CreateBleManagerObject()
        {
            GameObject managerObject = new GameObject();
            managerObject.name = "BleManager";

            managerObject.AddComponent<BleManager>();
        }
    }
}
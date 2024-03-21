using System;
using System.Collections.Generic;
using UnityEngine;

namespace Android.BLE
{
    public class BleDevice : IBleNotify
    {
        /// <summary>
        /// The MAC address of the <see cref="BleDevice"/>.
        /// </summary>
        public string MacAddress { get; } = string.Empty;

        /// <summary>
        /// The name of the <see cref="BleDevice"/>.
        /// </summary>
        public string Name { get; } = string.Empty;

        /// <summary>
        /// A collection of all the <see cref="BleGattService"/> on the <see cref="BleDevice"/>.
        /// Getting a <see cref="BleGattService"/> should be done using the <see cref="GetService(string)"/> method.
        /// </summary>
        public BleGattService[] Services { get; internal set; } = Array.Empty<BleGattService>();

        /// <summary>
        /// Reflects if the <see cref="BleDevice"/> is connected or not.
        /// </summary>
        public bool IsConnected { get; internal set; }


        /// <summary>
        /// A private collection that maps the UUID to the <see cref="BleGattService"/>.
        /// </summary>
        private readonly Dictionary<string, BleGattService> _servicesMap = new Dictionary<string, BleGattService>();

        /// <summary>
        /// A private string that keeps track of the Task ID used to connect to the device.
        /// </summary>
        private string _connectionTaskId = string.Empty;

        private OnDeviceConnected _onConnected;
        private OnDeviceDisconnected _onDisconnected;
        private OnMtuSizeChanged _onMtuSizeChanged;
        private OnRssiDataFound _onRssiDataFound;

        internal BleDevice(string macAddress, string name)
        {
            MacAddress = macAddress;
            Name = name;
        }

        /// <summary>
        /// Connect to the <see cref="BleDevice"/>.
        /// By will connect with the <see cref="Transportations.TRANSPORT_LE"/> type.
        /// <para>[Calls: <see href="https://developer.android.com/reference/android/bluetooth/BluetoothDevice#connectGatt(android.content.Context,%20boolean,%20android.bluetooth.BluetoothGattCallback)"/>]</para>
        /// </summary>
        /// <param name="onConnected">Is called once the <see cref="BleDevice"/> is connected.</param>
        /// <param name="onDisconnected">Is called if the <see cref="BleDevice"/> gets disconnected or connecting fails.</param>
        public void Connect(OnDeviceConnected onConnected = null, OnDeviceDisconnected onDisconnected = null)
        {
            _onConnected = onConnected;
            _onDisconnected = onDisconnected;

            BleTask task = new BleTask("connectToBleDevice", MacAddress, (int)Transportations.TRANSPORT_LE);
            _connectionTaskId = BleManager.Instance.SendTask(task, this);
        }

        /// <summary>
        /// Connect to the <see cref="BleDevice"/> using a specific <see cref="Transportations"/> type.
        /// By default the <see cref="Transportations"/> type will be <see cref="Transportations.TRANSPORT_LE"/>
        /// <para>[Calls: <see href="https://developer.android.com/reference/android/bluetooth/BluetoothDevice#connectGatt(android.content.Context,%20boolean,%20android.bluetooth.BluetoothGattCallback,%20int))"/>]</para>
        /// </summary>
        /// <param name="transport">The <see cref="Transportations"/> type that should be used when connecting to the <see cref="BleDevice"/>,.</param>
        /// <param name="onConnected">Is called once the <see cref="BleDevice"/> is connected.</param>
        /// <param name="onDisconnected">Is called if the <see cref="BleDevice"/> gets disconnected or connecting fails.</param>
        public void Connect(
            Transportations transport,
            OnDeviceConnected onConnected = null,
            OnDeviceDisconnected onDisconnected = null)
        {
            _onConnected = onConnected;
            _onDisconnected = onDisconnected;

            BleTask task = new BleTask("connectToBleDevice", MacAddress, (int)transport);
            _connectionTaskId = BleManager.Instance.SendTask(task, this);
        }

        /// <summary>
        /// Disconnect from the <see cref="BleDevice"/>.
        /// <para>[Calls: <see href="https://developer.android.com/reference/android/bluetooth/BluetoothGatt#disconnect()"/></para>
        /// </summary>
        public void Disconnect()
        {
            BleTask task = new BleTask("disconnectFromBleDevice", MacAddress);
            _connectionTaskId = BleManager.Instance.SendTask(task, this);
        }

        /// <summary>
        /// Request the <see cref="BleDevice"/>'s MTU Size to be bigger or smaller.
        /// <para>[Calls: <see href="https://developer.android.com/reference/android/bluetooth/BluetoothGatt#requestMtu(int)"/>]</para>
        /// </summary>
        /// <param name="mtuSize">The requested size that the MTU buffer should be.</param>
        /// <param name="onMtuSizeChanged">A callback that returns the MTU size that the <see cref="BleDevice"/> has after your request.</param>
        public void RequestMtuSize(int mtuSize, OnMtuSizeChanged onMtuSizeChanged = null)
        {
            _onMtuSizeChanged = onMtuSizeChanged;

            BleTask task = new BleTask("requestMtuSize", MacAddress, mtuSize);
            BleManager.Instance.SendTask(task, this);
        }

        /// <summary>
        /// Gets the RSSI <see cref="short"/> if it's available fo the <see cref="BleDevice"/>.
        /// <para>[Calls: <see href="https://developer.android.com/reference/android/content/Intent#getShortExtra(java.lang.String,%20short)"/></para>
        /// </summary>
        /// <param name="onRssiDataFound">A callback that returns the RSSI of the <see cref="BleDevice"/>.
        /// Returns <see cref="short.MinValue"/> if RSSI is not available for the <see cref="BleDevice"/>.</param>
        public void GetRssi(OnRssiDataFound onRssiDataFound)
        {
            _onRssiDataFound = onRssiDataFound;

            BleTask task = new BleTask("getRssiForDevice", MacAddress);
            BleManager.Instance.SendTask(task, this);
        }

        /// <summary>
        /// Returns the <see cref="BleGattService"/> if it's f.ound, else it returns <see langword="null"/>.
        /// Supports both 16-bit UUID's and 128-bit UUID's
        /// </summary>
        /// <param name="serviceUuid">The UUID of the <see cref="BleGattService"/>.</param>
        /// <returns>Returns the <see cref="BleGattService"/> if it's found, else it returns <see langword="null"/>.</returns>
        public BleGattService GetService(string serviceUuid)
        {
            // If a shorthand UUID is passed
            if (serviceUuid.Length == 4)
            {
                serviceUuid = "0000" + serviceUuid + "-0000-1000-8000-00805f9b34fb";
            }

            serviceUuid = serviceUuid.ToLower();

            if (!_servicesMap.ContainsKey(serviceUuid))
            {
                return null;
            }

            return _servicesMap[serviceUuid];
        }

        /// <summary>
        /// Returns the <see cref="BleGattCharacteristic"/> if it's f.ound, else it returns <see langword="null"/>.
        /// Supports both 16-bit UUID's and 128-bit UUID's
        /// </summary>
        /// <param name="serviceUuid">The UUID of the <see cref="BleGattCharacteristic"/>.</param>
        /// <returns>Returns the <see cref="BleGattCharacteristic"/> if it's found, else it returns <see langword="null"/>.</returns>
        public BleGattCharacteristic GetCharacteristic(string serviceUuid, string characteristicUuid)
        {
            BleGattService service = GetService(serviceUuid);
            if (service == null)
            {
                return null;
            }

            return service.GetCharacteristic(characteristicUuid);
        }

        /// <summary>
        /// Used internaly to handle the <see cref="BleMessage"/> received from the Java library.
        /// </summary>
        /// <param name="msg">The converted message from the Java library.</param>
        public void OnMessage(BleMessage msg)
        {
            if (!msg.HasError)
            {
                switch (msg.Command)
                {
                    case "connectedToDevice":
                        IsConnected = true;
                        break;
                    case "disconnectedFromDevice":
                        IsConnected = false;
                        _onDisconnected.Invoke(this);

                        BleManager.Instance.RemoveTaskFromStack(_connectionTaskId);
                        break;
                    case "connectingToDevice":
                        break;

                    case "discoveredServicesAndCharacteristics":
                        ServiceJsonPayload[] receivedServices = JsonUtility.FromJson<Wrapper<ServiceJsonPayload>>(msg.JsonData).Data;
                        BleGattService[] service = new BleGattService[receivedServices.Length];

                        for (int i = 0; i < receivedServices.Length; i++)
                        {
                            BleGattCharacteristic[] characteristics = new BleGattCharacteristic[receivedServices[i].Characteristics.Length];
                            for (int j = 0; j < receivedServices[i].Characteristics.Length; j++)
                            {
                                characteristics[j] = new BleGattCharacteristic(
                                    receivedServices[i].Characteristics[j].UUID.ToLower(),
                                    receivedServices[i].Characteristics[j].Permissions,
                                    receivedServices[i].Characteristics[j].Properties,
                                    receivedServices[i].Characteristics[j].WriteTypes);
                            }

                            service[i] = new BleGattService(receivedServices[i].UUID.ToLower(), characteristics, this);
                            _servicesMap.Add(receivedServices[i].UUID, service[i]);

                            foreach (BleGattCharacteristic characteristic in characteristics)
                            {
                                characteristic.SetParent(service[i]);
                            }
                        }

                        Services = service;

                        _onConnected.Invoke(this);
                        break;
                    case "requestMtuSize":
                        _onMtuSizeChanged?.Invoke(this, Convert.ToInt32(msg.Base64Data));
                        break;
                    case "getRssiForDevice":
                        _onRssiDataFound?.Invoke(this, short.Parse(msg.Base64Data));
                        break;
                }
            }
            else
            {
                Debug.LogError(msg.ErrorMessage);
            }
        }

        [Serializable]
        private class ServiceJsonPayload
        {
            public string UUID { get { return uuid; } }
            [SerializeField]
            private string uuid;

            public CharacteristicJsonPayload[] Characteristics { get { return characteristics; } }
            [SerializeField]
            private CharacteristicJsonPayload[] characteristics;

            [Serializable]
            public class CharacteristicJsonPayload
            {
                public string UUID { get { return uuid; } }
                [SerializeField]
                private string uuid;

                public int Permissions { get { return permissions; } }
                [SerializeField]
                private int permissions;

                public int Properties { get { return properties; } }
                [SerializeField]
                private int properties;

                public int WriteTypes { get { return writeTypes; } }
                [SerializeField]
                private int writeTypes;
            }
        }

        private class Wrapper<T>
        {
            public T[] Data { get { return data; } }
            [SerializeField]
            private T[] data;
        }
    }

    public delegate void OnDeviceConnected(BleDevice device);

    public delegate void OnDeviceDisconnected(BleDevice device);

    public delegate void OnMtuSizeChanged(BleDevice device, int mtuSize);

    public delegate void OnRssiDataFound(BleDevice device, short rssi);

    public enum Transportations
    {
        TRANSPORT_AUTO = 0,
        TRANSPORT_BREDR = 1,
        TRANSPORT_LE = 2,
    }
}

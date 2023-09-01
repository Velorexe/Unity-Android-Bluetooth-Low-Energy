using System;
using System.Collections.Generic;
using UnityEngine;

namespace Android.BLE
{
    public class BleDevice : IBleNotify
    {
        public string MacAddress { get; } = string.Empty;

        public string Name { get; } = string.Empty;

        public BleGattService[] Services { get; } = Array.Empty<BleGattService>();

        public bool IsConnected { get; internal set; }

        private readonly Dictionary<string, BleGattService> _servicesMap;

        private string _taskId = string.Empty;

        private OnDeviceConnected _onConnected;
        private OnDeviceDisconnected _onDisconnected;


        internal BleDevice(string macAddress, string name)
        {
            MacAddress = macAddress;
            Name = name;
        }


        public void Connect(OnDeviceConnected onConnected = null, OnDeviceDisconnected onDisconnected = null)
        {
            _onConnected = onConnected;
            _onDisconnected = onDisconnected;

            BleTask task = new BleTask("connectToBleDevice", MacAddress, (int)Transportations.TRANSPORT_LE);
            _taskId = BleManager.Instance.SendTask(task, this);
        }

        public void Connect(
            Transportations transport,
            OnDeviceConnected onConnected = null,
            OnDeviceDisconnected onDisconnected = null)
        {
            _onConnected = onConnected;
            _onDisconnected = onDisconnected;

            BleTask task = new BleTask("connectToBleDevice", MacAddress, (int)transport);
            _taskId = BleManager.Instance.SendTask(task, this);
        }

        public BleGattService GetService(string serviceUuid)
        {
            // If a shorthand UUID is passed
            if (serviceUuid.Length == 4)
            {
                serviceUuid = "0000" + serviceUuid + "-0000-1000-8000-00805f9b34fb";
            }

            if (!_servicesMap.ContainsKey(serviceUuid))
            {
                return null;
            }

            return _servicesMap[serviceUuid];
        }

        public BleGattCharacteristic GetCharacteristic(string serviceUuid, string characteristicUuid)
        {
            // If a shorthand UUID is passed
            if (serviceUuid.Length == 4)
            {
                serviceUuid = "0000" + serviceUuid + "-0000-1000-8000-00805f9b34fb";
            }

            if (!_servicesMap.ContainsKey(serviceUuid))
            {
                return null;
            }

            return _servicesMap[serviceUuid].GetCharacteristic(characteristicUuid);
        }

        public void OnMessage(BleMessage msg)
        {
            if (msg.ID == _taskId && !msg.HasError)
            {
                switch (msg.Command)
                {
                    case "connectedToDevice":
                        IsConnected = true;
                        _onConnected.Invoke(this);
                        break;
                    case "disconnectedFromDevice":
                        IsConnected = false;
                        _onDisconnected.Invoke(this);

                        BleManager.Instance.RemoveTaskFromStack(_taskId);
                        break;
                    case "connectingToDevice":
                        break;

                    case "discoveredServicesAndCharacteristics":
                        ServiceJsonPayload[] receivedServices = JsonUtility.FromJson<ServiceJsonPayload[]>(msg.JsonData);
                        BleGattService[] service = new BleGattService[receivedServices.Length];

                        for (int i = 0; i < receivedServices.Length; i++)
                        {
                            BleGattCharacteristic[] characteristics = new BleGattCharacteristic[receivedServices[i].Characteristics.Length];
                            for (int j = 0; j < receivedServices[i].Characteristics.Length; j++)
                            {
                                characteristics[j] = new BleGattCharacteristic(
                                    receivedServices[i].Characteristics[j].UUID,
                                    receivedServices[i].Characteristics[j].Permissions,
                                    receivedServices[i].Characteristics[j].Properties,
                                    receivedServices[i].Characteristics[j].WriteTypes);
                            }

                            service[i] = new BleGattService(receivedServices[i].UUID, characteristics);
                        }

                        break;
                }
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
    }

    public delegate void OnDeviceConnected(BleDevice device);

    public delegate void OnDeviceDisconnected(BleDevice device);

    public enum Transportations
    {
        TRANSPORT_AUTO = 0,
        TRANSPORT_BREDR = 1,
        TRANSPORT_LE = 2,
    }
}

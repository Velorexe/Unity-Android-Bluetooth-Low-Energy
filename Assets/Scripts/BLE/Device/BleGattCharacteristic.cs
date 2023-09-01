using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Android.BLE
{
    public class BleGattCharacteristic : IBleNotify
    {
        public string UUID { get; } = string.Empty;

        public BleGattService ParentService = null;

        public BleDevice ParentDevice { get { return ParentService.ParentDevice; } }


        public CharacteristicFormats Format { get; } = CharacteristicFormats.UNKNOWN;

        public CharacteristicPermissions[] Permissions { get; } = Array.Empty<CharacteristicPermissions>();

        public CharacteristicProperties[] Properties { get; } = Array.Empty<CharacteristicProperties>();

        public CharacteristicWriteTypes[] WriteTypes { get; } = Array.Empty<CharacteristicWriteTypes>();


        private Dictionary<string, OnReadValue> _onReadValueTasks = new Dictionary<string, OnReadValue>();

        private Dictionary<string, OnWriteValue> _onWriteValueTasks = new Dictionary<string, OnWriteValue>();

        private Dictionary<string, OnSubscribeValue> _onSubscribeValueTasks = new Dictionary<string, OnSubscribeValue>();


        internal BleGattCharacteristic(string uuid, int permissions, int properties, int writeTypes)
        {
            UUID = uuid;

            int[] parsedValues = permissions.GetDivisors(256);
            Permissions = parsedValues.Select(p => (CharacteristicPermissions)p).ToArray();

            parsedValues = properties.GetDivisors(128);
            Properties = parsedValues.Select(p => (CharacteristicProperties)p).ToArray();

            parsedValues = writeTypes.GetDivisors(4);
            WriteTypes = parsedValues.Select(w => (CharacteristicWriteTypes)w).ToArray();
        }


        public void Read(OnReadValue onRead = null)
        {
            if (ParentDevice.IsConnected)
            {
                BleTask task = new BleTask(
                    "readFromCharacteristic",
                    ParentService.ParentDevice.MacAddress,
                    ParentService.UUID,
                    UUID);

                string id = BleManager.Instance.SendTask(task, this);

                if (onRead != null)
                {
                    _onReadValueTasks.Add(id, onRead);
                }
            }
            else
            {
                Debug.LogError($"A 'Read' on characteristic {UUID} was executed, but device {ParentDevice.MacAddress} isn't connected.");
            }
        }

        public void Write(byte[] data, OnWriteValue onWrite = null)
        {
            if (ParentDevice.IsConnected)
            {
                BleTask task = new BleTask(
                    "writeToCharacteristic",
                    ParentService.ParentDevice.MacAddress,
                    ParentService.UUID,
                    UUID,
                    data);

                string id = BleManager.Instance.SendTask(task, this);

                if (onWrite != null)
                {
                    _onWriteValueTasks.Add(id, onWrite);
                }
            }
            else
            {
                Debug.LogError($"A 'Write' on characteristic {UUID} was executed, but device {ParentDevice.MacAddress} isn't connected.");
            }
        }

        public void Subscribe(OnSubscribeValue onValue = null)
        {
            if (ParentDevice.IsConnected)
            {
                BleTask task = new BleTask(
                    "writeToCharacteristic",
                    ParentService.ParentDevice.MacAddress,
                    ParentService.UUID,
                    UUID);

                string id = BleManager.Instance.SendTask(task, this);

                if (onValue != null)
                {
                    _onSubscribeValueTasks.Add(id, onValue);
                }
            }
            else
            {
                Debug.LogError($"A 'Subscribe' on characteristic {UUID} was executed, but device {ParentDevice.MacAddress} isn't connected.");
            }
        }

        void IBleNotify.OnMessage(BleMessage msg)
        {
            if (_onReadValueTasks.ContainsKey(msg.ID))
            {
                _onReadValueTasks[msg.ID]?.Invoke(!msg.HasError, msg.Data);
                _onReadValueTasks.Remove(msg.ID);

                BleManager.Instance.RemoveTaskFromStack(msg.ID);
            }

            if (_onWriteValueTasks.ContainsKey(msg.ID))
            {
                _onWriteValueTasks[msg.ID]?.Invoke(!msg.HasError);
                _onWriteValueTasks.Remove(msg.ID);

                BleManager.Instance.RemoveTaskFromStack(msg.ID);
            }

            if (_onSubscribeValueTasks.ContainsKey(msg.ID))
            {
                _onSubscribeValueTasks[msg.ID]?.Invoke(msg.Data);
            }
        }
    }

    public delegate void OnReadValue(bool success, byte[] data);

    public delegate void OnWriteValue(bool success);

    public delegate void OnSubscribeValue(byte[] data);

    public enum CharacteristicFormats
    {
        UNKNOWN = -1,

        FORMAT_UINT8 = 17,
        FORMAT_UINT16 = 18,
        FORMAT_UINT32 = 20,

        FORMAT_SINT8 = 33,
        FORMAT_SINT16 = 34,
        FORMAT_SINT32 = 36,

        FORMAT_FLOAT = 52,
        FORMAT_SFLOAT = 50,
    }

    public enum CharacteristicPermissions
    {
        PERMISSION_READ = 1,

        PERMISSION_READ_ENCRYPTED = 2,
        PERMISSION_READ_ENCRYPTED_MITM = 4,

        PERMISSION_WRITE = 16,

        PERMISSION_WRITE_ENCRYPTED = 32,
        PERMISSION_WRITE_ENCRYPTED_MITM = 64,

        PERMISSION_WRITE_SIGNED = 128,
        PERMISSION_WRITE_SIGNED_MITM = 256,
    }

    public enum CharacteristicProperties
    {
        PROPERTY_BROADCAST = 1,

        PROPERTY_READ = 2,

        PROPERTY_WRITE_NO_RESPONSE = 4,
        PROPERTY_WRITE = 8,

        PROPERTY_NOTIFY = 16,
        PROPERTY_INDICATE = 32,

        PROPERTY_SIGNED_WRITE = 64,

        PROPERTY_EXTENDED_PROPS = 128,
    }

    public enum CharacteristicWriteTypes
    {
        WRITE_TYPE_NO_RESPONSE = 1,
        WRITE_TYPE_DEFAULT = 2,
        WRITE_TYPE_SIGNED = 4,
    }
}

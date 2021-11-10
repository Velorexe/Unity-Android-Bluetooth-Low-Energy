using System;

namespace Android.BLE.Commands
{
    public class ConnectToDevice : BleCommand
    {
        protected readonly string _deviceAddress;

        public bool IsConnected { get => _isConnected; }
        private bool _isConnected = false;

        public readonly ConnectionChange OnConnected, OnDisconnected;

        public readonly ServiceDiscovered OnServiceDiscovered;
        public readonly CharacteristicDiscovered OnCharacteristicDiscovered;

        #region Constructors
        public ConnectToDevice(string deviceAddress) : base(true, true)
        {
            _deviceAddress = deviceAddress;
        }

        public ConnectToDevice(string deviceAddress,
            Action<string> onConnected) : base(true, true)
        {
            _deviceAddress = deviceAddress;
            OnConnected += new ConnectionChange(onConnected);
        }

        public ConnectToDevice(string deviceAddress,
            Action<string> onConnected,
            Action<string> onDisconnected) : base(true, true)
        {
            _deviceAddress = deviceAddress;

            OnConnected += new ConnectionChange(onConnected);
            OnDisconnected += new ConnectionChange(onDisconnected);
        }

        public ConnectToDevice(string deviceAddress,
            Action<string> onConnected,
            Action<string> onDisconnected,
            Action<string, string> onServiceDiscovered,
            Action<string, string, string> onCharacteristicDiscovered) : base(true, true)
        {
            _deviceAddress = deviceAddress;

            OnConnected += new ConnectionChange(onConnected);
            OnDisconnected += new ConnectionChange(onDisconnected);

            OnServiceDiscovered += new ServiceDiscovered(onServiceDiscovered);
            OnCharacteristicDiscovered += new CharacteristicDiscovered(onCharacteristicDiscovered);
        }
        #endregion

        public override void Start() => BleManager.SendCommand("connectToDevice", _deviceAddress);
        public void Disconnect() => BleManager.SendCommand("disconnectDevice", _deviceAddress);

        public override bool CommandReceived(BleObject obj)
        {
            if(string.Equals(obj.Device, _deviceAddress))
            {
                switch (obj.Command)
                {
                    case "DeviceConnected":
                    case "Authenticated":
                        {
                            _isConnected = true;
                            OnConnected?.Invoke(obj.Device);
                        }
                        break;
                    case "ServiceDiscovered":
                        {
                            OnServiceDiscovered?.Invoke(obj.Device, obj.Service);
                            break;
                        }
                    case "CharacteristicDiscovered":
                        {
                            OnCharacteristicDiscovered?.Invoke(obj.Device, obj.Service, obj.Characteristic);
                            break;
                        }
                    case "DisconnectedFromGattServer":
                        {
                            OnDisconnected?.Invoke(obj.Device);
                            return true;
                        }
                }
            }

            return false;
        }

        public delegate void ConnectionChange(string deviceAddress);

        public delegate void ServiceDiscovered(string deviceAddress, string serviceAddress);
        public delegate void CharacteristicDiscovered(string deviceAddress, string serviceAddress, string characteristicAddress);
    }
}
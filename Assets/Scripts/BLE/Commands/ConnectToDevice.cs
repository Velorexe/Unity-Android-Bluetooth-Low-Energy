using System;

namespace Android.BLE.Commands
{
    public class ConnectToDevice : BleCommand
    {
        protected readonly string _deviceAddress;

        public readonly ConnectionChange OnConnected, OnDisconnected;

        public readonly ServiceDiscovered OnServiceDiscovered;
        public readonly CharacteristicDiscovered OnCharacteristicDiscovered;

        #region Constructors
        public ConnectToDevice(string deviceAddress) : base(true, false)
        {
            _deviceAddress = deviceAddress;
        }

        public ConnectToDevice(string deviceAddress,
            Action<string> onConnected) : base(true, false)
        {
            _deviceAddress = deviceAddress;
            OnConnected += new ConnectionChange(onConnected);
        }

        public ConnectToDevice(string deviceAddress,
            Action<string> onConnected,
            Action<string> onDisconnected) : base(true, false)
        {
            _deviceAddress = deviceAddress;

            OnConnected += new ConnectionChange(onConnected);
            OnDisconnected += new ConnectionChange(onDisconnected);
        }

        public ConnectToDevice(string deviceAddress,
            Action<string> onConnected,
            Action<string> onDisconnected,
            Action<string, string> onServiceDiscovered,
            Action<string, string, string> onCharacteristicDiscovered) : base(true, false)
        {
            _deviceAddress = deviceAddress;

            OnConnected += new ConnectionChange(onConnected);
            OnDisconnected += new ConnectionChange(onDisconnected);

            OnServiceDiscovered += new ServiceDiscovered(onServiceDiscovered);
            OnCharacteristicDiscovered += new CharacteristicDiscovered(onCharacteristicDiscovered);
        }
        #endregion

        public override void Start() => BleManager.SendCommand("connectToDevice", _deviceAddress);


        public delegate void ConnectionChange(string deviceAddress);

        public delegate void ServiceDiscovered(string deviceAddress, string serviceAddress);
        public delegate void CharacteristicDiscovered(string deviceAddress, string serviceAddress, string characteristicAddress);
    }
}
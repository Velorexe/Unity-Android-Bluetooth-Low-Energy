namespace Android.BLE.Commands
{
    /// <summary>
    /// Command to connect to a given BLE device.
    /// </summary>
    public class ConnectToDevice : BleCommand
    {
        /// <summary>
        /// The UUID of the device to connect to.
        /// </summary>
        protected readonly string _deviceAddress;

        /// <summary>
        /// <see langword="true"/> if the BLE device is connected.
        /// </summary>
        public bool IsConnected { get => _isConnected; }
        private bool _isConnected = false;

        /// <summary>
        /// The .NET events that indicates a <see cref="ConnectionChange"/>.
        /// </summary>
        public readonly ConnectionChange OnConnected, OnDisconnected;

        /// <summary>
        /// The .NET event that indicates a newly discovered Service.
        /// </summary>
        public readonly ServiceDiscovered OnServiceDiscovered;

        /// <summary>
        /// The .NET event that indicates a newly discovered Characteristic.
        /// </summary>
        public readonly CharacteristicDiscovered OnCharacteristicDiscovered;

        #region Constructors
        /// <summary>
        /// Connects to a BLE device with the given UUID.
        /// </summary>
        /// <param name="deviceAddress">The UUID that BLE should connect to.</param>
        public ConnectToDevice(string deviceAddress) : base(true, true)
        {
            _deviceAddress = deviceAddress;
        }

        /// <summary>
        /// Connects to a BLE device with the given UUID and sends a notification
        /// if the device is connected.
        /// </summary>
        /// <param name="deviceAddress">The UUID that BLE should connect to.</param>
        /// <param name="onConnected">The <see cref="ConnectionChange"/> that will trigger if the device is connected.</param>
        public ConnectToDevice(string deviceAddress,
            ConnectionChange onConnected) : base(true, true)
        {
            _deviceAddress = deviceAddress;
            OnConnected += onConnected;
        }

        /// <summary>
        /// Connects to a BLE device with the given UUID and sends a notification
        /// if the device is connected or has disconnected.
        /// </summary>
        /// <param name="deviceAddress">The UUID that BLE should connect to.</param>
        /// <param name="onConnected">The <see cref="ConnectionChange"/> that will trigger if the device is connected.</param>
        /// <param name="onDisconnected">The <see cref="ConnectionChange"/> that will trigger if the device has disconnected.</param>
        public ConnectToDevice(string deviceAddress,
            ConnectionChange onConnected,
            ConnectionChange onDisconnected) : base(true, true)
        {
            _deviceAddress = deviceAddress;

            OnConnected += onConnected;
            OnDisconnected += onDisconnected;
        }

        /// <summary>
        /// Connects to a BLE device with the given UUID and sends a notification
        /// if the device is connected or has disconnected. Will also send a noticication
        /// if a new Service or Characteristic is discovered.
        /// </summary>
        /// <param name="deviceAddress">The UUID that BLE should connect to.</param>
        /// <param name="onConnected">The <see cref="ConnectionChange"/> that will trigger if the device is connected.</param>
        /// <param name="onDisconnected">The <see cref="ConnectionChange"/> that will trigger if the device has disconnected.</param>
        /// <param name="onServiceDiscovered">The <see cref="ServiceDiscovered"/> that will trigger if a new Service is discovered.</param>
        /// <param name="onCharacteristicDiscovered">The <see cref="CharacteristicDiscovered"/> that will trigger if a new Characteristic is discovered.</param>
        public ConnectToDevice(string deviceAddress,
            ConnectionChange onConnected,
            ConnectionChange onDisconnected,
            ServiceDiscovered onServiceDiscovered,
            CharacteristicDiscovered onCharacteristicDiscovered) : base(true, true)
        {
            _deviceAddress = deviceAddress;

            OnConnected += onConnected;
            OnDisconnected += onDisconnected;

            OnServiceDiscovered += onServiceDiscovered;
            OnCharacteristicDiscovered += onCharacteristicDiscovered;
        }
        #endregion

        public override void Start() => BleManager.SendCommand("connectToDevice", _deviceAddress);
        public void Disconnect() => BleManager.SendCommand("disconnectDevice", _deviceAddress);

        public override bool CommandReceived(BleObject obj)
        {
            if (string.Equals(obj.Device, _deviceAddress))
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

        /// <summary>
        /// A delegate that indicates a connection change on the BLE device.
        /// </summary>
        /// <param name="deviceAddress">The UUID of the BLE device.</param>
        public delegate void ConnectionChange(string deviceAddress);

        /// <summary>
        /// A delegate that indicates a newly discovered Service on the BLE device.
        /// </summary>
        /// <param name="deviceAddress">The UUID of the BLE device.</param>
        /// <param name="serviceAddress">The UUID of the Service.</param>
        public delegate void ServiceDiscovered(string deviceAddress, string serviceAddress);

        /// <summary>
        /// A delegate that indicate a newly discovered Characteristic on the BLE device.
        /// </summary>
        /// <param name="deviceAddress">The UUID of the BLE device.</param>
        /// <param name="serviceAddress">The UUID of the Service.</param>
        /// <param name="characteristicAddress">The UUID of the Characteristic.</param>
        public delegate void CharacteristicDiscovered(string deviceAddress, string serviceAddress, string characteristicAddress);
    }
}
using Android.BLE.Extension;

namespace Android.BLE.Commands
{
    /// <summary>
    /// Command to Subscribe to a BLE Device's Characteristic
    /// </summary>
    public class SubscribeToCharacteristic : BleCommand
    {
        /// <summary>
        /// The UUID of the BLE device.
        /// </summary>
        public readonly string DeviceAddress;

        /// <summary>
        /// The Service that parents the Characteristic.
        /// </summary>
        public readonly string Service;

        /// <summary>
        /// The Characteristic to write the message to.
        /// </summary>
        public readonly string Characteristic;

        /// <summary>
        /// The .NET event that sends the subscribe data back to the user.
        /// </summary>
        public readonly CharacteristicChanged OnCharacteristicChanged;

        /// <summary>
        /// Indicates if the UUID is custom (long-uuid instead of a short-hand).
        /// </summary>
        private readonly bool CustomGatt = false;

        /// <summary>
        /// Subscribes to a given BLE Characteristic.
        /// </summary>
        /// <param name="deviceAddress">The UUID of the device that the BLE should subscribe to.</param>
        /// <param name="service">The UUID of the Service that parents the Characteristic.</param>
        /// <param name="characteristic">The UUID of the Characteristic to read from.</param>
        /// <param name="customGatt"><see langword="true"/> if the GATT Characteristic UUID address is a long-hand, not short-hand.</param>
        public SubscribeToCharacteristic(string deviceAddress, string service, string characteristic, bool customGatt = false) : base(true, true)
        {
            DeviceAddress = deviceAddress;

            Service = service;
            Characteristic = characteristic;

            CustomGatt = customGatt;
        }

        /// <summary>
        /// Subscribes to a given BLE Characteristic and passes the data back to the user.
        /// </summary>
        /// <param name="deviceAddress">The UUID of the device that the BLE should subscribe to.</param>
        /// <param name="service">The UUID of the Service that parents the Characteristic.</param>
        /// <param name="characteristic">The UUID of the Characteristic to read from.</param>
        /// <param name="onDataFound">The <see cref="CharacteristicChanged"/> that will trigger if a value was updated on the Characteristic.</param>
        /// <param name="customGatt"><see langword="true"/> if the GATT Characteristic UUID address is a long-hand, not short-hand.</param>
        public SubscribeToCharacteristic(string deviceAddress, string service, string characteristic, CharacteristicChanged onDataFound, bool customGatt = false) : base(true, true)
        {
            DeviceAddress = deviceAddress;

            Service = service;
            Characteristic = characteristic;

            OnCharacteristicChanged += onDataFound;

            CustomGatt = customGatt;
        }

        public override void Start()
        {
            string command = CustomGatt ? "subscribeToCustomGattCharacteristic" : "subscribeToGattCharacteristic";
            BleManager.SendCommand(command, DeviceAddress, Service, Characteristic);
        }

        public override void End()
        {
            string command = CustomGatt ? "unsubscribeFromCustomGattCharacteristic" : "unsubscribeFromGattCharacteristic";
            BleManager.SendCommand(command, DeviceAddress, Service, Characteristic);
        }

        public void Unsubscribe() => End();

        public override bool CommandReceived(BleObject obj)
        {
            if (string.Equals(obj.Command, "CharacteristicValueChanged"))
            {
                if (CustomGatt)
                {
                    if (string.Equals(obj.Device, DeviceAddress) &&
                        string.Equals(obj.Service, Service) &&
                        string.Equals(obj.Characteristic, Characteristic))
                    {
                        OnCharacteristicChanged?.Invoke(obj.GetByteMessage());
                    }
                }
                else
                {
                    if (string.Equals(obj.Device, DeviceAddress) &&
                        string.Equals(obj.Service.Get4BitUuid(), Service) &&
                        string.Equals(obj.Characteristic.Get4BitUuid(), Characteristic))
                    {
                        OnCharacteristicChanged?.Invoke(obj.GetByteMessage());
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// A delegate that indicates a newly updated value on a Characteristic.
        /// </summary>
        /// <param name="value">The value that was updated on the Characteristic.</param>
        public delegate void CharacteristicChanged(byte[] value);
    }
}

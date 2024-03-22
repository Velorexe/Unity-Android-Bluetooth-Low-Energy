using System.Text;

namespace Android.BLE.Commands
{
    /// <summary>
    /// Command to write to a BLE Device's Characteristic
    /// </summary>
    public class WriteToCharacteristic : BleCommand
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
        /// The data that's send encoded in Base64.
        /// </summary>
        public readonly string Base64Data;

        /// <summary>
        /// Indicates if the UUID is custom (long-uuid instead of a short-hand).
        /// </summary>
        public readonly bool CustomGatt;

        /// <summary>
        /// Writes to a given BLE Characteristic with the Base64 string <paramref name="data"/>.
        /// </summary>
        /// <param name="deviceAddress">The UUID of the device that the BLE should send data to.</param>
        /// <param name="serviceAddress">The UUID of the Service that parents the Characteristic.</param>
        /// <param name="characteristicAddress">The UUID of the Characteristic to read from.</param>
        /// <param name="data">The Base64 encoded data that's send to the Characteristic</param>
        /// <param name="customGatt"><see langword="true"/> if the GATT Characteristic UUID address is a long-hand, not short-hand.</param>
        public WriteToCharacteristic(string deviceAddress, string serviceAddress, string characteristicAddress, string data, bool customGatt = false) : base(false, false)
        {
            DeviceAddress = deviceAddress;
            Service = serviceAddress;
            Characteristic = characteristicAddress;

            Base64Data = data;

            CustomGatt = customGatt;

            _timeout = 1f;
        }

        /// <summary>
        /// Writes to a given BLE Characteristic with the Base64 string <paramref name="data"/>.
        /// </summary>
        /// <param name="deviceAddress">The UUID of the device that the BLE should send data to.</param>
        /// <param name="serviceAddress">The UUID of the Service that parents the Characteristic.</param>
        /// <param name="characteristicAddress">The UUID of the Characteristic to read from.</param>
        /// <param name="data">The byte[] that'll be encoded to Base64 that's send to the Characteristic</param>
        /// <param name="customGatt"><see langword="true"/> if the GATT Characteristic UUID address is a long-hand, not short-hand.</param>
        public WriteToCharacteristic(string deviceAddress, string serviceAddress, string characteristicAddress, byte[] data, bool customGatt = false) : base(false, false)
        {
            DeviceAddress = deviceAddress;
            Service = serviceAddress;
            Characteristic = characteristicAddress;

            Base64Data = Encoding.UTF8.GetString(data);

            CustomGatt = customGatt;

            _timeout = 1f;
        }

        public override void Start()
        {
            string command = CustomGatt ? "writeToCustomGattCharacteristic" : "writeToGattCharacteristic";
            BleManager.SendCommand(command, DeviceAddress, Service, Characteristic, Base64Data);
        }
    }
}
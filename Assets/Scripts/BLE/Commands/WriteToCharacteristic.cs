using System.Text;

namespace Android.BLE.Commands
{
    public class WriteToCharacteristic : BleCommand
    {
        public readonly string DeviceAddress;

        public readonly string Service;
        public readonly string Characteristic;

        public readonly string Base64Data;

        public readonly bool CustomGatt;

        public WriteToCharacteristic(string deviceAddress, string serviceAddress, string characteristicAddress, string data, bool customGatt = false) : base(false, false)
        {
            DeviceAddress = deviceAddress;
            Service = serviceAddress;
            Characteristic = characteristicAddress;

            Base64Data = data;

            CustomGatt = customGatt;

            _timeout = 1f;
        }

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
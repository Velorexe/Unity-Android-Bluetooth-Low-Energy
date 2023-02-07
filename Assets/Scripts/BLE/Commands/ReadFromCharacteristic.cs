using Android.BLE.Extension;

namespace Android.BLE.Commands
{
    public class ReadFromCharacteristic : BleCommand
    {
        public readonly string DeviceAddress;

        public readonly string Service;
        public readonly string Characteristic;

        public ReadCharacteristicValueReceived OnReadCharacteristicValueReceived;

        public readonly bool CustomGatt;

        public ReadFromCharacteristic(string deviceAddress, string serviceAddress, string characteristicAddress, ReadCharacteristicValueReceived valueReceived, bool customGatt = false) : base(false, false)
        {
            DeviceAddress = deviceAddress;
            Service = serviceAddress;
            Characteristic = characteristicAddress;

            OnReadCharacteristicValueReceived = valueReceived;

            CustomGatt = customGatt;

            _timeout = 1f;
        }

        public override void Start()
        {
            string command = CustomGatt ? "readFromCustomCharacteristic" : "readFromCharacteristic";
            BleManager.SendCommand(command, DeviceAddress, Service, Characteristic);
        }

        public override bool CommandReceived(BleObject obj)
        {
            if (string.Equals(obj.Command, "ReadFromCharacteristic"))
            {
                if ((!CustomGatt && string.Equals(obj.Characteristic.Get4BitUuid(), Characteristic) && string.Equals(obj.Service.Get4BitUuid(), Service))
                    || (CustomGatt && string.Equals(obj.Characteristic, Characteristic) && string.Equals(obj.Service, Service)))
                {
                    OnReadCharacteristicValueReceived?.Invoke(obj.GetByteMessage());
                    return true;
                }
            }

            return false;
        }

        public delegate void ReadCharacteristicValueReceived(byte[] value);
    }
}
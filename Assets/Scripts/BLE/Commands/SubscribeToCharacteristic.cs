namespace Android.BLE.Commands
{
    public class SubscribeToCharacteristic : BleCommand
    {
        public readonly string DeviceAddress;

        public readonly string Service;
        public readonly string Characteristic;

        public readonly CharacteristicChanged OnCharacteristicChanged;

        private readonly bool _customGatt = false;

        public SubscribeToCharacteristic(string deviceAddress, string service, string characteristic, bool customGatt = false) : base(true, true)
        {
            DeviceAddress = deviceAddress;

            Service = service;
            Characteristic = characteristic;

            _customGatt = customGatt;
        }

        public SubscribeToCharacteristic(string deviceAddress, string service, string characteristic, CharacteristicChanged onDataFound, bool customGatt = false) : base(true, true)
        {
            DeviceAddress = deviceAddress;

            Service = service;
            Characteristic = characteristic;

            OnCharacteristicChanged += onDataFound;

            _customGatt = customGatt;
        }

        public override void Start()
        {
            string command = _customGatt ? "subscribeToCustomGattCharacteristic" : "subscribeToGattCharacteristic";
            BleManager.SendCommand(command, DeviceAddress, Service, Characteristic);
        }

        public override void End()
        {
            string command = _customGatt ? "unsubscribeFromCustomGattCharacteristic" : "unsubscribeFromGattCharacteristic";
            BleManager.SendCommand(command, DeviceAddress, Service, Characteristic);
        }

        public void Unsubscribe() => End();

        public override bool CommandReceived(BleObject obj)
        {
            if (string.Equals(obj.Command, "CharacteristicValueChanged"))
            {
                if (_customGatt)
                {
                    if (string.Equals(obj.Device, DeviceAddress) &&
                        string.Equals(obj.Service, DeviceAddress) &&
                        string.Equals(obj.Characteristic, Characteristic))
                    {
                        OnCharacteristicChanged?.Invoke(obj.GetByteMessage());
                    }
                }
                else
                {
                    if (string.Equals(obj.Device, DeviceAddress) &&
                        string.Equals(obj.Service, DeviceAddress) &&
                        string.Equals(obj.Characteristic, "0000" + Characteristic + "-0000-1000-8000-00805f9b34fb"))
                    {
                        OnCharacteristicChanged?.Invoke(obj.GetByteMessage());
                    }
                }
            }

            return false;
        }

        public delegate void CharacteristicChanged(byte[] value);
    }
}
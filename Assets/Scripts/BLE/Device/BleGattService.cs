using System;
using System.Collections.Generic;

namespace Android.BLE
{
    public class BleGattService
    {
        public string UUID { get; } = string.Empty;

        public BleDevice ParentDevice = null;

        public BleGattCharacteristic[] Characteristics { get; } = Array.Empty<BleGattCharacteristic>();

        private Dictionary<string, BleGattCharacteristic> _characteristicsMap = new Dictionary<string, BleGattCharacteristic>();


        internal BleGattService(string uuid, BleGattCharacteristic[] characteristics, BleDevice device)
        {
            UUID = uuid;
            Characteristics = characteristics;
            ParentDevice = device;

            foreach (BleGattCharacteristic characteristic in characteristics)
            {
                _characteristicsMap.Add(characteristic.UUID, characteristic);
            }
        }


        public BleGattCharacteristic GetCharacteristic(string characteristicUuid)
        {
            // If a shorthand UUID is passed
            if (characteristicUuid.Length == 4)
            {
                characteristicUuid = "0000" + characteristicUuid + "-0000-1000-8000-00805f9b34fb";
            }

            characteristicUuid = characteristicUuid.ToLower();

            if (!_characteristicsMap.ContainsKey(characteristicUuid))
            {
                return null;
            }

            return _characteristicsMap[characteristicUuid];
        }
    }
}

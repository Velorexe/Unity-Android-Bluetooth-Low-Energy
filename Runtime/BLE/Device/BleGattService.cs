using System;
using System.Collections.Generic;

namespace Android.BLE
{
    public class BleGattService
    {
        /// <summary>
        /// The UUID of the <see cref="BleGattService"/>.
        /// </summary>
        public string UUID { get; } = string.Empty;

        /// <summary>
        /// The <see cref="BleDevice"/> that this <see cref="BleGattCharacteristic"/> is part of.
        /// </summary>
        public BleDevice ParentDevice = null;

        /// <summary>
        /// A collection of all the <see cref="BleGattCharacteristic"/> on the <see cref="BleDevice"/>.
        /// Getting a <see cref="BleGattCharacteristic"/> should be done using the <see cref="GetCharacteristic(string)"/> method.
        /// </summary>
        public BleGattCharacteristic[] Characteristics { get; } = Array.Empty<BleGattCharacteristic>();

        /// <summary>
        /// A private collection that maps the UUID to the <see cref="BleGattCharacteristic"/>.
        /// </summary>
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

            Characteristics = characteristics;
        }

        /// <summary>
        /// Returns the <see cref="BleGattCharacteristic"/> if it's f.ound, else it returns <see langword="null"/>.
        /// Supports both 16-bit UUID's and 128-bit UUID's
        /// </summary>
        /// <returns>Returns the <see cref="BleGattCharacteristic"/> if it's found, else it returns <see langword="null"/>.</returns>
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

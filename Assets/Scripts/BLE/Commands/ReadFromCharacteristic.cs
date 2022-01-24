using System.Collections;
using UnityEngine;

namespace Android.BLE.Commands
{
    public class ReadFromCharacteristic : BleCommand
    {
        public readonly string DeviceAddress;

        public readonly string Service;
        public readonly string Characteristic;

        public readonly object StringData;

        public readonly bool CustomGatt;

        public ReadFromCharacteristic(string deviceAddress, string serviceAddress, string characteristicAddress, bool customGatt = false) : base(false, false)
        {
            DeviceAddress = deviceAddress;
            Service = serviceAddress;
            Characteristic = characteristicAddress;

            CustomGatt = customGatt;

            _timeout = 1f;
        }

        public override void Start()
        {
            string command = CustomGatt ? "readFromCustomCharacteristic" : "readFromCharacteristic";
            BleManager.SendCommand(command, DeviceAddress, Service, Characteristic);
        }
    }
}
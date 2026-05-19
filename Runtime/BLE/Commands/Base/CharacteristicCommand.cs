using Android.BLE.Extension;

namespace Android.BLE.Commands
{

    public abstract class CharacteristicCommand : BleCommand
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
        /// Indicates if the UUID is custom (long-uuid instead of a short-hand).
        /// </summary>
        protected readonly bool CustomGatt = false;

        public CharacteristicCommand(string deviceAddress, string service, string characteristic, bool customGatt = false) : base(false)
        {
            DeviceAddress = deviceAddress;
            Service = service;
            Characteristic = characteristic;
            CustomGatt = customGatt;
        } 


        protected bool CompareCharacteristics(string device, string service, string characteristic)
        {
            if (CustomGatt)
            {
                if (string.Equals(device, DeviceAddress) &&
                    string.Equals(service, Service) &&
                    string.Equals(characteristic, Characteristic))
                {
                    return true;
                }
            }
            else
            {
                if (string.Equals(device, DeviceAddress) &&
                    string.Equals(Service, service.Get16BitUuid()) &&
                    string.Equals(Characteristic, characteristic.Get16BitUuid()))
                {
                    return true;
                }
            }
            return false;
        }
    }

}

using System.Diagnostics;
using System.Threading.Tasks;
using Android.BLE.Extension;

namespace Android.BLE.Commands
{
    /// <summary>
    /// Command to Subscribe to a BLE Device's Characteristic
    /// </summary>
    public class SubscribeToCharacteristic : CharacteristicCommand
    {

        /// <summary>
        /// The .NET event that sends the subscribe data back to the user.
        /// </summary>
        public readonly CharacteristicChanged OnCharacteristicChanged;


        /// <summary>
        /// Subscribes to a given BLE Characteristic.
        /// </summary>
        /// <param name="deviceAddress">The UUID of the device that the BLE should subscribe to.</param>
        /// <param name="service">The UUID of the Service that parents the Characteristic.</param>
        /// <param name="characteristic">The UUID of the Characteristic to read from.</param>
        /// <param name="customGatt"><see langword="true"/> if the GATT Characteristic UUID address is a long-hand, not short-hand.</param>
        public SubscribeToCharacteristic(string deviceAddress, string service, string characteristic, bool customGatt = false) : base(deviceAddress, service, characteristic, customGatt)
        {
        }

        /// <summary>
        /// Subscribes to a given BLE Characteristic and passes the data back to the user.
        /// </summary>
        /// <param name="deviceAddress">The UUID of the device that the BLE should subscribe to.</param>
        /// <param name="service">The UUID of the Service that parents the Characteristic.</param>
        /// <param name="characteristic">The UUID of the Characteristic to read from.</param>
        /// <param name="onDataFound">The <see cref="CharacteristicChanged"/> that will trigger if a value was updated on the Characteristic.</param>
        /// <param name="customGatt"><see langword="true"/> if the GATT Characteristic UUID address is a long-hand, not short-hand.</param>
        public SubscribeToCharacteristic(string deviceAddress, string service, string characteristic, CharacteristicChanged onDataFound, bool customGatt = false) : base(deviceAddress, service, characteristic, customGatt)
        {
            OnCharacteristicChanged += onDataFound;
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
            
            if (!CompareCharacteristics(obj.Device,obj.Service,obj.Characteristic))
            {
                return false;
            }

            if (string.Equals(obj.Command, "DescriptorWrite"))
            {
                //UnityEngine.Debug.Log($"DescriptorWrite uuid={obj.Descriptor.Get16BitUuid()} val={obj.GetByteMessage()[0]} ");
                if(string.Equals(obj.Descriptor.Get16BitUuid(),"2902") && obj.GetByteMessage()[0] == 0x1)
                {
                    //UnityEngine.Debug.Log("DescriptorWrite recieved switching to parallel");
                    RunParallel = true;            
                }
            }
            else if (string.Equals(obj.Command, "CharacteristicValueChanged"))
            {
                OnCharacteristicChanged?.Invoke(obj.GetByteMessage());
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

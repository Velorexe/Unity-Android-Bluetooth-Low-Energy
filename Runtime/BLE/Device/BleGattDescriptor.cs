using System;

namespace Android.BLE
{
    public class BleGattDescriptor
    {
        /// <summary>
        /// Value used to disable notifications or indications
        /// </summary>
        private readonly byte[] DISABLE_NOTIFICATION_VALUE = new byte[] { 0x00, 0x00 };

        /// <summary>
        /// Value used to enable notification for a client configuration descriptor
        /// </summary>
        private readonly byte[] ENABLE_NOTIFICATION_VALUE = new byte[] { 0x01, 0x00 };

        /// <summary>
        /// Value used to enable indication for a client configuration descriptor
        /// </summary>
        private readonly byte[] ENABLE_INDICATION_VALUE = new byte[] { 0x02, 0x00 };

        public string UUID { get; } = string.Empty;

        public DescriptorTypes Type { get; } = DescriptorTypes.DISABLED;

        public DescriptorPermissions[] Permissions { get; } = Array.Empty<DescriptorPermissions>();


        protected BleGattDescriptor() { }


        public void SetType(DescriptorTypes newType)
        {
            if (Type == newType)
                return;

            switch (newType)
            {
                case DescriptorTypes.DISABLED:
                    break;
                case DescriptorTypes.NOTIFICATION:
                    break;
                case DescriptorTypes.INDICATION:
                    break;
            }
            // Set Descriptor Type
        }
    }

    public enum DescriptorPermissions
    {
        PERMISSION_READ = 1,
        PERMISSION_READ_ENCRYPTED = 2,
        PERMISSION_READ_ENCRYPTED_MITM = 4,

        PERMISSION_WRITE = 16,
        PERMISSION_WRITE_ENCRYPTED = 32,
        PERMISSION_WRITE_ENCRYPTED_MITM = 64,

        PERMISSION_WRITE_SIGNED = 128,
        PERMISSION_WRITE_SIGNED_MITM = 256,
    }

    public enum DescriptorTypes
    {
        DISABLED = 0,
        NOTIFICATION = 1,
        INDICATION = 2,
    }
}

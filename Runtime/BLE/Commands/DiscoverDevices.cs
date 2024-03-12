using System;

namespace Android.BLE.Commands
{
    /// <summary>
    /// Command to start discovering BLE devices in the area.
    /// </summary>
    public class DiscoverDevices : BleCommand
    {
        /// <summary>
        /// Default time that this command will search BLE devices for (in milliseconds).
        /// </summary>
        private const int StandardDiscoverTime = 10000;

        /// <summary>
        /// The .NET event that indicates that a new BLE device is discovered.
        /// </summary>
        public readonly DeviceDiscovered OnDeviceDiscovered;

        /// <summary>
        /// The time that this command will search BLE devices for.
        /// </summary>
        private int _discoverTime;

        /// <summary>
        /// Discovers BLE devices using the given time in milliseconds.
        /// </summary>
        /// <param name="discoverTime">The amount of searching time in milliseconds that. Defaults to <see cref="StandardDiscoverTime"/>.</param>
        public DiscoverDevices(int discoverTime = StandardDiscoverTime) : base(true, false)
        {
            _discoverTime = discoverTime;
        }

        /// <summary>
        /// Discovers BLE devices using the given time in milliseconds and
        /// passes the UUID of the discovered devices.
        /// </summary>
        /// <param name="onDeviceDiscovered">The <see cref="DeviceDiscovered"/> that will trigger if a device is discovered.</param>
        /// <param name="discoverTime">The amount of searching time in milliseconds that. Defaults to <see cref="StandardDiscoverTime"/>.</param>
        public DiscoverDevices(Action<string, string> onDeviceDiscovered, int discoverTime = StandardDiscoverTime) : base(true, false)
        {
            OnDeviceDiscovered += new DeviceDiscovered(onDeviceDiscovered);
            _discoverTime = discoverTime;
        }

        public override void Start() => BleManager.SendCommand("scanBleDevices", _discoverTime);

        public override void End() => BleManager.SendCommand("stopScanBleDevices");

        public override bool CommandReceived(BleObject obj)
        {
            if (string.Equals(obj.Command, "DiscoveredDevice"))
                OnDeviceDiscovered?.Invoke(obj.Device, obj.Name);

            return string.Equals(obj.Command, "FinishedDiscovering");
        }

        /// <summary>
        /// A delegate that indicates a newly discovered BLE device.
        /// </summary>
        /// <param name="deviceAddress">The UUID of the BLE device.</param>
        /// <param name="deviceName">The (nick)name of the BLE device.</param>
        public delegate void DeviceDiscovered(string deviceAddress, string deviceName);
    }
}
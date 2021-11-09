using System;

namespace Android.BLE.Commands
{
    public class DiscoverDevices : BleCommand
    {
        private const int StandardDiscoverTime = 10000;

        public readonly DeviceDiscovered OnDeviceDiscovered;
        private int _discoverTime;

        public DiscoverDevices(int discoverTime = StandardDiscoverTime) : base(true, false)
        {
            _discoverTime = discoverTime;
        }

        public DiscoverDevices(Action<string, string> onDeviceDiscovered, int discoverTime = StandardDiscoverTime) : base(true, false)
        {
            OnDeviceDiscovered += new DeviceDiscovered(onDeviceDiscovered);
            _discoverTime = discoverTime;
        }

        public override void Start(BleManager callBack) => callBack.SendCommand("scanBleDevices", _discoverTime);

        public override void End(BleManager callBack) => callBack.SendCommand("stopScanBleDevices");

        public override bool CommandReceived(BleObject obj)
        {
            if (string.Equals(obj.Command, "DiscoveredDevice"))
                OnDeviceDiscovered?.Invoke(obj.Device, obj.Name);

            return string.Equals(obj.Command, "FinishedDeiscovering");
        }

        public delegate void DeviceDiscovered(string deviceAddress, string deviceName);
    }
}
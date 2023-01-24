using System;
using static Android.BLE.Commands.ConnectToDevice;

namespace Android.BLE.Commands
{
    public class DiscoverDevices : BleCommand
    {
        private const int StandardDiscoverTime = 10000;

        public readonly DeviceDiscovered OnDeviceDiscovered;
        public readonly ServiceDiscovered OnServiceDiscovered;

        private readonly int _discoverTime;

        private readonly bool _scanServices = false;

        #region Constructors
        public DiscoverDevices(int discoverTime = StandardDiscoverTime) : base(true, false)
        {
            _discoverTime = discoverTime;
        }

        public DiscoverDevices(Action<string, string> onDeviceDiscovered, int discoverTime = StandardDiscoverTime) : base(true, false)
        {
            OnDeviceDiscovered += new DeviceDiscovered(onDeviceDiscovered);

            _discoverTime = discoverTime;
            _scanServices = false;
        }

        public DiscoverDevices(
            Action<string, string> onDeviceDiscovered,
            Action<string, string> onServiceDiscovered,
            int discoverTime = StandardDiscoverTime) : base(true, false)
        {
            OnDeviceDiscovered += new DeviceDiscovered(onDeviceDiscovered);
            OnServiceDiscovered += new ServiceDiscovered(onServiceDiscovered);

            _discoverTime = discoverTime;
            _scanServices = true;
        }
        #endregion

        public override void Start() => BleManager.SendCommand("scanBleDevices", _discoverTime, _scanServices);

        public override void End() => BleManager.SendCommand("stopScanBleDevices");

        public override bool CommandReceived(BleObject obj)
        {
            switch (obj.Command)
            {
                case "DiscoveredDevice":
                    OnDeviceDiscovered?.Invoke(obj.Device, obj.Name);
                    break;
                case "DiscoveredDeviceService":
                    OnServiceDiscovered?.Invoke(obj.Device, obj.Service);
                    break;
            }

            return string.Equals(obj.Command, "FinishedDiscovering");
        }

        public delegate void DeviceDiscovered(string deviceAddress, string deviceName);
    }
}
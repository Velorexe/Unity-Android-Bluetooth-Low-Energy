using Android.BLE;
using UnityEngine;

public class ExampleBleInteractor : MonoBehaviour
{
    [SerializeField]
    private GameObject _deviceButton;
    [SerializeField]
    private Transform _deviceList;

    private bool _isScanning = false;

    public void ScanForDevices()
    {
        if (!_isScanning)
        {
            _isScanning = true;
            BleManager.Instance.SearchForDevicesWithFilter(10 * 1000, OnDeviceFound, deviceName: "Testing Device");
        }
    }

    private void OnDeviceFound(BleDevice device)
    {
        DeviceButton button = Instantiate(_deviceButton, _deviceList).GetComponent<DeviceButton>();
        button.Show(device);
    }
}

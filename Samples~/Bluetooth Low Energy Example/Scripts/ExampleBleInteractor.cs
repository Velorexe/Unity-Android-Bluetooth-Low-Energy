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
            BleManager.Instance.SearchForDevices(10 * 1000, OnDeviceFound);
        }
    }

    private void OnDeviceFound(BleDevice device)
    {
        DeviceRowView button = Instantiate(_deviceButton, _deviceList).GetComponent<DeviceRowView>();
        button.Show(device);
    }
}

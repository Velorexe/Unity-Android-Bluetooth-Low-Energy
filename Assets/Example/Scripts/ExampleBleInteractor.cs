using UnityEngine;
using Android.BLE;
using Android.BLE.Commands;
using UnityEngine.Android;

public class ExampleBleInteractor : MonoBehaviour
{
    [SerializeField]
    private GameObject _deviceButton;
    [SerializeField]
    private Transform _deviceList;

    [SerializeField]
    private int _scanTime = 10000;

    private float _scanTimer = 0f;

    private bool _isScanning = false;

    public void ScanForDevices()
    {
        if (!_isScanning)
        {
            BleManager.Instance.QueueCommand(new DiscoverDevices(OnDeviceFound, _scanTime));
            _isScanning = true;
        }
        else
        {
            _scanTimer += Time.deltaTime;
            if (_scanTimer > _scanTime)
            {
                _scanTimer = 0f;
                _isScanning = false;
            }
        }
    }

    private void OnDeviceFound(string name, string device)
    {
        DeviceButton button = Instantiate(_deviceButton, _deviceList).GetComponent<DeviceButton>();
        button.Show(name, device);
    }
}

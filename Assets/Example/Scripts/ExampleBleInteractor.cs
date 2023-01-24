using Android.BLE;
using Android.BLE.Commands;
using System.Collections.Generic;
using UnityEngine;

public class ExampleBleInteractor : MonoBehaviour
{
    [SerializeField]
    private GameObject _deviceButton;
    [SerializeField]
    private Transform _deviceList;

    [SerializeField]
    private int _scanTime = 10;

    private float _scanTimer = 0f;

    private bool _isScanning = false;

    private Dictionary<string, DeviceButton> _deviceButtons = new Dictionary<string, DeviceButton>();

    public void ScanForDevices()
    {
        if (!_isScanning)
        {
            _isScanning = true;
            BleManager.Instance.QueueCommand(new DiscoverDevices(OnDeviceFound, OnDeviceServiceFound, _scanTime * 1000));
        }
    }

    private void Update()
    {
        if (_isScanning)
        {
            _scanTimer += Time.deltaTime;
            if (_scanTimer > _scanTime)
            {
                _scanTimer = 0f;
                _isScanning = false;
            }
        }
    }

    private void OnDeviceFound(string device, string name)
    {
        if (device != null)
        {
            DeviceButton button = Instantiate(_deviceButton, _deviceList).GetComponent<DeviceButton>();
            button.Show(device, name);

            _deviceButtons.Add(device, button);
        }
    }

    private void OnDeviceServiceFound(string device, string service)
    {
        Debug.Log($"Found device's ({device}) service ({service})");
        Debug.Log($"Dictionary is filled with {_deviceButtons.Count} entries");
        if (device != null && _deviceButtons.ContainsKey(device))
        {
            _deviceButtons[device].AddService(service);
        }
    }
}

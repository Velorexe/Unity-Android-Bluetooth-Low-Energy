using Android.BLE;
using UnityEngine;
using UnityEngine.UI;

public class DeviceButton : MonoBehaviour
{
    private string _deviceUuid = string.Empty;
    private string _deviceName = string.Empty;

    [SerializeField]
    private Text _deviceUuidText;
    [SerializeField]
    private Text _deviceNameText;

    [SerializeField]
    private Image _deviceButtonImage;
    [SerializeField]
    private Text _deviceButtonText;

    [SerializeField]
    private Color _onConnectedColor;
    private Color _previousColor;

    private bool _isConnected = false;

    private BleDevice _bleDevice;

    public void Show(BleDevice device)
    {
        _deviceButtonText.text = "Connect";

        _deviceUuid = device.MacAddress;
        _deviceName = device.Name;

        _deviceUuidText.text = device.MacAddress;
        _deviceNameText.text = device.Name;

        _bleDevice = device;
    }

    public void Connect()
    {
        _bleDevice.Connect(OnConnected, OnDisconnected);
    }

    private void OnConnected(BleDevice device)
    {
        _previousColor = _deviceButtonImage.color;
        _deviceButtonImage.color = _onConnectedColor;

        _isConnected = true;
        _deviceButtonText.text = "Disconnect";
    }

    private void OnDisconnected(BleDevice device)
    {
        _deviceButtonImage.color = _previousColor;

        _isConnected = false;
        _deviceButtonText.text = "Connect";
    }
}

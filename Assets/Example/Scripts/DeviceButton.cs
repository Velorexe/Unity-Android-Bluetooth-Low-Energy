using Android.BLE;
using System.Text;
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
    private Text _deviceRssiText;

    [SerializeField]
    private Image _deviceButtonImage;
    [SerializeField]
    private Text _deviceButtonText;

    [SerializeField]
    private Color _onConnectedColor;
    private Color _previousColor;

    private bool _isConnected = false;

    private BleDevice _bleDevice;

    private float _rssiTimer = 0f;

    public void Show(BleDevice device)
    {
        _deviceButtonText.text = "Connect";

        _deviceUuid = device.MacAddress;
        _deviceName = device.Name;

        _deviceUuidText.text = device.MacAddress;
        _deviceNameText.text = device.Name;

        _bleDevice = device;
    }

    public void Update()
    {
        _rssiTimer += Time.deltaTime;
        if (_rssiTimer > 0.5f)
        {
            _rssiTimer = 0f;
            _bleDevice.GetRssi((_, rsi) => _deviceRssiText.text = rsi + " dBm");
        }
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

        device.GetCharacteristic("180C", "2A56").Subscribe((value) =>
        {
            Debug.Log(Encoding.UTF8.GetString(value));
        });
    }

    private void OnDisconnected(BleDevice device)
    {
        _deviceButtonImage.color = _previousColor;

        _isConnected = false;
        _deviceButtonText.text = "Connect";
    }
}

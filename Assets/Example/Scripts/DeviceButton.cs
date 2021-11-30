using UnityEngine;
using UnityEngine.UI;
using Android.BLE;
using Android.BLE.Commands;

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
    private Color _onConnectedColor;
    private Color _previousColor;

    public void Show(string uuid, string name)
    {
        _deviceUuid = uuid;
        _deviceName = name;

        _deviceUuidText.text = uuid;
        _deviceNameText.text = name;
    }

    public void Connect()
    {
        BleManager.Instance.QueueCommand(new ConnectToDevice(_deviceUuid, OnConnected, OnDisconnected));
    }

    private void OnConnected(string deviceUuid)
    {
        _previousColor = _deviceButtonImage.color;
        _deviceButtonImage.color = _onConnectedColor;
    }

    private void OnDisconnected(string deviceUuid)
    {
        _deviceButtonImage.color = _previousColor;
    }
}

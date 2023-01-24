using Android.BLE;
using Android.BLE.Commands;
using System.Collections.Generic;
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
    private Text _deviceServicesAmountText;

    [SerializeField]
    private Image _deviceButtonImage;
    [SerializeField]
    private Text _deviceButtonText;

    [SerializeField]
    private Color _onConnectedColor;
    private Color _previousColor;

    private List<string> _services = new List<string>();

    private bool _isConnected = false;

    private ConnectToDevice _connectCommand;
    private SubscribeToCharacteristic _subscribeToCharacteristic;

    public void Show(string uuid, string name)
    {
        _deviceButtonText.text = "Connect";

        _deviceUuid = uuid;
        _deviceName = name;

        _deviceUuidText.text = uuid;
        _deviceNameText.text = name;
    }

    public void Connect()
    {
        if (!_isConnected)
        {
            _connectCommand = new ConnectToDevice(_deviceUuid, OnConnected, OnDisconnected);
            BleManager.Instance.QueueCommand(_connectCommand);
        }
        else
        {
            _subscribeToCharacteristic.Unsubscribe();
            _connectCommand.Disconnect();
        }
    }

    public void SubscribeToExampleService()
    {
        //Replace these Characteristics with YOUR device's characteristics
        _subscribeToCharacteristic = new SubscribeToCharacteristic(_deviceUuid, "1101", "2101");
        BleManager.Instance.QueueCommand(_subscribeToCharacteristic);
    }

    public void AddService(string service)
    {
        _services.Add(service);
        _deviceServicesAmountText.text = "Amount of services: " + _services.Count;
    }

    private void OnConnected(string deviceUuid)
    {
        _previousColor = _deviceButtonImage.color;
        _deviceButtonImage.color = _onConnectedColor;

        _isConnected = true;
        _deviceButtonText.text = "Disconnect";

        SubscribeToExampleService();
    }

    private void OnDisconnected(string deviceUuid)
    {
        _deviceButtonImage.color = _previousColor;

        _isConnected = false;
        _deviceButtonText.text = "Connect";
    }
}

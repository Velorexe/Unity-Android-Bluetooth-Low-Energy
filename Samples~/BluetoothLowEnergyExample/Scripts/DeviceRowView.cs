using Android.BLE;
using Android.BLE.Commands;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

public class DeviceRowView : MonoBehaviour
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

    [SerializeField] 
    private Button _buttonComponent;
    private Color _previousColor;

    private bool _isConnected = false;

    private ConnectToDevice _connectCommand;
    private ReadFromCharacteristic _readFromCharacteristic;

    public void Show(string uuid, string name)
    {
        _deviceButtonText.text = "Connect";

        _deviceUuid = uuid;
        _deviceName = name;

        _deviceUuidText.text = uuid;
        _deviceNameText.text = name;

        _buttonComponent.onClick.AddListener(ToggleConnect);

        gameObject.SetActive(true);
    }

    public void ToggleConnect()
    {
        if (!_isConnected)
        {
            _connectCommand = new ConnectToDevice(_deviceUuid, OnConnected, OnDisconnected);
            BleManager.Instance.QueueCommand(_connectCommand);
        }
        else
        {
            _connectCommand.Disconnect();
        }
        _buttonComponent.interactable = false;
    }

    public void SubscribeToExampleService()
    {
        //Replace these Characteristics with YOUR device's characteristics
        _readFromCharacteristic = new ReadFromCharacteristic(_deviceUuid, "180c", "2a56", (byte[] value) =>
        {
            Debug.Log(Encoding.UTF8.GetString(value));
        });
        BleManager.Instance.QueueCommand(_readFromCharacteristic);
    }

    private void OnConnected(string deviceUuid)
    {
        _previousColor = _deviceButtonImage.color;
        _deviceButtonImage.color = _onConnectedColor;

        _isConnected = true;
        _buttonComponent.interactable = true;
        _deviceButtonText.text = "Disconnect";

        SubscribeToExampleService();
    }

    private void OnDisconnected(string deviceUuid)
    {
        _deviceButtonImage.color = _previousColor;
        _buttonComponent.interactable = true;
        _isConnected = false;
        _deviceButtonText.text = "Connect";
    }
}

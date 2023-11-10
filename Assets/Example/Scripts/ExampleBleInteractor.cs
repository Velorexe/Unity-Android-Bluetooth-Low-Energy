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
    private int _scanTime = 10;

    private float _scanTimer = 0f;

    private bool _isScanning = false;
    private bool hasAllPermision=false;
    private void Start()
    {
        Debug.Log(Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN"));
    }

    public void RequestScanPermission()
    {
        var per = Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN");
        Debug.Log(per);
        if(!per)
        {
            Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");
        }
        
    }
    public void RequestConnectPermission()
    {
        var per = Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT");
        Debug.Log(per);
        if (!per)
        {
            Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");
        }
    }
    public void RequestBoth()
    {
        Permission.RequestUserPermissions(new string[] { "android.permission.BLUETOOTH_SCAN", "android.permission.BLUETOOTH_CONNECT" });

    }
    public void ScanForDevices()
    {
        if (!hasAllPermision)
        {
            if (Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_SCAN"))
            {
                if (Permission.HasUserAuthorizedPermission("android.permission.BLUETOOTH_CONNECT"))
                {
                    hasAllPermision = true;
                    if (!_isScanning)
                    {
                        _isScanning = true;
                        BleManager.Instance.QueueCommand(new DiscoverDevices(OnDeviceFound, _scanTime * 1000));
                    }
                }
                else
                {
                    Permission.RequestUserPermission("android.permission.BLUETOOTH_CONNECT");
                    Debug.Log("Permission needed");
                    Debug.Log("Please click again to scan");
                }
            }
            else
            {
                Debug.Log("Permission needed");
                Permission.RequestUserPermission("android.permission.BLUETOOTH_SCAN");
            }
        }
        else
        {
            if (!_isScanning)
            {
                _isScanning = true;
                BleManager.Instance.QueueCommand(new DiscoverDevices(OnDeviceFound, _scanTime * 1000));
            }
        }
        
    }

    private void Update()
    {
        if(_isScanning)
        {
            _scanTimer += Time.deltaTime;
            if(_scanTimer > _scanTime)
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

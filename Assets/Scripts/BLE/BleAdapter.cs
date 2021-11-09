using UnityEngine;
using System;
using Android.BLE.Events;

namespace Android.BLE
{
    public class BleAdapter : MonoBehaviour
    {
        // .NET Events
        public event EventHandler<BleObject> OnMessageReceived;
        public event EventHandler<string> OnErrorReceived;

        // Unity Events
        public BleMessageReceived UnityOnMessageReceived;
        public BleErrorReceived UnityOnErrorReceived;

        private void Awake() => gameObject.name = "BleAdapter";

        public void OnBleMessage(string jsonMessage)
        {
            BleObject obj = JsonUtility.FromJson<BleObject>(jsonMessage);
            if (obj.HasError)
            {
                OnErrorReceived?.Invoke(this, obj.ErrorMessage);
                UnityOnErrorReceived?.Invoke(obj.ErrorMessage);
            }
            else
            {
                OnMessageReceived?.Invoke(this, obj);
                UnityOnMessageReceived?.Invoke(obj);
            }
        }
    }
}
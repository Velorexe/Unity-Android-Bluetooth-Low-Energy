using UnityEngine;
using System;
using Android.BLE.Events;

namespace Android.BLE
{
    public class BleAdapter : MonoBehaviour
    {
        // .NET Events
        public event MessageReceived OnMessageReceived;
        public event ErrorReceived OnErrorReceived;

        // Unity Events
        public BleMessageReceived UnityOnMessageReceived;
        public BleErrorReceived UnityOnErrorReceived;

        private void Awake() => gameObject.name = "BleAdapter";

        public void OnBleMessage(string jsonMessage)
        {
            BleObject obj = JsonUtility.FromJson<BleObject>(jsonMessage);
            if (obj.HasError)
            {
                OnErrorReceived?.Invoke(obj.ErrorMessage);
                UnityOnErrorReceived?.Invoke(obj.ErrorMessage);
            }
            else
            {
                OnMessageReceived?.Invoke(obj);
                UnityOnMessageReceived?.Invoke(obj);
            }
        }

        public void LogMessage(string log) => Debug.Log(log);

        public delegate void MessageReceived(BleObject obj);
        public delegate void ErrorReceived(string errorMessage);
    }
}
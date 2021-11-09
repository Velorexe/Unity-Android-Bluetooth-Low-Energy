using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Android.BLE
{
    public class BleAdapter : MonoBehaviour
    {
        public event EventHandler<BleObject> OnMessageReceived;
        public event EventHandler<string> OnErrorReceived;

        public void OnBleMessage(string jsonMessage)
        {
            BleObject obj = JsonUtility.FromJson<BleObject>(jsonMessage);
            if (obj.HasError)
                OnErrorReceived?.Invoke(this, obj.ErrorMessage);
            else
                OnMessageReceived?.Invoke(this, obj);
        }
    }
}
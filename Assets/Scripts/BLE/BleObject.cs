using System.Collections;
using UnityEngine;
using System;

namespace Android.BLE
{
    [Serializable]
    public class BleObject
    {
        public bool HasError { get => hasError; }
        [SerializeField]
        private bool hasError = false;
        
        public string ErrorMessage { get => errorMessage; }
        [SerializeField]
        private string errorMessage = string.Empty;

        public object ConvertMessage<T>()
        {
            return null;
        }

        public override string ToString() => JsonUtility.ToJson(this, true);
    }
}
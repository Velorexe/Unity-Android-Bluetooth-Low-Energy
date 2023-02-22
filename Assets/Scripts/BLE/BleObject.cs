using System;
using UnityEngine;

namespace Android.BLE
{
    /// <summary>
    /// The JSON information that gets send from the Java library
    /// </summary>
    [Serializable]
    public class BleObject
    {
        #region Device Information
        public string Device => device;
        [SerializeField]
        private string device;

        public string Name => name;
        [SerializeField]
        private string name;

        public string Service => service;
        [SerializeField]
        private string service;

        public string Characteristic => characteristic;
        [SerializeField]
        private string characteristic;
        #endregion

        #region Command Information
        public string Command => command;
        [SerializeField]
        private string command;
        #endregion

        #region Error Information
        public bool HasError { get => hasError; }
        [SerializeField]
        private bool hasError = false;

        public string ErrorMessage { get => errorMessage; }
        [SerializeField]
        private string errorMessage = string.Empty;
        #endregion

        public string Base64Message { get => base64Message; }
        [SerializeField]
        private string base64Message = string.Empty;

        public byte[] GetByteMessage() => Convert.FromBase64String(base64Message);

        public override string ToString() => JsonUtility.ToJson(this, true);
    }
}
using System;
using UnityEngine;

namespace Android.BLE
{
    [Serializable]
    public class BleMessage
    {
        public string ID { get { return id; } }
        [SerializeField]
        private string id = string.Empty;

        public string Command { get { return command; } }
        [SerializeField]
        private string command = string.Empty;

        public string Device { get { return device; } }
        [SerializeField]
        private string device = string.Empty;

        public string Name { get { return name; } }
        [SerializeField]
        private string name = string.Empty;

        public string Base64Data { get { return base64Data; } }
        [SerializeField]
        private string base64Data = string.Empty;

        public string JsonData { get { return jsonData; } }
        [SerializeField]
        private string jsonData = string.Empty;

        public bool HasError { get { return hasError; } }
        [SerializeField]
        private bool hasError = false;

        public string ErrorMessage { get { return errorMessage; } }
        [SerializeField]
        private string errorMessage = string.Empty;
    }
}

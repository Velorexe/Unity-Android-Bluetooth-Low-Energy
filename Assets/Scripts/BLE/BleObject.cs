using System.Collections;
using UnityEngine;
using System;

namespace Android.BLE
{
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

        public override string ToString() => JsonUtility.ToJson(this, true);
    }
}
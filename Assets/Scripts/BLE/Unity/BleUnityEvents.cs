using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using System;

namespace Android.BLE.Events
{
    [Serializable]
    public class BleMessageReceived : UnityEvent<BleObject> { }

    [Serializable]
    public class BleErrorReceived : UnityEvent<string> { }
}
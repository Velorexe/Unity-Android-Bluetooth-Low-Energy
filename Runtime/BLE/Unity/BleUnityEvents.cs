using System;
using UnityEngine.Events;

namespace Android.BLE.Events
{
    /// <summary>
    /// A Unity event to hook onto using the Editor instead of using .NET events.
    /// </summary>
    [Serializable]
    public class BleMessageReceived : UnityEvent<BleObject> { }

    /// <summary>
    /// The equivalent of <see cref="BleMessageReceived"/> but then for errors.
    /// </summary>
    [Serializable]
    public class BleErrorReceived : UnityEvent<string> { }
}
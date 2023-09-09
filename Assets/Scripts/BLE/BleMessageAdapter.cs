using UnityEngine;

namespace Android.BLE
{
    public class BleMessageAdapter : MonoBehaviour
    {
        [SerializeField]
        private bool _logBleMessages = false;

        // .NET Events
        public event MessageReceived OnMessageReceived;
        public event ErrorReceived OnErrorReceived;

        /// <summary>
        /// Sets the name to "BleAdapter" to receive messages from the Java library.
        /// </summary>
        private void Awake() => gameObject.name = nameof(BleMessageAdapter);

        /// <summary>
        /// The method that the Java library will send their JSON messages to.
        /// </summary>
        /// <param name="jsonMessage">The <see cref="BleObject"/> in JSON format.</param>
        public void OnBleMessage(string jsonMessage)
        {
            BleMessage obj = JsonUtility.FromJson<BleMessage>(jsonMessage);

            if (_logBleMessages)
            {
                LogMessage("Received JSON message: ");
                LogMessage(jsonMessage);
            }

            if (obj.HasError)
            {
                OnErrorReceived?.Invoke(obj.ErrorMessage);
            }
            else
            {
                OnMessageReceived?.Invoke(obj);
            }
        }

        public void LogMessage(string log) => Debug.Log(log);

        public delegate void MessageReceived(BleMessage obj);
        public delegate void ErrorReceived(string errorMessage);
    }
}

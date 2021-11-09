using System.Collections;
using UnityEngine;

namespace Android.BLE
{
    public class BleManager : MonoBehaviour
    {
        public static BleManager Instance;

        [SerializeField]
        private BleAdapter _adapter;

        private void Awake()
        {
            Instance = this;

            if(_adapter == null)
            {
                _adapter = FindObjectOfType<BleAdapter>();
                if(_adapter == null)
                {
                    GameObject bleAdapter = new GameObject(typeof(BleAdapter).Name);
                    bleAdapter.transform.SetParent(this.transform);

                    _adapter = bleAdapter.AddComponent<BleAdapter>();
                }
            }
        }
    }
}
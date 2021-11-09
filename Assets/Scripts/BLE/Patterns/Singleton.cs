using System.Collections;
using UnityEngine;
using System;

namespace Assets.Scripts.BLE.Patterns
{
    //https://blog.mzikmund.com/2019/01/a-modern-singleton-in-unity/
    internal abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance => LazyInstance.Value;

        private static readonly Lazy<T> LazyInstance = new Lazy<T>(CreateSingleton);

        private static T CreateSingleton()
        {
            var ownerObject = new GameObject($"{typeof(T).Name} (singleton)");
            var instance = ownerObject.AddComponent<T>();
            DontDestroyOnLoad(ownerObject);
            return instance;
        }
    }
}
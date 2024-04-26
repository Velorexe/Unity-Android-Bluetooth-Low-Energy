using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
namespace Android.BLE
{

    public class AppPermissions : MonoBehaviour
    {
        [System.Serializable]
        public class PermissionData
        {
            public string name;
            public bool exitOnUserDenied;
            [NonSerialized] public bool userAuthorizedPermission;

            [NonSerialized] public bool gotPermissionResponse;

            public int minApiVersion = 31;
        }

        [SerializeField]
        PermissionData[] m_permissions = new PermissionData[]{
        new PermissionData(){name="android.permission.BLUETOOTH_SCAN",minApiVersion=31,exitOnUserDenied=true,userAuthorizedPermission=false},
        new PermissionData(){name="android.permission.BLUETOOTH_CONNECT",minApiVersion=31,exitOnUserDenied=true,userAuthorizedPermission=false}
    };

        public UnityEvent allPermissionsGrantedEvent;
        public UnityEvent<string> permissionDeniedEvent;
        public UnityEvent somePermissionsDeniedEvent;

        public static AppPermissions Instance
        {
            private set;
            get;
        }

        [SerializeField] bool _checkPermissionsOnStart;

        void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            if (_checkPermissionsOnStart)
            {
                CheckPermissions();
            }

        }

        public void CheckPermissions()
        {
            StartCoroutine(CheckPermissionsCoroutine());
        }
#if UNITY_2020_1_OR_NEWER
        IEnumerator CheckPermissionsCoroutine()
        {
            Dictionary<string, PermissionData> permissions = new Dictionary<string, PermissionData>();
            int apiVersion = getAPIVersion();
            //Debug.Log($"getAPIVersion={getAPIVersion()}");

            foreach (var permission in m_permissions)
            {
                if (apiVersion < permission.minApiVersion)
                {
                    continue;
                }
                if (permission.userAuthorizedPermission = Permission.HasUserAuthorizedPermission(name))
                {
                    continue;
                }
                permissions[permission.name] = permission;

            }
            var Callbacks = new PermissionCallbacks();
            Callbacks.PermissionDenied += (string permissionName) =>
            {
                Debug.LogWarning($"Permission {permissionName} denied");
                permissions[permissionName].gotPermissionResponse = true;
                if (permissions[permissionName].exitOnUserDenied)
                {
                    Application.Quit();
                }
            };
            Callbacks.PermissionDeniedAndDontAskAgain += (string permissionName) =>
            {
                Debug.LogWarning($"Permission {permissionName} denied and don't ask again");
                permissions[permissionName].gotPermissionResponse = true;
                if (permissions[permissionName].exitOnUserDenied)
                {
                    Application.Quit();
                }
            };
            Callbacks.PermissionGranted += (string permissionName) =>
            {
                Debug.LogWarning($"Permission {permissionName} granted");
                permissions[permissionName].gotPermissionResponse = true;
                permissions[permissionName].userAuthorizedPermission = true;
            };


            // Debug.Log($"Requesting permissions");
            // foreach(var permission in permissions.Keys){
            //      Debug.Log($"{permission}");
            // } 

            Permission.RequestUserPermissions(permissions.Keys.ToArray(), Callbacks);

            yield return new WaitUntil(() =>
            {
                return permissions.All((pair) => pair.Value.gotPermissionResponse);
            });

            if (permissions.All(pair => pair.Value.userAuthorizedPermission))
            {
                Debug.Log("Permissions granted");

                allPermissionsGrantedEvent.Invoke();
            }
            else
            {
                foreach (var permission in permissions)
                {
                    if (permission.Value.userAuthorizedPermission)
                    {
                        continue;
                    }
                    permissionDeniedEvent.Invoke(permission.Key);

                }
                somePermissionsDeniedEvent.Invoke();
            }

        }

        static int getAPIVersion()
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }

#else
    /*
        Bypass runtime permissions if running on 2019.4 or earlier
    */
    IEnumerator CheckPermissionsCoroutine()
    {
        allPermissionsGrantedEvent.Invoke();
    }
#endif
    
    }

}
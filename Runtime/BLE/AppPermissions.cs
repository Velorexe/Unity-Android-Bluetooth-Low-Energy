using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;
using UnityEngine.Events;
namespace Android.BLE
{

    /// <summary>
    /// Discovering and conecting to BLE devices on Android requires rumtime permssions as of API 26
    /// This class will prompt with the correct permissions, if neccessary, and will fire an event, once permissions have been granted
    /// </summary>
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

        const int MIN_SDK_RUNTIME_PERMISSION = 23;

        public UnityEvent allPermissionsGrantedEvent;
        public UnityEvent<string> permissionDeniedEvent;
        public UnityEvent somePermissionsDeniedEvent;

        public static AppPermissions Instance
        {
            private set;
            get;

        }

        [SerializeField] bool _checkPermissionsOnStart;

        [System.Serializable]
        public class ErrorMessageEvent:UnityEvent<string>
        {

        }

        void Awake()
        {
            // Allow for this script to be added by GameObject.AddComponent 
            // All UnityEvents are null by default if not Serialized by the Editor
            if(allPermissionsGrantedEvent==null){
                allPermissionsGrantedEvent = new UnityEvent();
            }
            if(permissionDeniedEvent==null){
                permissionDeniedEvent = new ErrorMessageEvent();                
            }
            if(somePermissionsDeniedEvent==null){
                somePermissionsDeniedEvent = new UnityEvent();                
            }
            
            Instance = this;
        }

        void Start()
        {
            if (_checkPermissionsOnStart)
            {
                RequestPermissions();
            }
        }

        public bool AllPermissionsGranted
        {
            get
            {
                int apiVersion = getAPIVersion();
                if (apiVersion < MIN_SDK_RUNTIME_PERMISSION)
                {
                    return true;
                }
                return m_permissions.All(perm => perm.userAuthorizedPermission || (apiVersion < perm.minApiVersion) || Permission.HasUserAuthorizedPermission(perm.name));
            }
        }

        public void RequestPermissions()
        {
            StartCoroutine(CheckPermissionsCoroutine());
        }
#if UNITY_2020_1_OR_NEWER
        IEnumerator CheckPermissionsCoroutine()
        {
#if UNITY_EDITOR
            yield return null;
            allPermissionsGrantedEvent.Invoke();
            yield break;
#endif

            Dictionary<string, PermissionData> permissions = new Dictionary<string, PermissionData>();
            int apiVersion = getAPIVersion();

            if (apiVersion < MIN_SDK_RUNTIME_PERMISSION)
            {
                allPermissionsGrantedEvent.Invoke();
                yield break;
            }

            foreach (var permission in m_permissions)
            {
                if (apiVersion < permission.minApiVersion)
                {
                    continue;
                }
                if (permission.userAuthorizedPermission = Permission.HasUserAuthorizedPermission(permission.name))
                {
                    continue;
                }
                permissions[permission.name] = permission;

            }
            if (permissions.Count == 0)
            {
                allPermissionsGrantedEvent?.Invoke();
                yield break;
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
                Debug.Log($"Permission {permissionName} granted");
                permissions[permissionName].gotPermissionResponse = true;
                permissions[permissionName].userAuthorizedPermission = true;
            };

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

#else
    /*
        Bypass runtime permissions if running on 2019.4 or earlier
    */
    IEnumerator CheckPermissionsCoroutine()
    {
        yield return null;
        allPermissionsGrantedEvent.Invoke();
        yield break;
    }
#endif
    static int getAPIVersion()
    {
        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
        {
            return version.GetStatic<int>("SDK_INT");
        }
    }

    }

}
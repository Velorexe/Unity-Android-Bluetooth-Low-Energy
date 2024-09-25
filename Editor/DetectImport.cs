using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Android.BLE.UnityEditor 
{
        
    public class DetectImport : AssetPostprocessor
    {
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths, bool didDomainReload)
        {            

            if (didDomainReload)
            {
                //Debug.Log("Domain has been reloaded");
                PostInstallWizard.RefreshFiles();
            }

            if(importedAssets.Contains("Packages/com.velorexe.androidbluetoothlowenergy/Plugins/Android/AndroidManifest.xml")){
                //Debug.Log("Package imported");
                PostInstallWizard.OpenWindow();
            }

            if(importedAssets.Contains("Assets/Plugins/Android/AndroidManifest.xml")){
                PostInstallWizard.RefreshFiles();
            }
        }
    }

}
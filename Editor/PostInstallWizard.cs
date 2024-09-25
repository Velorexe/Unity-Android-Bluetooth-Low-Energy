using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Android.BLE.UnityEditor 
{

    public class PostInstallWizard : EditorWindow
    {
        const string packageName = "com.velorexe.androidbluetoothlowenergy";
        static string assetsAndroidManifestPath;
        static string packageAndroidManifestPath = Path.GetFullPath($"Packages/{packageName}/Plugins/Android/AndroidManifest.xml");
        static bool hasExistingManifest;

        static bool forcePreAndroidSDK31 = false;
        static string existingManifestContents;


        Texture m_SuccessImage;
        Texture m_FailImage;

        private class AndroidPermission {
            public Regex regex;
            public string description;
            public string line;

            public bool foldOutOpen;

            public int minSdkVersion;

            public bool Exists(string manifest){
                return regex.IsMatch(manifest);
            }
        }

        AndroidPermission[] m_androidPermission = new AndroidPermission[]{
            new AndroidPermission(){
                regex = new Regex("uses-permission[^>]+android:name=\"android.permission.BLUETOOTH\""),
                description = "Legacy Bluetooth access ( pre API 31 )",
                line = "<uses-permission android:name=\"android.permission.BLUETOOTH\" android:maxSdkVersion=\"30\" />",            
            },
            new AndroidPermission(){
                regex = new Regex("uses-permission[^>]+android:name=\"android.permission.BLUETOOTH_ADMIN\""),
                description = "Legacy Bluetooth Admin access ( pre API 31 )",
                line = "<uses-permission android:name=\"android.permission.BLUETOOTH_ADMIN\" android:maxSdkVersion=\"30\" />",
            },
            new AndroidPermission(){
                regex = new Regex("uses-permission[^>]+android:name=\"android.permission.BLUETOOTH_SCAN\""),
                description = "Bluetooth Scan ( API 31 and above )",
                line = "<uses-permission android:name=\"android.permission.BLUETOOTH_SCAN\" android:usesPermissionFlags=\"neverForLocation\" />",
                minSdkVersion = 31
            },
            new AndroidPermission(){
                regex = new Regex("uses-permission[^>]+android:name=\"android.permission.BLUETOOTH_CONNECT\""),
                description = "Bluetooth Connect ( API 31 and above )",
                line = "<uses-permission android:name=\"android.permission.BLUETOOTH_CONNECT\" />",
                minSdkVersion = 31
            },
        };
        

        [MenuItem("Window/Android Bluetooth Low Energy Library")]
        public static void OpenWindow()
        {
            var window = GetWindow<PostInstallWizard>(false,"BLE Library Configuration Wizard");    
            window.Show();
        }

        public PostInstallWizard():base()
        {
            RefreshFiles();
        }

        
        public void Awake()
        {
            RefreshFiles();
            EditorApplication.Beep();

            Debug.Log(PlayerSettings.Android.targetSdkVersion);
            
            Debug.Log(PlayerSettings.Android.minSdkVersion.ToString());
            
            GUIHyperlinkClick();


            GetIcons();

        }

        void GetIcons()
        {
            #if UNITY_2023_1_OR_NEWER
                m_SuccessImage = EditorGUIUtility.IconContent("TestPassed").image; //"GreenCheckmark", 
            #else
                m_SuccessImage = EditorGUIUtility.IconContent("TestPassed").image; //"GreenCheckmark", 
            #endif 
            m_FailImage = EditorGUIUtility.IconContent("console.erroricon").image;
        }

#if UNITY_2020_1_OR_NEWER
        void GUIHyperlinkClick()
        {
            EditorGUI.hyperLinkClicked += OnHyperLinkClicked;
        }

        void OnDestroy()
        {
            EditorGUI.hyperLinkClicked -= OnHyperLinkClicked;
        }

        private void OnHyperLinkClicked(EditorWindow window, HyperLinkClickedEventArgs args)
        {
            if(window!=this){
                return;
            }
            Application.OpenURL( args.hyperLinkData["href"] );
        }
#else
        void GUIHyperlinkClick()
        {
            //not availble before 2020.1
        }
#endif

        public static void RefreshFiles(){
            assetsAndroidManifestPath = Path.Combine(ProjectPath(),"Plugins/Android/AndroidManifest.xml");
            
            hasExistingManifest = File.Exists(assetsAndroidManifestPath);
            if(hasExistingManifest){
                existingManifestContents = File.ReadAllText(assetsAndroidManifestPath);
            }
            else {
                Debug.LogWarning($"File not found {assetsAndroidManifestPath}");
            }

        }

        public void OnGUI()
        {
            var height = EditorGUIUtility.singleLineHeight;
            GUIStyle style = new GUIStyle( GUI.skin.label ) { richText = true };
            GUIStyle bold = new GUIStyle(style){ fontStyle = FontStyle.Bold };
                    
            if( hasExistingManifest ){
                EditorGUILayout.LabelField("AndroidManifest.xml file found",bold);
                EditorGUILayout.LabelField("Manifest Bluetooth permissions:");
                // if targetSDK set to automatic ( value is 0 ), it's hard to determine the SDK that will be used, use minSDK instead.
                var targetSDK = (int)PlayerSettings.Android.targetSdkVersion;
                if(targetSDK==0){                    
                    EditorGUILayout.BeginHorizontal();
                    GUILayout.Label("Android API Level "); 
                    if( GUILayout.Button("API Level 31 or greater ( suggested )") ) forcePreAndroidSDK31=false;
                    if( GUILayout.Button("API Level to pre 31")) forcePreAndroidSDK31=true;
                    EditorGUILayout.EndHorizontal();
                    targetSDK = forcePreAndroidSDK31 ? 0:31;
                }

                foreach(var androidPermission in m_androidPermission){
                    if(targetSDK<androidPermission.minSdkVersion){
                        continue;
                    }
                    bool hasPerm = androidPermission.Exists(existingManifestContents);
                    EditorGUILayout.GetControlRect(GUILayout.Height(4));             
                    EditorGUILayout.BeginHorizontal();
                    //var rect = EditorGUILayout.GetControlRect(GUILayout.Width(height));
                    androidPermission.foldOutOpen = EditorGUILayout.Foldout(androidPermission.foldOutOpen, $"{androidPermission.description}");
                    //EditorGUILayout.LabelField($"{androidPermission.description} ",style);
                    var rect = EditorGUILayout.GetControlRect(GUILayout.Width(height-4));
                    rect.height = height-4;
                    rect.y+=2;
                                        
                    GUI.DrawTexture(rect,hasPerm ? m_SuccessImage : m_FailImage);
                    EditorGUILayout.EndHorizontal();
                    if(androidPermission.foldOutOpen){
                        EditorGUILayout.TextField(androidPermission.line);
                    }
                }
                
                //existingManifestContents 
            }
            else 
            {
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("No existing AndroidManifest.xml file found",bold);
                         
                if(GUILayout.Button("Use template AndroidManifest.xml")){
                    Directory.CreateDirectory(Path.Combine(ProjectPath(),"Plugins/Android"));
                    File.Copy(packageAndroidManifestPath,assetsAndroidManifestPath);
                    RefreshFiles();
                    AssetDatabase.Refresh();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.GetControlRect();
            EditorGUILayout.TextField("Click here for <a href=\"https://developer.android.com/develop/connectivity/bluetooth/bt-permissions\">more information on bluetooth permissions</a>", style);

            

            
        }

        static string ProjectPath()
        {
            return Path.Combine(System.IO.Directory.GetCurrentDirectory(),"Assets");
        }

    }
}

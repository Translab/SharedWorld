#define PKPRO_SHOW_DEBUG

using System.IO;
using SKStudios.Common.Utils.SafeRemoveComponent;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System;
using System.Collections.Generic;
using SKStudios.Common.Editor;
using SKStudios.Common.Utils;

namespace SKStudios.Portals.Editor
{


    public enum ProjectImportStatus { Unknown, Uninitialized, Initialized }
    public enum ProjectMode { None, Default, VR }

    public static class SetupUtility
    {

        public const string vrSymbol = "SKS_VR";
        public const string projectSymbol = "SKS_PORTALS";


        private const string _ignoreSetup = "pkpro_dont_show_setup";
        private const string _importingVRTK = "skspro_importing_vrtk";
        private const string _performingSetup = "skspro_performing_import";
        private const string _performingFirstTimeSetup = "skspro_performing_first_time_setup";
        private const string _timedFeedbackPopupActive = "pkpro_feedback_popup_triggered";

        static ProjectMode _projectMode;
        static ProjectImportStatus _isProjectInitialized;

        public static bool ignoringInitialSetup {
            get { return EditorPrefs.GetBool(_ignoreSetup); }
            set { EditorPrefs.SetBool(_ignoreSetup, value); }
        }

        public static bool performingSetup {
            get { return EditorPrefs.GetBool(_performingSetup); }
            set { EditorPrefs.SetBool(_performingSetup, value); }
        }

        public static bool performingFirstRunSetup {
            get { return EditorPrefs.GetBool(_performingFirstTimeSetup); }
            set { EditorPrefs.SetBool(_performingFirstTimeSetup, value); }
        }

        public static bool timedFeedbackPopupActive {
            get { return EditorPrefs.GetBool(_timedFeedbackPopupActive); }
            set { EditorPrefs.SetBool(_timedFeedbackPopupActive, value); }
        }

        // This is an int instead of a bool to account for the two-step reload process due to VRTK adding its own symbol defines and triggering an extra reload after import.
        public static int importingVRTK {
            get { return EditorPrefs.GetInt(_importingVRTK); }
            set { EditorPrefs.SetInt(_importingVRTK, value); }
        }

        // This doesn't account for a situation in which the VRTK scripting symbols are defined but the VRTK package isn't present.
        public static bool VRTKIsMaybeInstalled {
            get { return IsScriptingSymbolDefined("VRTK_VERSION_"); }
        }

        public static bool projectInitialized {
            get { return importStatus == ProjectImportStatus.Initialized; }
        }

        // We can safely cache this because any changes to the scripting symbols results in a reload of the project.
        public static ProjectImportStatus importStatus {
            get {
                if (_isProjectInitialized != ProjectImportStatus.Unknown) return _isProjectInitialized;

                if (IsScriptingSymbolDefined(projectSymbol))
                {
                    _isProjectInitialized = ProjectImportStatus.Initialized;
                }
                else
                {
                    _isProjectInitialized = ProjectImportStatus.Uninitialized;
                }

                return _isProjectInitialized;
            }
        }

        // We can safely cache this because any changes to the scripting symbols results in a reload of the project.
        public static ProjectMode projectMode {
            get {
                if (_projectMode != ProjectMode.None) return _projectMode;

                if (importStatus == ProjectImportStatus.Initialized)
                {
                    if (IsScriptingSymbolDefined(vrSymbol))
                    {
                        _projectMode = ProjectMode.VR;
                    }
                    else
                    {
                        _projectMode = ProjectMode.Default;
                    }
                }
                else
                {
                    _projectMode = ProjectMode.None;
                }

                return _projectMode;
            }
        }

        /// <summary>
        /// Makes sure the project has been properly configured, and handles properly showing the setup screen on first install.
        /// </summary>
        [DidReloadScripts]
        static void OnProjectReload()
        {
            // don't bother the user if they're playing
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                return;
            }

            try
            {
                if (Math.Abs(GlobalPortalSettings.TimeInstalled) < 100)
                {
                    GlobalPortalSettings.TimeInstalled = DateTime.UtcNow.Ticks;
                }
            }
            catch (NullReferenceException)
            {
                return;
            }


            if (projectInitialized)
            {
                if (projectMode == ProjectMode.VR)
                {
                    // check to see if we're currently trying to load vrtk
                    var importingStep = importingVRTK;
                    if (importingStep > 0)
                    {
                        if (importingStep == 2)
                        {
                            // has vrtk been imported successfully?
                            if (VRTKIsMaybeInstalled)
                            {

                                // first reload of vrtk, wait out vrtk adding its own compile symbols
                                importingVRTK = 1;
                            }
                            else
                            {
                                // vrtk probably didn't install/run correctly... maybe detect improper import later
                                importingVRTK = 0;
                                CheckImportFlags();
                            }
                        }
                        else
                        {
                            // second reload of vrtk, check the import flags
                            importingVRTK = 0;
                            CheckImportFlags();
                        }
                    }
                    else
                    {
                        // we're not importing vrtk
                        CheckImportFlags();
                    }
                }
                else
                {
                    CheckImportFlags();
                }

                if (!GlobalPortalSettings.Nagged)
                {
                    var timeSinceInstall = DateTime.UtcNow.Ticks - GlobalPortalSettings.TimeInstalled;

                    if (timeSinceInstall > 5.888e+12D)
                    {
                        GlobalPortalSettings.Nagged = true;
                        timedFeedbackPopupActive = true;
                        SettingsWindow.Show(true, SettingsWindow.FEEDBACK);
                    }
                }
            }
            else
            {
                // project is uninitialized, show the setup window
                if (!ignoringInitialSetup)
                {
                    EditorUtility.ClearProgressBar();
                    SettingsWindow.Show();
                }
            }
        }

        static void CheckImportFlags()
        {
            if (performingSetup)
            {
                performingSetup = false;
                Dependencies.RescanDictionary();


                if (performingFirstRunSetup)
                {
                    performingFirstRunSetup = false;
                    GlobalPortalSettings.TimeInstalled = DateTime.UtcNow.Ticks;

                    // first time setup is completed
                    SettingsWindow.Show(true, SettingsWindow.SETUP);
                }
                else
                {
                    // setup is completed
                    SettingsWindow.Show(true, SettingsWindow.SETUP);
                }

                EditorUtility.ClearProgressBar();
            }
            else
            {
                //  all done, no need to pop up the window automatically!
                SettingsWindow.Hide();
            }
        }




        public static void ApplyVR(bool includeVRTK)
        {
            EditorUtility.DisplayProgressBar("Applying VR Presets...", "", 1f);

            if (includeVRTK)
            {
                EditorPrefs.SetInt(_importingVRTK, 2);

                // import VRTK package
                var vrtkPath = Directory.GetFiles("Assets", "vrtk.unitypackage", SearchOption.AllDirectories);
                if (vrtkPath.Length > 0)
                {
                    AssetDatabase.ImportPackage(vrtkPath[0], false);
                    //AssetDatabase.Refresh();
                }
                else
                {
                    // failed to find vrtk package
                }


            }
            else
            {
                // if the user decides to set VR mode without importing VRTK the compile flags can cause errors/
                // detect those errors and let the user know the problem.
                if (!VRTKIsMaybeInstalled)
                {
                    ConsoleCallbackHandler.AddCallback(HandleVRTKImportError, LogType.Error, "CS0246");
                }
            }

            var vrtkSupportPath = Directory.GetFiles("Assets", "vrtk_support.unitypackage", SearchOption.AllDirectories);
            if (vrtkSupportPath.Length > 0)
            {
                AssetDatabase.ImportPackage(vrtkSupportPath[0], false);
                AssetDatabase.Refresh();
            }
            else
            {
                // failed to find vrtk_support package
            }

            EditorPrefs.SetBool(_performingSetup, true);
            if (!projectInitialized)
            {
                PerformFirstTimeSetup();
            }

            SKSEditorUtil.AddDefine(vrSymbol, GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));
        }

        private static void HandleVRTKImportError()
        {
            ConsoleCallbackHandler.RemoveCallback(LogType.Error, "CS0246");
            EditorUtility.ClearProgressBar();

            EditorUtility.DisplayDialog("No VRTK Installation Found", "No suitable VRTK installation found. VR Portal scripts will not function and may throw errors if VRTK is not present.\n\nIf you have no existing VRTK installation, you should check the 'Also import VRTK' box before applying VR mode.", "Okay");

            SKSEditorUtil.RemoveDefine(vrSymbol, GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));
            performingSetup = false;

            if (performingFirstRunSetup)
            {
                SKSEditorUtil.RemoveDefine(projectSymbol, GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));
                performingFirstRunSetup = false;
            }
        }

        public static void ApplyDefault()
        {
            EditorUtility.DisplayProgressBar("Applying Default Presets...", "", 1f);

            EditorPrefs.SetBool(_performingSetup, true);
            if (!projectInitialized)
            {
                PerformFirstTimeSetup();
            }

            SKSEditorUtil.RemoveDefine(vrSymbol, GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));
        }

        static void PerformFirstTimeSetup()
        {
            SKSEditorUtil.AddDefine(projectSymbol, GetGroupFromBuildTarget(EditorUserBuildSettings.activeBuildTarget));
            EditorPrefs.SetBool(_performingFirstTimeSetup, true);
            EditorPrefs.DeleteKey(_ignoreSetup);

            //Create the default tags and layers
            GenTagDatabase();

            ReassignPrefabLayers();
        }

        [MenuItem("Tools/Generate Tag Database")]
        static void GenTagDatabase()
        {
            foreach (var t in Keywords.Tags.AllValues())
                SKTagManager.CreateTag(t);

            foreach (var l in Keywords.Layers.AllLayers)
                SKTagManager.CreateLayer(l);
        }

#if SKS_DEV
        [MenuItem("Tools/Reassign prefab layers")]
	    public static void ReassignPrefabLayersMenu() {
            ReassignPrefabLayers();
        }
#endif
        /// Reshuffles layers created during startup process.
        public static void ReassignPrefabLayers()
        {
            GameObject[] objects = Resources.LoadAll<GameObject>("");
            foreach (GameObject go in objects)
            {
                if (TagDatabase.tags.ContainsKey(go.name))
                {
                    if (TagDatabase.tags[go.name].Count == 0) continue;

                    foreach (Transform transform in go.transform.GetComponentsInChildren<Transform>())
                    {
                        if (TagDatabase.tags[go.name].ContainsKey(transform.gameObject.name))
                        {
#if SKS_DEV
                            String initialLayer = LayerMask.LayerToName(transform.gameObject.layer);
#endif
                            Dictionary<String, String> layerDict = TagDatabase.tags[go.name];
                            String layerString = layerDict[transform.gameObject.name];
                            transform.gameObject.layer = LayerMask.NameToLayer(layerString);
#if SKS_DEV
                            Debug.Log(String.Format("Changed parent {0}, child {1}, from layer {2} to layer {3}", 
                                go.name, transform.name, initialLayer, LayerMask.LayerToName(transform.gameObject.layer)));
#endif
                            EditorUtility.SetDirty(transform.gameObject);
                        }
                    }
                }

                EditorUtility.SetDirty(go);
            }
            AssetDatabase.Refresh();
            // unload unused assets?
        }

        // Allows for fuzzy matching of scripting symbols, eg. a query of "SOME_" will return true for "SOME_SYMBOL"
        public static bool IsScriptingSymbolDefined(string symbolFragment)
        {
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = GetGroupFromBuildTarget(buildTarget);
            var scriptingSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(targetGroup);

            return scriptingSymbols.IndexOf(symbolFragment) > -1;
        }

        static BuildTargetGroup GetGroupFromBuildTarget(BuildTarget buildTarget)
        {
            switch (buildTarget)
            {
                case BuildTarget.StandaloneOSX:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneOSXIntel64:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneOSXIntel:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneWindows64:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneWindows:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneLinux64:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneLinux:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.StandaloneLinuxUniversal:
                    return BuildTargetGroup.Standalone;

                case BuildTarget.Android:
                    return BuildTargetGroup.Android;

                case BuildTarget.iOS:
                    return BuildTargetGroup.iOS;

                case BuildTarget.WebGL:
                    return BuildTargetGroup.WebGL;

                case BuildTarget.WSAPlayer:
                    return BuildTargetGroup.WSA;

                case BuildTarget.Tizen:
                    return BuildTargetGroup.Tizen;

                case BuildTarget.PSP2:
                    return BuildTargetGroup.PSP2;

                case BuildTarget.PS4:
                    return BuildTargetGroup.PS4;

                case BuildTarget.PSM:
                    return BuildTargetGroup.PSM;

                case BuildTarget.XboxOne:
                    return BuildTargetGroup.XboxOne;

                case BuildTarget.SamsungTV:
                    return BuildTargetGroup.SamsungTV;

                case BuildTarget.N3DS:
                    return BuildTargetGroup.N3DS;

                case BuildTarget.WiiU:
                    return BuildTargetGroup.WiiU;

                case BuildTarget.tvOS:
                    return BuildTargetGroup.tvOS;

                case BuildTarget.NoTarget:
                    return BuildTargetGroup.Unknown;

                default:
                    return BuildTargetGroup.Unknown;

            }
        }

        public static string GetDocumentationPath()
        {
            var path = GetAssetRoot() + "readme.pdf";
            if (File.Exists(path)) return path;
            return string.Empty;
        }

        // @Hack, there's got to be a better way to do this
        public static string GetAssetRoot()
        {
            string assetName = "PortalKit Pro";
            string path = new System.Diagnostics.StackTrace(true).GetFrame(0).GetFileName();
            if (path == null) return string.Empty;

            path = path.Substring(0, path.LastIndexOf(Path.DirectorySeparatorChar));
            string root = path.Substring(0, path.LastIndexOf(assetName) + (assetName.Length + 1));
            return root;
        }



        // This clears all your scripting symbols, not just ones created by the setup process! Only use for debugging purposes!
        public static void DEBUG_ClearSetupData()
        {
            EditorPrefs.DeleteKey(_ignoreSetup);
            EditorPrefs.DeleteKey(_performingSetup);
            EditorPrefs.DeleteKey(_performingFirstTimeSetup);

            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            var targetGroup = GetGroupFromBuildTarget(buildTarget);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(targetGroup, "");
        }
    }
}
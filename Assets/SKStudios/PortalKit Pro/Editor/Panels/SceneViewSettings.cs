using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using SKStudios.Common.Utils;
using SKStudios.Rendering;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace SKStudios.Portals.Editor
{
    /// <summary>
    /// Used to setup SKSGlobalRenderSettings. Appears as a semitransparent fogged docked window.
    /// </summary>
    [InitializeOnLoad]
    [ExecuteInEditMode]
    public class SceneViewSettings : EditorWindow
    {

        internal static class ConnectInfo
        {
            public static String UpdateURL = "http://34.211.133.6:3010/";
        }

        internal static class Content
        {
            public static Texture ErrorIcon = EditorGUIUtility.Load("icons/console.erroricon.png") as Texture2D;
            public static Texture ErrorIconSmall = EditorGUIUtility.Load("icons/console.erroricon.sml.png") as Texture2D;
            public static Texture WarnIcon = EditorGUIUtility.Load("icons/console.warnicon.png") as Texture2D;
            public static Texture WarnIconSmall = EditorGUIUtility.Load("icons/console.warnicon.sml.png") as Texture2D;
            public static Texture InfoIcon = EditorGUIUtility.Load("icons/console.infoicon.png") as Texture2D;
            public static Texture InfoIconSmall = EditorGUIUtility.Load("icons/console.infoicon.sml.png") as Texture2D;

            public static Texture GetIcon(int id, bool large) {
                switch (id) {
                    case 1:
                        return large ? InfoIcon : InfoIconSmall;
                    case 2:
                        return large ? WarnIcon : WarnIconSmall;
                    case 3:
                        return large ? ErrorIcon : ErrorIconSmall;
                    default:
                        goto case 1;
                }
            }
        }

        private const int maxRating = 5;
        private const int supportRating = 3;
        //private static readonly float starSizeFull = starSize * maxRating;

        internal static class Styles
        {
            static bool _initialized = false;

            public static GUIStyle bgStyle;

            public static Material blurMat;
            public static Material borderMat;

            //public static Texture2D borderTex;
            public static Texture texturePlaceholder;

            public static GUIStyle windowStyle;
            public static GUIStyle coloredFoldout;
            public static GUIStyle menuOptionsStyle;
            public static GUIStyle notificationCloseStyle;
            public static GUIStyle notificationTextStyle;
            public static void Init()
            {
                if (_initialized) return;
                _initialized = true;

                blurMat = Resources.Load<Material>("UI/blur");
                borderMat = Resources.Load<Material>("UI/BorderMat");
                _initialized = true;

                windowStyle = new GUIStyle(GUI.skin.window)
                {
                    normal = {background = null},
                    border = new RectOffset(4, 4, 4, 4),
                    margin = new RectOffset(0, 0, 0, 0),
                    richText = true
                };

                texturePlaceholder = new Texture2D(1, 1);

                if (EditorStyles.foldout != null)
                    coloredFoldout = new GUIStyle(EditorStyles.foldout);
                else
                    coloredFoldout = new GUIStyle();

                coloredFoldout.normal.textColor = Color.white;
                coloredFoldout.hover.textColor = Color.white;
                coloredFoldout.active.textColor = Color.white;
                coloredFoldout.focused.textColor = Color.white;
                coloredFoldout.active.textColor = Color.white;
                coloredFoldout.onActive.textColor = Color.white;
                coloredFoldout.onFocused.textColor = Color.white;
                coloredFoldout.onHover.textColor = Color.white;
                coloredFoldout.onNormal.textColor = Color.white;

                if (skin.button != null)
                    menuOptionsStyle = new GUIStyle(skin.button);
                else
                    menuOptionsStyle = new GUIStyle();

                menuOptionsStyle.fontSize = 10;
                menuOptionsStyle.fontStyle = FontStyle.Bold;
                menuOptionsStyle.wordWrap = false;
                menuOptionsStyle.clipping = TextClipping.Overflow;
                menuOptionsStyle.margin = new RectOffset(0, 0, 0 ,0);

                notificationCloseStyle = new GUIStyle(menuOptionsStyle);
                notificationCloseStyle.normal.background = null;
                notificationCloseStyle.margin = new RectOffset(0, 0, 0, 0);

                notificationTextStyle = new GUIStyle(GUI.skin.label);
                notificationTextStyle.margin = new RectOffset(0, 0, 0, 0);

                var proBg = GlobalStyles.LoadImageResource("pkpro_selector_bg_pro");
                var defaultBg = GlobalStyles.LoadImageResource("pkpro_selector_bg");

                bgStyle = new GUIStyle();
                bgStyle.normal.background = EditorGUIUtility.isProSkin ? proBg : defaultBg;
                bgStyle.border = new RectOffset(2, 2, 2, 2);
                bgStyle.padding = new RectOffset(32, 5, 5, 5);
                bgStyle.richText = true;
                bgStyle.normal.textColor = Color.white;
                bgStyle.wordWrap = true;

            }
        }

        [Serializable]
        internal class ClientInfo
        {
            public String Version = String.Format("{0}.{1}.{2}", GlobalPortalSettings.MAJOR_VERSION,
                GlobalPortalSettings.MINOR_VERSION, GlobalPortalSettings.PATCH_VERSION);
        }

        

        [Serializable]
        internal class NotificationInfo
        {
            public List<Notification> Notifications;

            public NotificationInfo()
            {
                Notifications = new List<Notification>();
            }

            public void IgnoreMessage(int id)
            {
                SKSGlobalRenderSettings.IgnoredNotifications.Add(id);
            }

            public List<Notification> GetActiveNotifications()
            {
                var returnedList = new List<Notification>();
                foreach (Notification n in Notifications)
                {
                    if (SKSGlobalRenderSettings.IgnoredNotifications.Contains(n.Id)) continue;
                    returnedList.Add(n);
                }
                returnedList.Sort();
                return returnedList;
            }

            
            [Serializable]
            public class Notification : IComparable
            {
                public Notification(String message, int type, int id)
                {
                    Message = message;
                    Type = type;
                    Id = id;
                }
                public String Message;
                public int Type;
                public int Id;

                public int CompareTo(object obj)
                {
                    if (obj == null) return 1;
                    Notification other = obj as Notification;
                    if (other != null)
                        return other.Type - Type;
                    else 
                        throw new ArgumentException("Object is not a Notification");
                }
            }
        }

        private static Rect windowRect;
        private static GUISkin skin;


        private static float bumperSize = 3;
        private static Vector2 scrollPos;
        private static Vector2 defaultDockLocation = Vector2.zero;

        private static bool ImageSettings = false;
        private static bool InteractionSettings = false;
        private static bool EditorSettings = false;

        private static double startTime;
        private static float movetime = 4f;
        private static Vector2 defaultSize = new Vector2(740, 220);
        private static Vector2 size = defaultSize;
        private static Vector2 minimizedSize = new Vector2(200, 80);
        private static float time = 0;
        private static Camera sceneCamera;
        private static bool compatabilitymode;
        private static bool importantUpdate;

        private static bool hasLoadedWebpage;
        private static SceneViewSettings portalMenu;

        private static Texture texturePlaceholder = new Texture2D(1, 1);

        private static NotificationInfo _notificationInfo;

        private static Color _borderColor = new Color(0.5f, 0.5f, 0.5f, 1);

        public static Color BorderColor {
            get { return _borderColor; }
            set {
                _borderColor = value;
                Styles.borderMat.color = _borderColor;
            }
        }
        static SceneViewSettings()
        {
            EditorApplication.update += Enable;
        }

        [DidReloadScripts]
        public static void AutoOpen() {
            //Touch all setting ScriptableObjects so that they are not created off of the main thread
            SKSGlobalRenderSettings touch = SKSGlobalRenderSettings.Instance;
            GlobalPortalSettings touch2 = GlobalPortalSettings.Instance;
            //todo: Restore
            //if (!hasLoadedWebpage)
            {
                _notificationInfo = new NotificationInfo();
                new Thread(GetNotifications).Start();
                hasLoadedWebpage = true;
            }
        }
        /// <summary>
        /// Enable the menu
        /// </summary>
        [MenuItem("Tools/SK Studios/PortalKit Pro/Notifications", priority = 100)]
        public static void MenuEnable()
        {
            //Undo.RecordObject(SKSGlobalRenderSettings.Instance, "Global Portal Settings");
            SKSGlobalRenderSettings.MenuClosed = false;
            EditorApplication.update += Enable;
        }

        /// <summary>
        /// Setup the menu
        /// </summary>
        public static void Enable()
        {
            
            Disable();
            compatabilitymode = (SystemInfo.operatingSystemFamily == OperatingSystemFamily.MacOSX);
            EditorApplication.update -= Enable;
            SKSGlobalRenderSettings.Minimized = true;
            // EditorApplication.update += UpdateRect;
            skin = Resources.Load<GUISkin>("UI/PortalEditorGUISkin");
            SceneView.onSceneGUIDelegate += OnScene;
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public static void Disable()
        {
            //SceneView.onSceneGUIDelegate -= OnScene;
        }

        private static void UpdateRect()
        {
            if (Event.current.type == EventType.Layout)
            {
                /*
                if (!compatabilitymode)
                    defaultDockLocation = new Vector2((Screen.width) - size.x - 10, (Screen.height) - size.y - 19);
                else
                    defaultDockLocation = new Vector2(0, 20);*/
                defaultDockLocation = new Vector2((Screen.width / 2) - (windowRect.width / 2), 16);

                windowRect = new Rect(defaultDockLocation.x, defaultDockLocation.y, size.x, size.y);
                time = (float)(EditorApplication.timeSinceStartup - startTime) * movetime;
                if (time > 1)
                    time = 1;

                if (SKSGlobalRenderSettings.Minimized)
                {
                    //windowRect.position = Mathfx.Sinerp(defaultDockLocation + new Vector2(0, size.y - 20),
                    //    defaultDockLocation, 1 - time);
                    size.x = Mathfx.Sinerp(defaultSize.x, minimizedSize.x, time);
                    size.y = Mathfx.Sinerp(defaultSize.y, minimizedSize.y, time);
                    if (time < 1)
                        SceneView.RepaintAll();
                }
                else
                {
                    //windowRect.position = Mathfx.Sinerp(defaultDockLocation + new Vector2(0, size.y - 20),
                    //    defaultDockLocation, time);
                    size.x = Mathfx.Sinerp(defaultSize.x, minimizedSize.x, 1 - time);
                    size.y = Mathfx.Sinerp(defaultSize.y, minimizedSize.y, 1 - time);
                    if (time < 1)
                        SceneView.RepaintAll();
                }
            }

        }


        private static void OnScene(SceneView sceneview)
        {
            Styles.Init();
            if (!texturePlaceholder)
            {
                texturePlaceholder = new Texture2D(1, 1);
            }
            if (!hasLoadedWebpage)
            {
                _notificationInfo = new NotificationInfo();
                new Thread(GetNotifications).Start();
                hasLoadedWebpage = true;
            }

#if SKS_VR
            SKSGlobalRenderSettings.SinglePassStereo = PlayerSettings.stereoRenderingPath == StereoRenderingPath.SinglePass;
#endif

            if (Camera.current.name.Equals("SceneCamera"))
            {
                sceneCamera = Camera.current;
                UIBlurController.AddBlurToCamera(sceneCamera);
            }



            Handles.BeginGUI();



            GUI.skin = skin;
            //windowRect = new Rect (Screen.width - size.x - 100, Screen.height - size.y - 190, 200, 200);
            if (SetupUtility.projectInitialized)
            {
                if (SKSGlobalRenderSettings.MenuClosed)
                {
                    Handles.EndGUI(); return;
                }
                //Blur
                Graphics.DrawTexture(new Rect(windowRect.x, windowRect.y - 16, windowRect.width, windowRect.height),
                    texturePlaceholder, Styles.blurMat);
                //Window Border
                //Graphics.DrawTexture(new Rect(windowRect.x, windowRect.y - 16, windowRect.width, windowRect.height),
                //    Styles.borderTex, Styles.borderMat);
                 GUI.Window(0, windowRect, DoWindow, "<color=#2599f5>[PKPRO]</color> Notifications:");
                    
                GUI.skin = null;
                Handles.EndGUI();
            }
            else
            {
                Graphics.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texturePlaceholder, Styles.blurMat);
                if (GUILayout.Button("Click here to re-open the import dialog"))
                    SettingsWindow.Show();
#if SKS_DEV
                
                if (GUILayout.Button(
                    "(Intended for the devs of the asset) Click here to reset the ScriptableObjects for deployment")) {
                    SKSGlobalRenderSettings.RecursionNumber = 0;
                    SKSGlobalRenderSettings.AggressiveRecursionOptimization = true;
                    SKSGlobalRenderSettings.AdaptiveQuality = true;
                    SKSGlobalRenderSettings.CustomSkybox = true;
                    SKSGlobalRenderSettings.Preview = false;
                    SKSGlobalRenderSettings.PhysicsPassthrough = false;
                    SKSGlobalRenderSettings.PhysStyleB = true;
                    SKSGlobalRenderSettings.Gizmos = true;
                    SKSGlobalRenderSettings.IgnoredNotifications = new List<int>();
                    SKSGlobalRenderSettings.MenuClosed = false;
                    SKSGlobalRenderSettings.Minimized = false;
                    GlobalPortalSettings.Nagged = false;
                }
#endif
            }

        }

        /// <summary>
        /// Displays the window
        /// </summary>
        /// <param name="windowID">ID of the window</param>
        public static void DoWindow(int windowID)
        {
            BorderColor = Color.Lerp(new Color(1, 1, 1, 1), new Color(0, 0, 0, 1), Mathf.Sin((float)EditorApplication.timeSinceStartup));
            UpdateRect();
            EditorGUILayout.BeginVertical();
            Undo.RecordObject(SKSGlobalRenderSettings.Instance, "SKSGlobalRenderSettings");
            //Header controls
            UnityEditor.EditorGUI.BeginChangeCheck();
            if (GUI.Button(new Rect(3, 3, 15, 15), "X", Styles.menuOptionsStyle))
            {
                SKSGlobalRenderSettings.MenuClosed = true;
                Disable();
            }
                if (GUI.Button(new Rect(20, 3, 15, 15), SKSGlobalRenderSettings.Minimized ? "□" : "_", Styles.menuOptionsStyle))
                {
                    SKSGlobalRenderSettings.Minimized = !SKSGlobalRenderSettings.Minimized;
                    startTime = EditorApplication.timeSinceStartup;
                }


            Color guiColor = GUI.color;
            
            var activeNotifications = _notificationInfo.GetActiveNotifications();
            if (activeNotifications.Count == 0)
            {
                
                GUILayout.FlexibleSpace();
                {
                    GUILayout.BeginHorizontal();
                    {
                        GUILayout.FlexibleSpace();
                        GUILayout.Label("No notifications to display");
                        GUILayout.FlexibleSpace();
                    }
                    GUILayout.EndHorizontal();
                }
                //GUILayout.FlexibleSpace();
            }else if (SKSGlobalRenderSettings.Minimized) {
                int messages = 0;
                int warnings = 0;
                int errors = 0;
                foreach (NotificationInfo.Notification n in activeNotifications) {
                    switch (n.Type) {
                        case 1:
                            messages++;
                            break;
                        case 2:
                            warnings++;
                            break;
                        case 3:
                            errors++;
                            break;
                        default:
                            messages++;
                            break;
                    }
                }

                Func<int, int, Rect> getInfoIconPos = (index, total) => {
                    int hsize = 16;
                    int vsize = 16;
                    int padding = 3;
                    int vOffset = 10;
                    return new Rect(
                        0, vOffset + (((vsize + padding) * index) - ((vsize * total + (padding - 1) * total) / 2)) +
                           (windowRect.height / 2), hsize, vsize);
                };
                GUILayout.BeginHorizontal();
                {
                    int number = 0, index = 0;
                    if (messages > 0)
                        number++;
                    if (warnings > 0)
                        number++;
                    if (errors > 0)
                        number++;

                    Rect positionRect;
                    if (errors > 0) {
                        positionRect = getInfoIconPos(index++, number);
                        GUI.DrawTexture(positionRect, Content.ErrorIconSmall);
                        positionRect.x += positionRect.width;
                        positionRect.width = 400;
                        GUI.Label(positionRect, String.Format("{0} new critical notifications", errors), Styles.notificationTextStyle);
                    }


                    if (warnings > 0) {
                        positionRect = getInfoIconPos(index++, number);
                        GUI.DrawTexture(positionRect, Content.WarnIconSmall);
                        positionRect.x += positionRect.width;
                        positionRect.width = 400;
                        GUI.Label(positionRect, String.Format("{0} new important notifications", warnings), Styles.notificationTextStyle);
                    }


                    if (messages > 0) {
                        positionRect = getInfoIconPos(index++, number);
                        GUI.DrawTexture(positionRect, Content.InfoIconSmall);
                        positionRect.x += positionRect.width;
                        positionRect.width = 400;
                        GUI.Label(positionRect, String.Format("{0} new information notifications", messages), Styles.notificationTextStyle);
                    }
                        
                }
                GUILayout.EndHorizontal();
                GUI.color = guiColor;
                return;
            }
            else {
                GUI.color = Color.white;
                if (time >= 1)
                    try
                    {
                        scrollPos = GUILayout.BeginScrollView(scrollPos, false, false,
                            GUILayout.Width(windowRect.width - 20), GUILayout.Height(windowRect.height - 20));
                    }
                    catch (System.InvalidCastException) { }
                
                foreach (var n in activeNotifications)
                {
                    String message = n.Message;
                    Vector2 size = Styles.bgStyle.CalcSize(new GUIContent(message));
                    GUILayout.Box(message, Styles.bgStyle, GUILayout.MinHeight(Mathf.Max(size.y, 32)));

                    Rect labelFieldRect = GUILayoutUtility.GetLastRect();
                    GUI.DrawTexture(new Rect(labelFieldRect.x, labelFieldRect.y, 32, 32), Content.GetIcon(n.Type, true));
                    GUILayout.Space(10);
                    int width = 15;
                    if (GUI.Button(new Rect(labelFieldRect.x + labelFieldRect.width - width, labelFieldRect.y, width, width), "X",
                        Styles.notificationCloseStyle))
                    {
                        _notificationInfo.IgnoreMessage(n.Id);
                    }
                }

              
            }

            GUILayout.BeginHorizontal();
            {
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Restore all notifications",
                    Styles.menuOptionsStyle, GUILayout.MaxWidth(200)))
                    SKSGlobalRenderSettings.IgnoredNotifications = new List<int>();
#if SKS_DEV
                /*
                if (GUILayout.Button("Re-scan server for notifications",
                    Styles.menuOptionsStyle, GUILayout.MaxWidth(200)))
                    AutoOpen();*/
#endif
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndHorizontal();

            if(activeNotifications.Count != 0 && !SKSGlobalRenderSettings.Minimized && time >= 1)
                GUILayout.EndScrollView();


            GUI.color = guiColor;
            //GUILayout.FlexibleSpace();
            
            EditorGUILayout.EndVertical();
        }

        private static void GetNotifications() {
            String clientInfo = JsonUtility.ToJson(new ClientInfo());
            WebRequest request = WebRequest.Create(ConnectInfo.UpdateURL);
            request.Method = WebRequestMethods.Http.Post;
            try {
                byte[] encodedData = System.Text.Encoding.ASCII.GetBytes(clientInfo.ToCharArray());
                request.ContentLength = encodedData.Length;
                request.ContentType = "application/json";
                Stream dataStream = request.GetRequestStream();
                dataStream.Write(encodedData, 0, encodedData.Length);
                dataStream.Close();
                using (WebResponse response = request.GetResponse()) {
                    Stream stream = response.GetResponseStream();
                    if (stream == null) return;
                    using (StreamReader sr = new StreamReader(stream)) {
                        string notificationString = sr.ReadToEnd();
                        _notificationString = notificationString;


                        EditorApplication.update -= SetNotifications;
                        EditorApplication.update += SetNotifications;
                    }
                }
            }
            catch (Exception e) {
                //silent failures, non-essential
#if SKS_DEV
                Debug.Log("Could not get Notifications: " + e.Message);
#endif
            }
            
        }

        private static string _notificationString;
        private static void SetNotifications()
        {
            EditorApplication.update -= SetNotifications;
            _notificationInfo = (NotificationInfo)JsonUtility.FromJson(_notificationString, typeof(NotificationInfo));
            if (_notificationInfo.GetActiveNotifications().Count > 0)
                MenuEnable();
        }
    }
}


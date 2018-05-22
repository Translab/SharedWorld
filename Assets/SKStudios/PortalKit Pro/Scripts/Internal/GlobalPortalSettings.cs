using System;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SKStudios.Portals {
    /// <summary>
    /// Class containing saved settings data specific to the PortalKit Pro asset.
    /// </summary>
    [CreateAssetMenu(menuName = "ScriptableObjects/GlobalPortalSettings")]
    public class GlobalPortalSettings : ScriptableObject {
        public const int MAJOR_VERSION = 7;
        public const int MINOR_VERSION = 5;
        public const int PATCH_VERSION = 0;
        private static GlobalPortalSettings _instance;

        [SerializeField] [HideInInspector] private bool _nagged;

        [SerializeField] [HideInInspector] private Teleportable _playerTeleportableCached;

        [SerializeField] [HideInInspector] private double _timeInstalled;

        /// <summary>
        ///     Returns the singleton instance of the Global Portal Settings
        /// </summary>
        public static GlobalPortalSettings Instance {
            get {
                if (!_instance) {
#if UNITY_EDITOR
                    AssetDatabase.Refresh();
#endif

                    _instance = Resources.Load<GlobalPortalSettings>("Global Portal Settings");
                }

                return _instance;
            }
        }

        /// <summary>
        ///     The Player Teleportable
        /// </summary>
        public static Teleportable PlayerTeleportable {
            get { return Instance._playerTeleportableCached; }
            set {
                Instance._playerTeleportableCached = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        public static double TimeInstalled {
            get { return Instance._timeInstalled; }
            set {
                Instance._timeInstalled = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        public static bool Nagged {
            get { return Instance._nagged; }
            set {
                Instance._nagged = value;
#if UNITY_EDITOR
                EditorUtility.SetDirty(Instance);
#endif
            }
        }

        public override string ToString() {
            try {
                var builder = new StringBuilder();
                var duration = new TimeSpan((long) (DateTime.UtcNow.Ticks - TimeInstalled));
                var minutes = duration.TotalMinutes;
                builder.Append("SKSRenderSettings:{");
                builder.Append(minutes).Append('|');
                builder.Append(_nagged);
                builder.Append("}");
                return builder.ToString();
            }
            catch (Exception e) {
                return string.Format("SKSRenderSettings:{Error {0}}", e.Message);
            }
        }
    }
}
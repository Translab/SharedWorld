using UnityEngine;

namespace SKStudios.Portals {
    /// <summary>
    ///     Script used to mark which Teleportable is the Player's, for the purpose of seamless camera passthrough.
    /// </summary>
    [RequireComponent(typeof(Teleportable))]
    [ExecuteInEditMode]
    public class PlayerTeleportable : MonoBehaviour {
        private void Awake() {
            Update();
        }

        private void Update() {
            GlobalPortalSettings.PlayerTeleportable = GetComponent<Teleportable>();
        }

        private void OnDestroy() {
            GlobalPortalSettings.PlayerTeleportable = null;
        }
    }
}
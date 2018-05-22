using SKStudios.Common;
using SKStudios.Rendering;
using UnityEngine;

namespace SKStudios.Portals {
    public partial class Portal : EffectRenderer {
        /// <summary>
        ///     All collsision methods are externed to another script
        /// </summary>
        public void E_OnTriggerEnter(Collider col) {
            if (!Enterable)
                return;
#if !DISABLE_PHYSICS_IGNORE
            Physics.IgnoreCollision(col, PortalCollider);
#endif
            var teleportableBody = col.attachedRigidbody;
            if (!teleportableBody)
                return;
            var teleportScript = teleportableBody.GetComponent<Teleportable>();

            AddTeleportable(teleportScript);
        }


        /// <summary>
        ///     Checks if objects are in Portal, and teleports them if they are. Also handles player entry.
        /// </summary>
        /// <param name="col"></param>
        public void E_OnTriggerStay(Collider col) {
            if (col.gameObject.isStatic) return;
            if (!Enterable) return;
            if (!Target) return;

            var teleportableBody = col.attachedRigidbody;
            if (!teleportableBody)
                return;
            //todo: cache these
            var teleportScript = teleportableBody.GetComponent<Teleportable>();
            if (!AddTeleportable(teleportScript)) return;

            if (teleportScript == GlobalPortalSettings.PlayerTeleportable) _headInPortalTrigger = true;

            var globalLocalZScale =
                (Quaternion.Inverse(ArrivalTarget.rotation) *
                 Vector3.Scale(ArrivalTarget.forward, Target.transform.lossyScale)).z;
            //Updates clip planes for disappearing effect
            if (!NonObliqueOverride && SKSGlobalRenderSettings.Clipping)
                teleportScript.SetClipPlane(
                    Origin.position,
                    Origin.forward,
                    ArrivalTarget.position - -ArrivalTarget.forward * (globalLocalZScale * 0.01f),
                    -ArrivalTarget.forward);


            WakeBufferWall();

            //Makes objects collide with invisible buffer bounds
            foreach (var c in BufferWall) teleportScript.CollisionManager.AddCollision(this, c);

            //Makes objects collide with invisible buffer bounds
            foreach (var c in ((Portal) Target).BufferWall) {
#if !DISABLE_PHYSICS_IGNORE
                teleportScript.CollisionManager.IgnoreCollision(this, c, true);
#endif
            }

            if (SKSGlobalRenderSettings.PhysicsPassthrough) 
                using (var c = PassthroughColliders.GetEnumerator()) 
                    while (c.MoveNext()) 
                        teleportScript.CollisionManager.AddCollision(this, c.Current.Value); 

            //Passes Portal info to teleport script
            teleportScript.SetPortalInfo(this);

            //Enables Doppleganger
            teleportScript.EnableDoppleganger();

            //Checks if object should be teleported
            TryTeleportTeleporable(teleportScript, col);
        }

        /// <summary>
        ///     Removes primed objects from the queue if they move away from the Portal
        /// </summary>
        public void E_OnTriggerExit(Collider col) {
            if (!Enterable)
                return;

            var teleportableBody = col.attachedRigidbody;
            if (!teleportableBody)
                return;
            var teleportScript = teleportableBody.GetComponent<Teleportable>();

            if (col == HeadCollider)
                _headInPortalTrigger = false;

            RemoveTeleportable(teleportScript);
        }
    }
}
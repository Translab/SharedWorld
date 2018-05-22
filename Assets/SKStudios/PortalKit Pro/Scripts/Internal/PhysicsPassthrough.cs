using System.Collections;
using System.Collections.Generic;
using SKStudios.Common.Extensions;
using SKStudios.Common.Utils;
using SKStudios.Rendering;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace SKStudios.Portals {
    /// <summary>
    /// Class allowing for the linked <see cref="Portal"/> to have <see cref="Teleportable"/> objects 
    /// that are entering it interact with objects on the other side.
    /// </summary>
    public class PhysicsPassthrough : MonoBehaviour {
        private readonly Dictionary<Collider, Coroutine> _delayedDictionary =
            new Dictionary<Collider, Coroutine>();

        private readonly Dictionary<Collider, Coroutine> _delayEnableDictionary =
            new Dictionary<Collider, Coroutine>();

        private Bounds _bounds;

        private BoxCollider _collider;
        private PhysicsPassthroughScanner _scanner;

        /// <summary>
        ///     All colliders that can not be managed by this class
        /// </summary>
        [HideInInspector] public HashSet<int> BumColliders;

        /// <summary>
        ///     All colliders managed by this class
        /// </summary>
        [HideInInspector] public HashSet<int> Colliders;

        /// <summary>
        ///     Collection of all ignored colliders
        /// </summary>
        [HideInInspector] public HashSet<Collider> IgnoredColliders;

        public bool Initialized;

        /// <summary>
        ///     The target portal
        /// </summary>
        [HideInInspector] public Portal Portal;

        private BoxCollider Collider {
            get {
                if (_collider == null)
                    _collider = gameObject.GetComponent<BoxCollider>();
                return _collider;
            }
        }

        private void OnEnable() {
            IncrementalUpdater.RegisterObject<PhysicsPassthrough>(this, UpdateBounds, UpdateStep.FixedUpdate);
        }

        private void OnDisable() {
            IncrementalUpdater.RemoveObject<PhysicsPassthrough>(this, UpdateStep.FixedUpdate);
        }

        /// <summary>
        ///     Initialize the scanner with the given "parent" Portal.
        /// </summary>
        /// <param name="portal">The Portal that owns this scanner</param>
        public void Initialize(Portal portal) {
            if (Initialized) return;

            IgnoredColliders = new HashSet<Collider>();
            Colliders = new HashSet<int>();
            BumColliders = new HashSet<int>();

            Portal = portal;
            Initialized = true;
            Collider.enabled = true;
            Collider.size = Vector3.zero;

            foreach (Transform t in transform)
                if (_scanner = t.GetComponent<PhysicsPassthroughScanner>())
                    break;

            if (_scanner)
                _scanner.Initialize(this);
            else
                Debug.LogWarning(
                    "No PhysicsPassthroughScanner child found on PhysicsPassthrough! Physics passthrough will not function as expected.");

            UpdateBounds();

            Collider.attachedRigidbody.WakeUp();
        }

        /// <summary>
        ///     Update the bounds of the PhysicsPassthrough
        /// </summary>
        private void UpdateBounds() {
            if (!Initialized || !Portal || !Portal.ArrivalTarget) return;
            transform.position = Portal.ArrivalTarget.position +
                                 Vector3.Scale(_bounds.extents, -Portal.ArrivalTarget.forward * 1.1f);
            var scale = Portal.Origin.InverseTransformVector(Vector3.one * Portal.Target.Origin.lossyScale.magnitude) *
                        0.9f;
            scale.x = Mathf.Abs(scale.x);
            scale.y = Mathf.Abs(scale.y);
            scale.z = Mathf.Abs(scale.z);
            transform.localScale = scale;
            transform.rotation = Portal.ArrivalTarget.rotation;
            Collider.size = Vector3.one * 0.9f;
            _bounds = Collider.bounds;
        }

        /// <summary>
        ///     Add a collider to the physics passthrough tracker
        /// </summary>
        /// <param name="col">the collider to add</param>
        public void OnTriggerEnter(Collider col) {
            if (!Initialized) return;
            //Skip triggers
            if (col.isTrigger)
                return;
            //Ignores unresolvable colliders
            if (BumColliders.Contains(col.GetInstanceID()))
                return;
            //Only run if ready and enabled
            if (!SKSGlobalRenderSettings.PhysicsPassthrough || !Initialized) return;

            //Early rejection for invalid colliders
            if (!col
                || col.gameObject.layer.Equals(Keywords.Layers.CustomRenderer)
                || col.isTrigger
                || col.gameObject.CompareTag(Keywords.Tags.PhysicDupe)
                || Colliders.Contains(col.GetInstanceID()))
                return;

            if (IgnoredColliders.Count == 0 && Time.time > 1) {
                DelayedDetect(col);
                return;
            }

            //Instantiate new collider copy
            var newCollider = new GameObject {isStatic = false};
            Collider newColliderComponent;

            //Unfortunately no better way to do this
            if (col is BoxCollider) {
                newColliderComponent = newCollider.AddComponent<BoxCollider>();
                newColliderComponent.enabled = false;
                newColliderComponent = newColliderComponent.CloneFrom((BoxCollider) col);
                ((BoxCollider) newColliderComponent).size = ((BoxCollider) col).size;
                ((BoxCollider) newColliderComponent).center = ((BoxCollider) col).center;
            }
            else if (col is MeshCollider) {
                newColliderComponent = newCollider.AddComponent<MeshCollider>();
                newColliderComponent.enabled = false;
                newColliderComponent = newColliderComponent.CloneFrom((MeshCollider) col);
                var newMeshCollider = (MeshCollider) newColliderComponent;
                newMeshCollider.sharedMesh = ((MeshCollider) col).sharedMesh;
            }
            else if (col is CapsuleCollider) {
                newColliderComponent = newCollider.AddComponent<CapsuleCollider>();
                newColliderComponent.enabled = false;
                newColliderComponent = newColliderComponent.CloneFrom((CapsuleCollider) col);
            }
            else if (col is SphereCollider) {
                newColliderComponent = newCollider.AddComponent<SphereCollider>();
                newColliderComponent.enabled = false;
                newColliderComponent = newColliderComponent.CloneFrom((SphereCollider) col);
            }
            /*
            else if (col is CharacterController)
            {
                newColliderComponent = newCollider.AddComponent<CapsuleCollider>();
                newColliderComponent.enabled = false;
                newColliderComponent = newColliderComponent.GetCopyOf((CapsuleCollider)col);
            }*/
            else if (col is TerrainCollider) {
                newColliderComponent = newCollider.AddComponent<TerrainCollider>();
                newColliderComponent.enabled = false;
                //newColliderComponent = newColliderComponent.GetCopyOf((TerrainCollider)col);
                ((TerrainCollider) newColliderComponent).terrainData = ((TerrainCollider) col).terrainData;
            }
            else {
                BumColliders.Add(col.GetInstanceID());
                return;
            }

            foreach (var c in IgnoredColliders)
                if (c && newColliderComponent)
                    Physics.IgnoreCollision(c, newColliderComponent, true);

            UpdatePhysicsDupe(col, newColliderComponent, true);

            newCollider.tag = Keywords.Tags.PhysicDupe;
            newCollider.layer = Keywords.Layers.IgnoreRaycast;
            newCollider.name = "Duplicate Collider of " + col.name;
            newCollider.SetActive(false);

            DelayedEnable(newCollider, newColliderComponent);

            FinishInstantiation(col, newCollider);
        }

        private void FinishInstantiation(Collider col, GameObject newCollider) {
            Portal.PassthroughColliders.Add(col, newCollider.GetComponent<Collider>());
            Colliders.Add(col.GetInstanceID());
        }

        private void DelayedDetect(Collider newColliderComponent) {
            _delayedDictionary[newColliderComponent] = StartCoroutine(DelayedDetectEnum(newColliderComponent));
        }

        private IEnumerator DelayedDetectEnum(Collider col) {
            yield return WaitCache.Fixed;
            yield return WaitCache.Fixed;
            if (col) OnTriggerEnter(col);
        }

        private void DelayedEnable(GameObject newCollider, Collider newColliderComponent) {
            _delayEnableDictionary[newColliderComponent] =
                StartCoroutine(DelayedEnableEnum(newCollider, newColliderComponent));
        }

        private IEnumerator DelayedEnableEnum(GameObject obj, Collider col) {
            yield return WaitCache.Fixed;
            yield return WaitCache.Fixed;
            if (obj) obj.SetActive(true);

            if (col) col.enabled = true;
        }

        /// <summary>
        ///     Draw the bounding box for the passthrough manager
        /// </summary>
        public void OnDrawGizmosSelected() {
            if (!Initialized) return;

            var style = new GUIStyle(new GUIStyle {alignment = TextAnchor.MiddleCenter});
            style.normal.textColor = Color.white;
#if UNITY_EDITOR
            Handles.Label(_bounds.center + transform.up, "Physics Passthrough Detection Zone", style);
#endif

            Gizmos.color = new Color(1, 0, 0, 0.2f);
            Gizmos.DrawCube(_bounds.center, _bounds.extents * 2f);
        }

        /// <summary>
        ///     Remove collider's dupes that leave the detection zone
        /// </summary>
        /// <param name="col"></param>
        public void OnTriggerExit(Collider col) {
            //If some objects were awaiting processing and they leave the volume, axe them immediately
            //todo: this would be a useful class 
            Coroutine delayedCoroutine;
            if (_delayedDictionary.TryGetValue(col, out delayedCoroutine))
                StopCoroutine(delayedCoroutine);

            Coroutine delayedEnableCoroutine;
            if (_delayEnableDictionary.TryGetValue(col, out delayedEnableCoroutine))
                StopCoroutine(delayedEnableCoroutine);

            //Was the collider that left the volume one of the dupes? If so, destroy.
            if (!Initialized)
                return;


            Collider toDestroy;

            if (!Portal.PassthroughColliders.TryGetValue(col, out toDestroy))
                return;

            toDestroy.enabled = false;
            Destroy(toDestroy.gameObject);
            Portal.PassthroughColliders.Remove(col);
            Colliders.Remove(col.GetInstanceID());
        }

        /// <summary>
        ///     Used while teleporting objects to prevent physics step kickback
        /// </summary>
        /// <param name="col">the collider to rescan</param>
        public void ForceRescanOnColliders(IEnumerable<Collider> cols) {
            var b = _scanner.GetComponent<Collider>().bounds;
            foreach (var col in cols)
                if (col && b.Intersects(col.bounds))
                    _scanner.OnTriggerEnter(col);
        }

        /// <summary>
        ///     Realtime physics interactions on other side of Portal. Only triggered when teleportable is near.
        /// </summary>
        public void UpdatePhysics() {
            if (!SKSGlobalRenderSettings.PhysicsPassthrough) return;

            foreach (var original in Portal.PassthroughColliders.Keys) {
                //Get the analogous collider
                Collider dupe;

                if (!Portal.PassthroughColliders.TryGetValue(original, out dupe))
                    continue;
                UpdatePhysicsDupe(original, dupe);
            }
        }

        /// <summary>
        ///     Update the location of a physical duplicate of another collider
        /// </summary>
        /// <param name="original">The original collider</param>
        /// <param name="dupe">The Collider to move</param>
        /// <param name="init">Is this collider being initialized?</param>
        private void UpdatePhysicsDupe(Collider original, Collider dupe, bool init = false) {
            //Only update static objects on the first frame if the portal itself is also static
            if (original.gameObject.isStatic && Portal.gameObject.isStatic && !init || !original || !dupe)
                return;
            var isEnabled = dupe.enabled;
            dupe.enabled = false;
            dupe.transform.SetParent(original.transform);
            dupe.transform.localScale = Vector3.one;
            dupe.transform.localRotation = Quaternion.identity;
            dupe.transform.localPosition = Vector3.zero;

            dupe.transform.SetParent(Portal.ArrivalTarget, true);
            dupe.transform.SetParent(Portal.Origin, false);
            dupe.enabled = isEnabled;
        }
    }
}
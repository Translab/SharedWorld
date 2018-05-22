using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using SKStudios.Common.Extensions;
using SKStudios.Common.Utils;
using SKStudios.Common.Utils.SafeRemoveComponent;
using SKStudios.Rendering;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if VR_PORTALS
using VRTK;
#endif
namespace SKStudios.Portals {
    /// <summary>
    /// Component that drives logic of objects that are able to pass through <see cref="Portal"/>s.
    /// </summary>
    public class Teleportable : MonoBehaviour {

        /// <summary>
        ///     Re-calculates the doppleganger. Call when your teleportable object undergoes a visual change, to
        ///     delete the doppleganger and recreate it.
        /// </summary>
        public void RecomputeDoppleganger(){
            Destroy(BoundsCollider.gameObject);
            OnEnable();
        }

        private GameObject _doppleDisabler;

        /// <summary>
        ///     Event raised when this Teleportable teleports. Args are in the form of "Source portal, Destination portal".
        /// </summary>
        public event Action<Portal, Portal> OnTeleport;

        /// <summary>
        ///     Event raised when this Teleportable enters a portal trigger
        /// </summary>
        public event Action<Portal> OnPortalTriggerEnter;

        /// <summary>
        ///     Event raised when this Teleportable leaves a portal trigger
        /// </summary>
        public event Action<Portal> OnPortalTriggerExit;

        //The root of the game object to be copied
        public Transform Root;

        //Material to replace all other non-particle system materials on this object with (optional)
        public Material ReplacementMaterial;

        //Material to replace all particle system materials on this object with
        public Material ReplacementParticleMaterial;

        //Is the teleportable visual-only?
        public bool VisOnly;

        //Is the Teleportable fast-moving?
        public bool FastMoving = false;

        //Is the Teleportable a bullet?
        public bool Bullet = false;

        public CollisionIgnoreManager<Portal> CollisionManager;

        private bool _visOnly;

        //Delay (optional)
        public float delay;

        //Should scripts be stripped from the teleportable's doppleganger? 
        private readonly bool StripScripts = true;

        //Should scripts be stripped from the teleportable's doppleganger? 
        private readonly bool StripColliders = true;

        //Should scripts be stripped from the teleportable's doppleganger? 
        private readonly bool StripJoints = true;

        //Should scripts be stripped from the teleportable's doppleganger? 
        private readonly bool StripRigidbodies = true;

        //todo: Remember that this is a stopgap hack state flag and if it becomes a problem fix it
        private bool fastMovingAdded;

        private Dictionary<int, Collider> _colliders;

        private BoxCollider _boundsCollider;

        private BoxCollider BoundsCollider {
            get {
                if (!_boundsCollider) {
                    var colliderObj = new GameObject("Bounds obj");
                    colliderObj.transform.SetParent(transform, false);
                    _boundsCollider = colliderObj.AddComponent<BoxCollider>();
                    _boundsCollider.isTrigger = true;
                }

                return _boundsCollider;
            }
        }

        private Portal _currentPortal;

        private Portal CurrentPortal {
            get { return _currentPortal; }
            set {
                _currentPortal = value;

                if (TeleportableScripts != null)
                    foreach (var ts in TeleportableScripts) {
                        if (!ts) continue;
                        ts.currentPortal = _currentPortal;
                    }
            }
        }

        //In the form of Original, Duplicate
        public Dictionary<Transform, Transform> Transforms { get; private set; }
        public Dictionary<Renderer, Renderer> Renderers { get; private set; }
        public HashSet<SkinnedMeshRenderer> SkinnedRenderers { get; private set; }
        public HashSet<TeleportableScript> TeleportableScripts { get; private set; }


        [HideInInspector]
        public bool TeleportedLastFrame { get; set; }


        private readonly HashSet<Portal> _currentlyTeleportingPortals = new HashSet<Portal>();

        public bool MidTeleport {
            get { return _currentlyTeleportingPortals.Count > 0; }
        }

        public void StartTeleport(Portal p) {
            _currentlyTeleportingPortals.Add(p);
        }

        public void FinishTeleport(Portal p) {
            _currentlyTeleportingPortals.Remove(p);
        }

        [HideInInspector]
        public bool AddedLastFrame { get; set; }

        [HideInInspector]
        public bool RemovedLastFrame { get; set; }
#if !SKS_DEV
        [HideInInspector]
#endif
        public GameObject Doppleganger;
        [HideInInspector] public bool MovementOverride;
        [HideInInspector] public Bounds TeleportableBounds;

        public bool IsActive = true;

        private Collider _attachedCollider;

        private Collider AttachedCollider {
            get {
                if (!_attachedCollider)
                    _attachedCollider = GetComponent<Collider>();
                return _attachedCollider;
            }
        }

        private Animator test;
        [HideInInspector] public bool initialized;

        public Teleportable() {
            TeleportedLastFrame = false;
            RemovedLastFrame = false;
            AddedLastFrame = false;
        }

#if VR_PORTALS
        [HideInInspector] VRTK_InteractableObject interactable;
#endif

        private void OnEnable() {
            _colliders = new Dictionary<int, Collider>();

#if UNITY_EDITOR
            ConsoleCallbackHandler.AddCallback(() => {
                Dependencies.RescanDictionary();
                StartCoroutine(ConsoleTheConsole());
            }, LogType.Error, "Can't remove");
            if (PrefabUtility.GetPrefabParent(gameObject) == null &&
                PrefabUtility.GetPrefabObject(gameObject) != null)
                return;

#endif

            if (Doppleganger)
                Destroy(Doppleganger);

            if (_doppleDisabler)
                Destroy(_doppleDisabler);

            _doppleDisabler = new GameObject("Doppleganger Disabler (temp)");
            _doppleDisabler.SetActive(false);

            Transforms = new Dictionary<Transform, Transform>();
            Renderers = new Dictionary<Renderer, Renderer>();
            SkinnedRenderers = new HashSet<SkinnedMeshRenderer>();
            TeleportableScripts = new HashSet<TeleportableScript>();

            if (!Root)
                Root = transform;

            StartCoroutine(SpawnDoppleganger());
#if VR_PORTALS
            interactable = gameObject.GetComponent<VRTK_InteractableObject>();
#endif
            _visOnly = VisOnly;
#if !SKS_DEV
            Doppleganger.ApplyHideFlagsRecursive(HideFlags.HideAndDontSave);
#endif
            UpdateBounds();
            TeleportableBounds.center = transform.position;
        }

        private void OnDisable() {
            if (CollisionManager != null)
                CollisionManager.Reset();
            if (Doppleganger)
                Destroy(Doppleganger);
        }

        //I am a master of wit
        private IEnumerator ConsoleTheConsole() {
            yield return WaitCache.Frame;
            Debug.ClearDeveloperConsole();
            Debug.Log(
                "<color=#2599f5>[PKPRO]</color> Dependency graph not computed! Caught exception and wrote Dependency graph.");
        }

        private IEnumerator SpawnDoppleganger() {
            //Wait for any initializing behaviors
            if (delay > 0)
                yield return new WaitForSeconds(delay);

            //Spawns the Doppleganger game object
            Doppleganger = Instantiate(Root.gameObject, _doppleDisabler.transform);

            //Disables behaviors to prevent double instantiation
            IEnumerable<Component> dopplBehaviours = Doppleganger.GetComponentsInChildren<Behaviour>();
            foreach (var c in dopplBehaviours)
                if (c)
                    if (!(c is Light) && c is Behaviour)
                        ((Behaviour) c).enabled = false;


            InstantiateDoppleganger(Doppleganger.transform);

            //yield return WaitCache.Frame;
            Action<IEnumerable<Renderer>> replaceMaterialAction = rendererCollection => {
                foreach (var r in rendererCollection) {
                    Material rMaterial;
                    if (r is ParticleSystemRenderer)
                        rMaterial = ReplacementParticleMaterial;
                    else
                        rMaterial = ReplacementMaterial;

                    if (rMaterial == null)
                        continue;

                    var newMats = new Material[r.sharedMaterials.Length];
                    for (var i = 0; i < r.sharedMaterials.Length; i++) {
                        var dNewMat = new Material(rMaterial);
                        var m = r.sharedMaterials[i];
                        if (!dNewMat || !m)
                            continue;
                        dNewMat.CopyPropertiesFromMaterial(m);
                        dNewMat.renderQueue = m.renderQueue;
                        newMats[i] = dNewMat;
                    }

                    r.sharedMaterials = newMats;
                }
            };

            replaceMaterialAction(Renderers.Keys);
            replaceMaterialAction(Renderers.Values);

            //Initializes teleportable scripts
            foreach (var ts in TeleportableScripts) {
                if (!ts) continue;
                ts.Initialize(this);
            }

            Doppleganger.transform.SetParent(transform);

            ResetDoppleganger();
            initialized = true;
            Doppleganger.name = Doppleganger.name + " (Doppleganger)";
            //IncrementalUpdater.RegisterObject<Teleportable>(this, UpdateBounds, UnityUpdateStep.LateUpdate);
            CollisionManager = new CollisionIgnoreManager<Portal>(_colliders.Values.ToArray());
        }

        /// <summary>
        ///     Instantiate the Doppeganger recursively. Makes a fully copy of all visual
        ///     components of the object, discarding non-visual components.
        /// </summary>
        /// <param name="currentLevel">Current level to target</param>
        private void InstantiateDoppleganger(Transform currentLevel) {
            var other = SKSGeneralUtils.FindAnalogousTransform(currentLevel, Doppleganger.transform, Root, true);

            //Remove the MainCamera tag if it's been erroniously copied.
            if (currentLevel.tag.Equals("MainCamera"))
                currentLevel.tag = Keywords.Tags.Untagged;

            currentLevel.gameObject.name = currentLevel.gameObject.name;
            foreach (var component in currentLevel.GetComponents<Component>())
                if (component is Teleportable) {
                    component.SafeDestroyComponent();
                }
                //Copies Transforms for later updating
                else if (component is Transform) {
                    if (other) {
                        if (!Transforms.ContainsKey(other)) Transforms.Add(other, (Transform) component);
                    }
                    else {
                        Destroy(currentLevel.gameObject);
                        break;
                    }
                }
                else if (component is Renderer) {
                    if (component is SkinnedMeshRenderer) SkinnedRenderers.Add(component as SkinnedMeshRenderer);

                    var meshRenderer = component as MeshRenderer;
                    if (meshRenderer != null) {
                        var otherRend = other.GetComponent<MeshRenderer>();
                        if (!Renderers.ContainsKey(otherRend))
                            Renderers[otherRend] = meshRenderer;
                        //Adds colliders to list for collision ignoring upon Portal entry
                    }
                    else {
                        var otherRend = other.GetComponent<Renderer>();
                        if (!Renderers.ContainsKey(otherRend))
                            Renderers[otherRend] = (Renderer) component;
                        //Adds colliders to list for collision ignoring upon Portal entry
                    }
                }
                else if (component is Collider) {
                    if (!component.GetComponent<TeleportablePhysExclude>()) {
                        var c = other.GetComponent<Collider>();
                        if (!_colliders.ContainsKey(c.GetInstanceID())) _colliders.Add(c.GetInstanceID(), c);
#if SKS_VR
                        else {
                            //Fix for VRTK double-genning body colliders for no reason

                            currentLevel.gameObject.transform.SetParent(null);
                            c.enabled = false;
                            int key = c.GetInstanceID();
                            _colliders[key].enabled = false;
                            c.isTrigger = true;
                            _colliders[key].isTrigger = true;
                            DestroyImmediate(_colliders[key].gameObject);
                            DestroyImmediate(currentLevel.gameObject);
                            continue;

                    }
#endif
                    }

                    if (StripColliders && component)
                        component.SafeDestroyComponent();
                }
                else if (component is Rigidbody) {
                    if (StripRigidbodies)
                        component.SafeDestroyComponent();
                }
                else if (component is Joint) {
                    if (StripJoints)
                        component.SafeDestroyComponent();
                }
                else if (component is MonoBehaviour) {
                    //Handling of teleportable scripts
                    if (component is TeleportableScript)
                        TeleportableScripts.Add(other.GetComponent<TeleportableScript>());

                    if (!StripScripts)
                        ((MonoBehaviour) component).enabled = true;
                    else
                        component.SafeDestroyComponent();
                    //Nonspecific setup copying
                }
                else {
                    var system = component as ParticleSystem;
                    if (system != null) {
                        var otherSystem = other.GetComponent<ParticleSystem>();
                        system.randomSeed = otherSystem.randomSeed;
                        system.time = otherSystem.time;
                    }
                    else if (component is MeshFilter || component is Light) {
                        //nothin to do
                    }
                    else {
                        component.SafeDestroyComponent();
                    }
                }

            if (other)
                currentLevel.gameObject.SetActive(other.gameObject.activeSelf);

            foreach (Transform t in currentLevel)
                InstantiateDoppleganger(t);
        }

        /// <summary>
        ///     Update the bounds of the teleportable to include all colliders
        /// </summary>
        private void UpdateBounds() {
            if (_colliders.Count <= 0) return;
            TeleportableBounds.size = Vector3.zero;
            //todo: Removed for gc purposes, might need to be re-added
            //Collider col = _colliders.Values.ElementAt(0);
            //if (!col) return;
            if (_colliders.Count == 0)
                return;

            var centered = false;
            if (TeleportableScripts.Count == 0) {
                foreach (var c in _colliders.Values)
                {
                    //Todo: This is a patch to fix an immediate error, but it signals an underlying issue. Investigate.
                    if (c == null) continue;
                        if (!centered)
                    {
                        
                            TeleportableBounds.center = c.transform.position;
                    }
                       
                    TeleportableBounds.Encapsulate(c.bounds);
                }

                //if (c.GetComponent<TeleportableScript>() != null)
                if (!centered)
                    TeleportableBounds.center = transform.position;
            }

            UpdateBoundsRecursive(Root);

            BoundsCollider.transform.position = Vector3.zero;
            BoundsCollider.transform.rotation = Quaternion.identity;
            BoundsCollider.transform.localScale = Vector3.one;
            BoundsCollider.transform.localScale = BoundsCollider.transform.InverseTransformVector(Vector3.one);

            BoundsCollider.center = TeleportableBounds.center;
            BoundsCollider.size = TeleportableBounds.size;
        }

        /// <summary>
        ///     Update recursively, ignoring branches with teleportablescripts on them
        /// </summary>
        /// <param name="currentLevel"></param>
        private void UpdateBoundsRecursive(Transform currentLevel) {
            var c = currentLevel.gameObject.SKGetComponentOnce<Collider>();
            if (c && c != BoundsCollider) TeleportableBounds.Encapsulate(c.bounds.center);

            for (var i = 0; i < currentLevel.childCount; i++)
                UpdateBoundsRecursive(currentLevel.GetChild(i));
        }

        public void Teleport(Portal source, Portal destination) {
            if (OnTeleport != null)
                OnTeleport.Invoke(source, destination);

            foreach (var t in TeleportableScripts) {
                if (!t) continue;
                t.OnTeleport();
            }

            UpdateBounds();
            StartCoroutine(FlashFix());
        }

        /// <summary>
        ///     Fix for teleportables flashing as they pass through portals
        /// </summary>
        /// <returns></returns>
        private IEnumerator FlashFix() {
            foreach (var r in Renderers.Keys)
                r.sharedMaterial.SetFloat(Keywords.ShaderKeys.ClipOverride, 1);
            foreach (var r in Renderers.Values)
                r.sharedMaterial.SetFloat(Keywords.ShaderKeys.ClipOverride, 1);

            yield return WaitCache.Frame;
            yield return WaitCache.Frame;

            foreach (var r in Renderers.Keys)
                r.sharedMaterial.SetFloat(Keywords.ShaderKeys.ClipOverride, 0);
            foreach (var r in Renderers.Values)
                r.sharedMaterial.SetFloat(Keywords.ShaderKeys.ClipOverride, 0);
        }

        /// <summary>
        ///     Resets the doppleganger state
        /// </summary>
        public void ResetDoppleganger() {
            Doppleganger.transform.SetParent(null);
            Doppleganger.transform.localPosition = Vector3.zero;
            Doppleganger.transform.localRotation = Quaternion.identity;
            //Doppleganger.SetActive(false);
            DisableDoppleganger();
        }

        private void FixedUpdate() {
            if (!gameObject.activeInHierarchy)
                return;
            UpdateBounds();

            if (!FastMoving) return;

            if (!AttachedCollider || !AttachedCollider.attachedRigidbody) return;
            RaycastHit[] hits;
            var velocity = AttachedCollider.attachedRigidbody.velocity.normalized;
            var magnitude = velocity.magnitude * Time.deltaTime;
            if (!Bullet) {
                hits = Physics.BoxCastAll(transform.position - velocity * magnitude, AttachedCollider.bounds.extents,
                    velocity, Quaternion.identity,
                    AttachedCollider.attachedRigidbody.velocity.magnitude * Time.deltaTime, ~0,
                    QueryTriggerInteraction.Collide);
#if UNITY_EDITOR
                DebugDrawExtensions.DrawBoxCastBox(transform.position - velocity * magnitude,
                    AttachedCollider.bounds.extents, Quaternion.identity, velocity,
                    AttachedCollider.attachedRigidbody.velocity.magnitude * Time.deltaTime * 2f, Color.white);
#endif
            }
            else {
                hits = Physics.BoxCastAll(transform.position - velocity * magnitude, AttachedCollider.bounds.extents,
                    velocity, Quaternion.identity,
                    AttachedCollider.attachedRigidbody.velocity.magnitude * Time.deltaTime * 2f, ~0,
                    QueryTriggerInteraction.Collide);
#if UNITY_EDITOR
                DebugDrawExtensions.DrawBoxCastBox(transform.position - velocity * magnitude,
                    AttachedCollider.bounds.extents, Quaternion.identity, velocity,
                    AttachedCollider.attachedRigidbody.velocity.magnitude * Time.deltaTime * 2f, Color.white);
#endif
            }

            var predictedCollide = false;
            foreach (var h in hits) {
                if (!h.collider) continue;
                var portal = h.collider.GetComponent<Portal>();
                if (!portal) {
                    PortalTrigger trigger;
                    if (trigger = h.collider.GetComponent<PortalTrigger>())
                        portal = trigger.portal;
                }

                if (portal) {
                    //Force the next physics step to tick on the bullet
                    if (Bullet)
                        if (SKSGeneralUtils.IsBehind(
                            transform.position + AttachedCollider.attachedRigidbody.velocity * Time.deltaTime,
                            portal.Origin.position,
                            portal.Origin.forward)) {
                            transform.position += AttachedCollider.attachedRigidbody.velocity * Time.deltaTime;
                            PortalUtils.TeleportObject(gameObject, portal.Origin, portal.ArrivalTarget, Root);
                        }

                    portal.E_OnTriggerStay(AttachedCollider);
                    predictedCollide = true;
                    fastMovingAdded = true;
                }

                /*
                if (portal) {
                    if (PortalUtils.IsBehind(transform.position + velocity, portal.Origin.position,
                        portal.Origin.forward)) {
                        portal.E_OnTriggerEnter(AttachedCollider);
                        predictedCollide = true;
                    }
                    
                }*/
            }


            if (!predictedCollide && fastMovingAdded)
                if (CurrentPortal != null && AttachedCollider) {
                    CurrentPortal.E_OnTriggerExit(AttachedCollider);
                    fastMovingAdded = false;
                }
        }


        private void LateUpdate() {
            if (!gameObject.activeInHierarchy || !enabled)
                return;
            //If the object is grabbed, make vis only
#if VR_PORTALS
            if (interactable)
                VisOnly = interactable.IsGrabbed() || _visOnly;
#endif
            foreach (var t in TeleportableScripts)
                if (t && t.isActiveAndEnabled)
                    t.CustomUpdate();
        }

        /// <summary>
        ///     Reset state
        /// </summary>
        private void Update() {
            if (!gameObject.activeInHierarchy)
                return;

            TeleportedLastFrame = false;
            AddedLastFrame = false;
            RemovedLastFrame = false;
        }

        /// <summary>
        ///     Updates the doppleganger's visuals
        /// </summary>
        /// <returns></returns>
        public void UpdateDoppleganger() {
            if (!initialized) return;
            Doppleganger.transform.SetParent(Root.parent);
            Doppleganger.transform.localRotation = Root.localRotation;
            Doppleganger.transform.localPosition = Root.localPosition;
            if (!MovementOverride) {
                foreach (var t in Transforms.Keys) {
                    if (t == null || Transforms[t] == null || t == Root)
                        continue;
                    //if(t.GetComponent<TeleportableScript>())
                    Transforms[t].localPosition = t.localPosition;
                    Transforms[t].localRotation = t.localRotation;
                }


                //Prevents renderers from being enabled mid-frame

                foreach (var t in Transforms.Keys) {
                    if (t == null || Transforms[t] == null)
                        continue;
                    Transforms[t].gameObject.SetActive(t.gameObject.activeSelf);
                }

                Doppleganger.transform.localScale = Root.localScale;
                Doppleganger.transform.localPosition = Root.localPosition;
                Doppleganger.transform.localRotation = Root.localRotation;
            }
        }

        /// <summary>
        ///     Enable doppleganger rendering
        /// </summary>
        public void EnableDoppleganger() {
            StartCoroutine(DoppleAction(() => {
                if (Doppleganger) {
                    using (var c = Renderers.GetEnumerator())
                        while (c.MoveNext())
                            if (c.Current.Key && c.Current.Value)
                                c.Current.Value.enabled = c.Current.Key.enabled;

                    Doppleganger.SetActive(true);
                }
            }));
        }

        /// <summary>
        ///     Disables doppleganger rendering
        /// </summary>
        public void DisableDoppleganger() {
            StartCoroutine(DoppleAction(() => {
                if (Doppleganger) {
                    foreach (var r in Renderers.Keys)
                        if (r && Renderers[r])
                            Renderers[r].enabled = false;
                    SetClipPlane(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero, true);
                    Doppleganger.SetActive(false);
                }
            }));
        }

        /// <summary>
        ///     Execute some action on the doppleganger for this teleportable
        /// </summary>
        /// <param name="behavior">Action to execute</param>
        /// <returns>IEnumerator</returns>
        private IEnumerator DoppleAction(Action behavior)
        {
            var wait = WaitCache.OneTenthS;
            while (!initialized)
                yield return wait;
            behavior();
        }

        /// <summary>
        ///     Sets the clip plane for the teleportable object's material
        /// </summary>
        /// <param name="sourcePosition">The position of the plane</param>
        /// <param name="sourceNormal">The normal of the plane</param>
        /// <param name="destPosition">The position of the plane</param>
        /// <param name="destNormal">The normal of the plane</param>
        /// <param name="DopplegangerOnly">Is the clip transform only being applied to the doppleganger?</param>
        public void SetClipPlane(Vector3 sourcePosition, Vector3 sourceNormal, Vector3 destPosition, Vector3 destNormal,
            bool DopplegangerOnly = false) {
            if (!initialized)
                return;

            using (var r = Renderers.GetEnumerator()) {
                while (r.MoveNext()) {
                    var key = r.Current.Key;
                    if (key && !DopplegangerOnly) {
                        var rms = SharedMaterialsCache.GetSharedMaterials(key);
                        for (var i = 0; i < rms.Length; i++) {
                            var rm = rms[i];
                            if (!rm) continue;
                            rm.SetVector(Keywords.ShaderKeys.ClipPos, sourcePosition);
                            rm.SetVector(Keywords.ShaderKeys.ClipVec, sourceNormal);
                            if (!rm.HasProperty(Keywords.ShaderKeys.ClipPos) && SKSGlobalRenderSettings.Clipping)
                                Debug.LogWarning(
                                    "Valid pixel-perfect material not set on teleportable. Object will be able to" +
                                    " be seen through the back of portals unless this is replaced.");
                        }
                    }
                    var dr = r.Current.Value;
                    if (dr) {
                        var rms = SharedMaterialsCache.GetSharedMaterials(dr);
                        for (var i = 0; i < rms.Length; i++) {
                            var rm = rms[i];
                            if (!rm) continue;
                            rm.SetVector(Keywords.ShaderKeys.ClipPos, destPosition);
                            rm.SetVector(Keywords.ShaderKeys.ClipVec, destNormal);
                            if (!rm.HasProperty(Keywords.ShaderKeys.ClipPos) && SKSGlobalRenderSettings.Clipping)
                                Debug.LogWarning(
                                    "Valid pixel-perfect material not set on teleportable. Object will be able to" +
                                    "be seen through the back of portals unless this is replaced.");
                        }
                    }    
                }
            }
        }

        /// <summary>
        ///     Adds a child to the teleportable and updates components
        /// </summary>
        /// <param name="gameObject">GameObject to add</param>
        public void AddChild(GameObject gameObject) {
            gameObject.SetActive(!gameObject.activeSelf);
            var newObject = Instantiate(gameObject);
            gameObject.SetActive(!gameObject.activeSelf);
            newObject.transform.parent =
                SKSGeneralUtils.FindAnalogousTransform(gameObject.transform.parent, Root, Doppleganger.transform, true);
            UpdateBounds();
            InstantiateDoppleganger(newObject.transform);
        }

        /// <summary>
        ///     Remove child from teleportable
        /// </summary>
        /// <param name="gameObject"></param>
        public void RemoveChild(GameObject gameObject) {
            Destroy(SKSGeneralUtils.FindAnalogousTransform(gameObject.transform, Root, Doppleganger.transform)
                .gameObject);
        }

        /// <summary>
        ///     Set the portal info of the currently active portal
        /// </summary>
        /// <param name="portal"></param>
        public void SetPortalInfo(Portal portal) {
            CurrentPortal = portal;
        }

        /// <summary>
        ///     Method called by Portal when this Teleportable enters its detection zone
        /// </summary>
        /// <param name="portal">portal entered</param>
        public void EnterPortal(Portal portal) {
            if (OnPortalTriggerEnter != null)
                OnPortalTriggerEnter.Invoke(portal);
        }

        /// <summary>
        ///     Method called by Portal when this Teleportable leaves its detection zone
        /// </summary>
        /// <param name="portal">portal entered</param>
        public void LeavePortal(Portal portal) {
            if (OnPortalTriggerExit != null)
                OnPortalTriggerExit.Invoke(portal);

            if (TeleportableScripts != null)
                foreach (var ts in TeleportableScripts) {
                    if (!ts) continue;
                    ts.LeavePortal();
                }

            DisableDoppleganger();
        }

        /// <summary>
        ///     Cleanup
        /// </summary>
        private void OnDestroy() {
            if (TeleportableScripts != null)
                foreach (var ts in TeleportableScripts) {
                    if (!ts) continue;
                    Destroy(ts);
                }

            Destroy(Doppleganger);
        }

        /// <summary>
        ///     Draws bounds of the teleportable
        /// </summary>
        public void OnDrawGizmosSelected() {
            Gizmos.color = new Color(0, 1, 1, 0.4f);

            var style = new GUIStyle();
            style.normal.textColor = Color.black;
#if UNITY_EDITOR
            Handles.Label(transform.position, "Bounds of Teleportable", style);
#endif

            Gizmos.DrawCube(TeleportableBounds.center, TeleportableBounds.extents * 2f);
        }
    }
}
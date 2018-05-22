using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SKStudios.Portals {
    /// <summary>
    ///     Tracks collisions controlled by type T
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CollisionIgnoreManager<T> where T : class {
        private readonly Dictionary<T, HashSet<Collider>> _addedColliders;
        private readonly Dictionary<T, HashSet<Collider>> _ignoredColliders;

        public CollisionIgnoreManager(Collider[] colliders) {
            Colliders = colliders;
            _ignoredColliders = new Dictionary<T, HashSet<Collider>>();
            _addedColliders = new Dictionary<T, HashSet<Collider>>();
        }

        public Collider[] Colliders { get; private set; }

        /// <summary>
        ///     Resume all collisions handled with this object
        /// </summary>
        public void Reset() {
            var keys = _ignoredColliders.Keys.ToArray();
            if (_ignoredColliders != null)
                foreach (var k in keys)
                    ResumeCollisionsForKey(k);
        }

        /// <summary>
        ///     Make Teleportable not collide with gameobject. Overridden by AddCollision by default.
        /// </summary>
        /// <param name="key">the key for which collision is being ignored</param>
        /// <param name="ignoredCollider">Collider to ignore</param>
        /// <param name="shouldOverrideAdd">Should this override added collisions?</param>
        /// <param name="untracked">Should this collision get reset when other collisions are? If false, will not be resettable.</param>
        public void IgnoreCollision(T key, Collider ignoredCollider, bool shouldOverrideAdd = false,
            bool untracked = false) {
#if !DISABLE_PHYSICS_IGNORE
            if (key == null)
                return;
            if (untracked) goto RETURN;

            HashSet<Collider> ignoredSet;
            if (!_ignoredColliders.TryGetValue(key, out ignoredSet)) {
                ignoredSet = new HashSet<Collider>();
                _ignoredColliders.Add(key, ignoredSet);
            }

            HashSet<Collider> addedSet;
            if (!_addedColliders.TryGetValue(key, out addedSet)) {
                addedSet = new HashSet<Collider>();
                _addedColliders.Add(key, addedSet);
            }

            ignoredSet.Add(ignoredCollider);

            if (_addedColliders[key].Contains(ignoredCollider) && !shouldOverrideAdd) return;

            RETURN:
            foreach (var col in Colliders) {
                if (col == null || ignoredCollider == null)
                    continue;
                Physics.IgnoreCollision(col, ignoredCollider, true);
            }
#endif
        }


        /// <summary>
        ///     Make Teleportable collide with gameobject. Overrides IgnoreCollision by default.
        /// </summary>
        /// ///
        /// <param name="key">the key for which collision is being ignored</param>
        /// <param name="addCollider">Collider to add</param>
        /// <param name="untracked">Should this collision get reset when other collisions are? If false, will not be resettable.</param>
        public void AddCollision(T key, Collider addCollider, bool untracked = false) {
#if !DISABLE_PHYSICS_IGNORE
            if (key == null) return;
            if (untracked) goto RETURN;

            HashSet<Collider> addedSet;
            if (!_addedColliders.TryGetValue(key, out addedSet)) {
                addedSet = new HashSet<Collider>();
                _addedColliders.Add(key, addedSet);
            }

            addedSet.Add(addCollider);

            RETURN:
            foreach (var col in Colliders) {
                if (col == null || addCollider == null)
                    continue;
                Physics.IgnoreCollision(col, addCollider, false);
            }
#endif
        }

        /// <summary>
        ///     Undo all changes made with AddCollision and RemoveCollision
        /// </summary>
        /// <param name="key">the Key</param>
        public void ResumeCollisionsForKey(T key) {
#if !DISABLE_PHYSICS_IGNORE
            if (key == null)
                return;

            HashSet<Collider> ignoredSet;
            if (!_ignoredColliders.TryGetValue(key, out ignoredSet)) {
                ignoredSet = new HashSet<Collider>();
                _ignoredColliders.Add(key, ignoredSet);
            }

            HashSet<Collider> addedSet;
            if (!_addedColliders.TryGetValue(key, out addedSet)) {
                addedSet = new HashSet<Collider>();
                _addedColliders.Add(key, addedSet);
            }

            foreach (var col in Colliders) {
                foreach (var aCol in _addedColliders[key])
                    if (aCol && col)
                        Physics.IgnoreCollision(col, aCol, true);

                foreach (var iCol in _ignoredColliders[key])
                    if (iCol && col)
                        Physics.IgnoreCollision(col, iCol, false);
            }

            _ignoredColliders.Remove(key);
            _addedColliders.Remove(key);
#endif
        }
    }
}
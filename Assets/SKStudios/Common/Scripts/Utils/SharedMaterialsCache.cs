using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SKStudios.Common.Utils {
    /// <summary>
    /// Class that stores SharedMaterials from renderers so that they only
    /// have to be retrieved once to prevent gc alloc. Only use for renderers
    /// that will not be changing materials.
    /// </summary>
    public static class SharedMaterialsCache {
        private static readonly Dictionary<Renderer, Material[]> SharedMaterials;
        static SharedMaterialsCache() {
            SharedMaterials = new Dictionary<Renderer, Material[]>();
        }

        /// <summary>
        /// Get the shared materials of a given renderer.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        public static Material[] GetSharedMaterials(Renderer r) {
            Material[] matArr;
            if (!SharedMaterials.TryGetValue(r, out matArr)) {
                matArr = r.sharedMaterials;
                SharedMaterials[r] = matArr;
            }

            return matArr;
        }
    }
}

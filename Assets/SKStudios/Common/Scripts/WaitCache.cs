using UnityEngine;

namespace SKStudios.Common.Utils {
    /// <summary>
    ///     For some reason, Unity coroutine waits generate garbage no matter what. I'm not happy with that.
    /// </summary>
    public static class WaitCache {
        public static readonly WaitForSeconds OneTenthS = new WaitForSeconds(0.1f);
        public static readonly WaitForSeconds OneS = new WaitForSeconds(1f);
        public static readonly WaitForEndOfFrame Frame = new WaitForEndOfFrame();
        public static readonly WaitForFixedUpdate Fixed = new WaitForFixedUpdate();
    }
}
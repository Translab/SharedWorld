using System.Collections;
using SKStudios.Common.Utils;
using UnityEngine;

namespace SKStudios.Portals.Demos {
    //[ExecuteInEditMode]
    public class Spinning : MonoBehaviour {
        public Vector3 axis = Vector3.up;
        private float destSpeed;
        public float speed = 4f;
        public bool spinning = false;
        public float spinupTime;
        public bool triggerRequired = false;

        private void Start() {
            if (triggerRequired) {
                destSpeed = speed;
                speed = 0;
            }
        }
        // Update is called once per frame

        private void Update() {
            if (spinning)
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles +
                                                      Quaternion.AngleAxis(speed * Time.deltaTime, axis).eulerAngles);
        }

        private IEnumerator smoothToSpeed(float spinupTime, float speed) {
            while (this.speed <= destSpeed) {
                this.speed += Mathfx.Sinerp(0, destSpeed, Time.deltaTime / spinupTime);
                yield return WaitCache.Fixed;
            }

            yield return null;
            this.speed = destSpeed;
        }
    }
}
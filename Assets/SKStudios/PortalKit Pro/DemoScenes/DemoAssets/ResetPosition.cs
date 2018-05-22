using System.Collections;
using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class ResetPosition : MonoBehaviour {
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private Vector3 initialScale;
        private IEnumerator ResetPos;
        public float resetTime;

        private Rigidbody rigid;

        // Use this for initialization
        private void Start() {
            rigid = gameObject.GetComponent<Rigidbody>();
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            initialScale = transform.localScale;
            ResetPos = ResetPositionandVelocity(resetTime);
            StartCoroutine(ResetPos);
        }

        public IEnumerator ResetPositionandVelocity(float time) {
            while (true) {
                yield return new WaitForSecondsRealtime(time);
                rigid.isKinematic = true;
                transform.position = initialPosition;
                transform.rotation = initialRotation;
                transform.localScale = initialScale;
                rigid.isKinematic = false;
                rigid.velocity = Vector3.zero;
            }
        }
    }
}
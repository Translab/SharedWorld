using System.Collections;
using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class BallGun : MonoBehaviour {
        public GameObject ball;
        public float delay = 5;

        public float speed = 200;

        public Transform tip;

        // Use this for initialization
        private void Start() {
            StartCoroutine(FireBall());
        }

        private IEnumerator FireBall() {
            while (true) {
                yield return new WaitForSeconds(delay);
                var newBall = Instantiate(ball);
                newBall.transform.position = tip.position;
                var rigid = newBall.GetComponent<Rigidbody>();
                rigid.isKinematic = false;
                rigid.velocity = Vector3.zero;
                rigid.AddForce(tip.forward * speed);
                StartCoroutine(DestroyBall(newBall));
            }
        }

        private IEnumerator DestroyBall(GameObject go) {
            yield return new WaitForSeconds(5f);
            Destroy(go);
        }
    }
}
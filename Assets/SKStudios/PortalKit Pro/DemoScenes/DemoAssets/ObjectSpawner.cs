using System.Collections;
using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class ObjectSpawner : MonoBehaviour {
        private Rigidbody body;
        private GameObject cube;
        public GameObject spawnedObject;

        // Use this for initialization
        private void Start() {
            cube = Instantiate(spawnedObject);
            body = cube.GetComponent<Rigidbody>();
            StartCoroutine(DropCube());
        }

        private IEnumerator DropCube() {
            while (true) {
                cube.transform.position = transform.position;
                cube.transform.rotation = Quaternion.identity;
                body.velocity = Vector3.zero;
                yield return new WaitForSeconds(2);
            }
        }
    }
}
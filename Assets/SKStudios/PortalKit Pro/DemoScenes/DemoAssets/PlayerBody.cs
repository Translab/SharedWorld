using System.Collections;
using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class PlayerBody : MonoBehaviour {
        public GameObject target;

        private Collider targetCol;

        // Use this for initialization
        private void Start() {
            StartCoroutine(Setup());
        }

        // Update is called once per frame
        private void Update() {
            if (targetCol)
                transform.position = targetCol.bounds.center;
        }

        private IEnumerator Setup() {
            yield return new WaitForSeconds(1);
            targetCol = target.GetComponent<Collider>();
        }
    }
}
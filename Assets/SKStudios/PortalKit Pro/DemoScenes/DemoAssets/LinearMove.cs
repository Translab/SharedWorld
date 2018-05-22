using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class LinearMove : MonoBehaviour {
        private bool dir;
        public float height = 1;
        private Vector3 initialPos;
        public float speed = 10;

        // Use this for initialization
        private void Start() {
            initialPos = transform.position;
        }

        // Update is called once per frame
        private void Update() {
            if (!dir)
                transform.position = transform.position + new Vector3(0, speed * Time.deltaTime, 0);
            else
                transform.position = transform.position - new Vector3(0, speed * Time.deltaTime, 0);

            if (transform.position.y < initialPos.y - height && dir)
                dir = false;
            if (transform.position.y > initialPos.y && !dir)
                dir = true;
        }
    }
}
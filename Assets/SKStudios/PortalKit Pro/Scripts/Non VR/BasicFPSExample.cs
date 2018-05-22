using SKStudios.Common.Utils;
using UnityEngine;

namespace SKStudios.Portals
{
    public class BasicFPSExample : MonoBehaviour
    {
        //private Vector2 _mouseAbsolute;
        private Rigidbody _rigidbody;

        private Vector2 _smoothMouse;

        // Assign this if there's a parent object controlling motion, such as a Character Controller.
        // Yaw rotation will affect this object instead of the camera if set.
        public GameObject characterBody;

        public Vector2 clampInDegrees = new Vector2(360, 180);
        private float distanceMoved;

        public GameObject eyeParent;
        public Camera headCam;
        public Transform body;
        public GameObject headset;
        private Quaternion lastBodyRot;
        public bool lockCursor;
        public float moveSpeed = 100f;
        private Vector3 moveStartPosition;

        private bool moving;
        private bool paused;
        public Collider playerPhysBounds;
        public Vector2 sensitivity = new Vector2(2, 2);
        public Vector2 smoothing = new Vector2(3, 3);
        public Vector2 targetCharacterDirection;
        public Vector2 targetDirection;
        private float totalTime;

        private void Start()
        {
            // Set target direction to the camera's initial orientation.
            targetDirection = transform.localRotation.eulerAngles;
            // Set target direction for the character body to its inital state.
            if (characterBody) targetCharacterDirection = characterBody.transform.localRotation.eulerAngles;
            _rigidbody = GetComponent<Rigidbody>();
        }

        private void OnEnable()
        {
            playerPhysBounds.enabled = true;
            playerPhysBounds.transform.localPosition +=
                new Vector3(0, playerPhysBounds.GetComponent<Collider>().bounds.extents.y, 0);
        }


        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
                paused = !paused;

            Cursor.lockState = !paused ? CursorLockMode.Locked : CursorLockMode.None;
            if (paused) return;
            // Allow the script to clamp based on a desired target value.
            var targetOrientation = Quaternion.Euler(targetDirection);
            var targetCharacterOrientation = Quaternion.Euler(targetCharacterDirection);

            // Get raw mouse input for a cleaner reading on more sensitive mice.
            var mouseDelta = new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y"));

            Vector3 euler = headset.transform.parent.localEulerAngles;
            int phi = Mathf.Abs((int)euler.x - 0) % 360;       // This is either the distance or 360 - distance
            int distance = phi > 180 ? 360 - phi : phi;

            // Scale input against the sensitivity setting and multiply that against the smoothing value.
            mouseDelta = Vector2.Scale(mouseDelta,
                new Vector2(sensitivity.y * smoothing.x, sensitivity.x * smoothing.y));

            // Interpolate mouse movement over time to apply smoothing delta.
            _smoothMouse.x = Mathf.Lerp(_smoothMouse.x, mouseDelta.x, 1f / smoothing.x);
            _smoothMouse.y = Mathf.Lerp(_smoothMouse.y, mouseDelta.y, 1f / smoothing.y);

            var rotation = Quaternion.AngleAxis(-_smoothMouse.y, Vector3.right);
            float halfclamp = (clampInDegrees.y / 2);
            rotation *= Quaternion.AngleAxis(_smoothMouse.x * ((halfclamp - distance + 25) / (halfclamp + 25)), Vector3.up);

            headset.transform.parent.localRotation *= rotation;

            Vector3 newEuler = headset.transform.parent.localEulerAngles;
            phi = Mathf.Abs((int)newEuler.x - 0) % 360;       // This is either the distance or 360 - distance
            int newDistance = phi > 180 ? 360 - phi : phi;

            if (distance > halfclamp && newDistance >= distance)
                newEuler.x = euler.x;

            headset.transform.parent.localEulerAngles = new Vector3(newEuler.x, newEuler.y, 0);
            Quaternion targetBodyRot = Quaternion.Euler(0, newEuler.y, 0);
            body.transform.localRotation =
                Quaternion.Lerp(body.transform.localRotation,
                    targetBodyRot, 
                        Time.deltaTime * 3);
        }

        void LateUpdate()
        {
            _rigidbody.velocity = new Vector3(0, _rigidbody.velocity.y, 0);
            var dirVector = headCam.transform.forward;
            dirVector.y = 0;
            dirVector = dirVector.normalized * moveSpeed;
            //Keyboard input
            if (Input.GetKey(KeyCode.LeftShift)) dirVector *= 2f;

            if (Input.GetKey(KeyCode.W))
            {
                // transform.position += dirVector * transform.lossyScale.magnitude * Time.deltaTime;
                _rigidbody.velocity += dirVector * transform.lossyScale.x * Time.fixedDeltaTime;
            }

            if (Input.GetKey(KeyCode.A))
            {
                // transform.position += Quaternion.AngleAxis(-90, Vector3.up) * dirVector * transform.lossyScale.magnitude * Time.deltaTime;
                _rigidbody.velocity += Quaternion.AngleAxis(-90, Vector3.up) * dirVector * transform.lossyScale.x *
                                       Time.fixedDeltaTime;
            }

            if (Input.GetKey(KeyCode.S))
            {
                //transform.position += Quaternion.AngleAxis(180, Vector3.up) * dirVector * transform.lossyScale.magnitude * Time.deltaTime;
                _rigidbody.velocity += Quaternion.AngleAxis(180, Vector3.up) * dirVector * transform.lossyScale.x *
                                       Time.fixedDeltaTime;
            }

            if (Input.GetKey(KeyCode.D))
            {
                // transform.position += Quaternion.AngleAxis(90, Vector3.up) * dirVector * transform.lossyScale.magnitude * Time.deltaTime;
                _rigidbody.velocity += Quaternion.AngleAxis(90, Vector3.up) * dirVector * transform.lossyScale.x *
                                       Time.fixedDeltaTime;
            }

        }
    }
}
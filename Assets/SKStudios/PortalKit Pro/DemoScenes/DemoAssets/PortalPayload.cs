using System.Collections;
using SKStudios.Common.Utils;
using UnityEngine;

namespace SKStudios.Portals.Demos {
    public class PortalPayload : MonoBehaviour {
        private Coroutine _spawn;
        public float OpenTime;
        public GameObject PortalObj;
        public Vector3 PortalSize;
        public float ShotSpeed;
        [HideInInspector] public bool Travelling;


        private void Update() {
            if (!Travelling) return;

            transform.position += transform.forward * ShotSpeed;

            RaycastHit hit;
            var ray = new Ray(transform.position, transform.forward);
            if (Physics.Raycast(ray, out hit, 1f, ~0, QueryTriggerInteraction.Ignore)) {
                Travelling = false;
                if (hit.collider.gameObject.layer != LayerMask.NameToLayer("Water")) return;


                //Check down to adjust portal upwards if it's going to be halfway in the ground
                ray = new Ray(hit.point + hit.normal * 0.01f, Vector3.down);
                RaycastHit downHit;
                if (Physics.Raycast(ray, out downHit, 5f, 1 << LayerMask.NameToLayer("Water"),
                    QueryTriggerInteraction.Ignore)) {
                    PortalObj.transform.position = downHit.point + hit.normal * 0.1f +
                                                   new Vector3(0, PortalSize.y / 2f, 0);
                    PortalObj.transform.rotation = Quaternion.LookRotation(-hit.normal, transform.up);
                    if (_spawn != null)
                        StopCoroutine(_spawn);
                    _spawn = StartCoroutine(SpawnPortal());
                }
                //If not close to the floor, place on wall
                else {
                    PortalObj.transform.position = hit.point + hit.normal * 0.1f;
                    PortalObj.transform.rotation = Quaternion.LookRotation(-hit.normal, transform.up);
                    if (_spawn != null)
                        StopCoroutine(_spawn);
                    _spawn = StartCoroutine(SpawnPortal());
                }
            }
        }

        private IEnumerator SpawnPortal() {
            PortalObj.SetActive(true);
            var start = Time.time;
            var elapsedTime = Time.time - start;
            var controller = PortalObj.GetComponent<PortalController>();
            while (elapsedTime < OpenTime) {
                elapsedTime = Time.time - start;
                var timeScalar = elapsedTime / OpenTime;
                var portalScale = Mathfx.Berp(Vector3.one * 0.01f, PortalSize, timeScalar);
                ;
                //controller.Portal.transform.localScale = portalScale;
                controller.PortalOpeningSize = portalScale;
                yield return WaitCache.Frame;
            }

            gameObject.SetActive(false);
            Travelling = false;
        }
    }
}
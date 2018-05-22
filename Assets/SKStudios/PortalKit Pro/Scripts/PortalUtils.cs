using System;
using Eppy;
using SKStudios.Common.Extensions;
using UnityEngine;

namespace SKStudios.Portals {
    /// <summary>
    /// Utilities utilized heavily by <see cref="Portal"/>s and related classes.
    /// </summary>
    public static class PortalUtils {
        /// <summary>
        ///     Allows teleportation of gameobjects from an Origin to a ArrivalTarget. It teleports the object
        ///     by its root, which if unspecified is the root of the game object. Objects can be teleported
        ///     in reference to another gameobject, which is useful for dopplegangers
        /// </summary>
        /// <param name="origin">The Origin of the Portal</param>
        /// <param name="destination">The ArrivalTarget of the Portal</param>
        /// <param name="teleportedObject">The Object to teleport</param>
        /// <param name="deepestRoot">The deepest root to teleport the object by</param>
        /// <param name="reference">The transform to teleport the object in reference to</param>
        public static void TeleportObject(GameObject teleportedObject, Transform origin, Transform destination,
            Transform deepestRoot = null, Transform reference = null, bool physics = true, bool scale = true) {
            if (!destination)
                throw new ArgumentException();

            Rigidbody rigidBody;
            var oldMode = CollisionDetectionMode.Discrete;
            rigidBody = teleportedObject.SKGetComponentOnce<Rigidbody>();
            if (!deepestRoot)
                deepestRoot = teleportedObject.transform.root;
            if (!reference)
                reference = deepestRoot;

            var oldScale = deepestRoot.localScale;
            var diffQuat = destination.rotation * Quaternion.Inverse(origin.rotation);
            var localPos = origin.InverseTransformPoint(reference.position);
            deepestRoot.position = destination.TransformPoint(localPos);
            deepestRoot.rotation = diffQuat * reference.rotation;
            var newScale = reference.localScale;
            if (scale) {
                newScale.Scale(new Vector3(1f / origin.transform.lossyScale.x, 1f / origin.transform.lossyScale.y,
                    1f / origin.transform.lossyScale.z));
                newScale.Scale(destination.transform.lossyScale);
                deepestRoot.localScale = newScale;
            }


            if (rigidBody) {
                rigidBody.WakeUp();
                if (physics) {
                    rigidBody.velocity =
                        destination.TransformVector(
                            origin.InverseTransformVector(rigidBody
                                .velocity));

                    rigidBody.angularVelocity =
                        destination.TransformVector(origin.InverseTransformVector(rigidBody.angularVelocity));
                }
            }
        }

        /// <summary>
        ///     Returns a distance from a given point to a given plane
        /// </summary>
        /// <param name="normal">Normal of the plane</param>
        /// <param name="Q">Point on the plane</param>
        /// <param name="P">Point to check distance of</param>
        /// <returns></returns>
        public static float PlanePointDistance(Vector3 normal, Vector3 Q, Vector3 P) {
            //Vector3 P = ;
            //Since we are handling this in local space, Q which is also the start of N and the transform's point is equal to 0
            //Vector3 Q = Vector3.zero;
            // P = transform.position - P;
            var V = P - Q;
            return Vector3.Dot(V, normal);
        }

        /// <summary>
        ///     Returns the hit and final cast ray of a castable through-Portal ray.
        /// </summary>
        /// <param name="ray"></param>
        /// <param name="maxdistance"></param>
        /// <param name="layerMask"></param>
        /// <param name="triggerInteraction"></param>
        /// <returns></returns>
        public static Tuple<Ray, RaycastHit> TeleportableRaycast(Ray ray, float maxdistance, LayerMask layerMask,
            QueryTriggerInteraction triggerInteraction = QueryTriggerInteraction.Ignore) {
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, maxdistance, layerMask, triggerInteraction)) {
                var portal = hit.collider.gameObject.SKGetComponentOnce<Portal>();
                if (portal) {
                    //Emit a delegate raycast from the otherRoot Portal
                    var rotation = portal.ArrivalTarget.rotation *
                                   (Quaternion.Inverse(portal.transform.rotation) *
                                    Quaternion.LookRotation(ray.direction, Vector3.up));
                    Vector3 newPoint;
                    newPoint = portal.Origin.InverseTransformPoint(hit.point);
                    newPoint = portal.ArrivalTarget.TransformPoint(newPoint);
                    Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.yellow);
                    var newDir = rotation * Vector3.forward;

                    var newRay = new Ray(newPoint + newDir * 0.001f, newDir);
                    return TeleportableRaycast(newRay, maxdistance - hit.distance, layerMask, triggerInteraction);
                }

                //Hit an object otherRoot than a Portal
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.blue);
                return new Tuple<Ray, RaycastHit>(ray, hit);
            }

            //Didn't hit anything
            Debug.DrawRay(ray.origin, ray.direction * maxdistance, Color.red);
            return new Tuple<Ray, RaycastHit>(ray, hit);
        }

        /// <summary>
        ///     Returns an array of vertices which can be used for approximate calculations forp a 3d flat mesh.
        /// </summary>
        /// <param name="refMesh"></param>
        /// <returns></returns>
        public static Vector3[] BackCheckVerts(Mesh refMesh) {
            var modelVerts = refMesh.vertices;
            var checkedVerts = new Vector3[modelVerts.Length + 1];
            var middlevert = Vector3.zero;
            for (var i = 0; i < checkedVerts.Length; i++)
                //Outside verts
                if (i < checkedVerts.Length / 2) {
                    middlevert += modelVerts[i];
                    checkedVerts[i] = modelVerts[i];
                }
                //Inside verts
                else if (!(i + 1 == checkedVerts.Length)) {
                    middlevert += modelVerts[i];
                    checkedVerts[i] = modelVerts[i] / 2;
                }
                //Checks the midpoint
                else {
                    checkedVerts[i] = middlevert / modelVerts.Length;
                }

            return checkedVerts;
        }

        /// <summary>
        ///     Returns an array of vertices which can be used for approximate calculations forp a 3d flat mesh.
        /// </summary>
        /// <param name="calling"> the portal that called this</param>
        /// <returns></returns>
        public static Vector3[] DownCheckVerts(this Portal calling) {
            var checkedVerts = new Vector3[5];
            var middlevert = Vector3.zero;
            var scale = calling.DetectionZone.transform.lossyScale;
            checkedVerts[0] = new Vector3(scale.x / 2f, scale.y / 2f, 0);
            checkedVerts[1] = new Vector3(-scale.x / 2f, scale.y / 2f, 0);
            checkedVerts[2] = new Vector3(scale.x / 2f, scale.y / 2f, scale.z / 2f);
            checkedVerts[3] = new Vector3(-scale.x / 2f, scale.y / 2f, scale.z / 2f);
            for (var i = 0; i < checkedVerts.Length - 1; i++)
                middlevert += checkedVerts[i];
            checkedVerts[checkedVerts.Length - 1] = middlevert / (checkedVerts.Length - 1);
            return checkedVerts;
        }

        /// <summary>
        ///     Convenience method for extracting submesh data out of mesh. Useful for checking if a Portal can be placed on a
        ///     wall.
        /// </summary>
        /// <param name="refMesh"></param>
        /// <param name="triangleIndex"></param>
        /// <returns></returns>
        public static int SubmeshIndexOfTriangle(Mesh refMesh, int triangleIndex) {
            for (var i = 0; i < refMesh.subMeshCount; i++) {
                var tris = refMesh.GetTriangles(i);
                for (var u = 0; u < tris.Length; u++)
                    if (u == triangleIndex)
                        return i;
            }

            return -1;
        }
    }
}
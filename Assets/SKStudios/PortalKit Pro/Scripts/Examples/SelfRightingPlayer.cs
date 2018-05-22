using UnityEngine;

public class SelfRightingPlayer : MonoBehaviour {
    private Rigidbody _body;
    public float moveSpeed = 2f;

    // Use this for initialization
    private void Start() {
        _body = gameObject.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update() {
        //Get the forward direction of the capsule solely on the XZ plane for rotational calculation
        var forward = transform.forward;
        forward = new Vector3(forward.x, 0, forward.z).normalized;

        //rotate the rotation closer to true up based on MoveSpeed
        var newRot = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(
                forward,
                Vector3.up), Time.deltaTime * moveSpeed);

        //Apply rotation to player
        if (!_body) {
            transform.rotation = newRot;
        }
        else {
            //Prevent this corrective rotation from applying angular velocity to the player if the player has a rigidbody attached
            var velocity = _body.velocity;
            _body.isKinematic = true;
            transform.rotation = newRot;
            _body.isKinematic = false;
            _body.velocity = velocity;
        }
    }
}
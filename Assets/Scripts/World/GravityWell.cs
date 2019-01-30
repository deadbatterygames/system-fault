using UnityEngine;

//
// GravityWell.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Pulls all Rigidbodies in the SceneManager towards itself
//

public class GravityWell : MonoBehaviour {

    [SerializeField] float gravityStrength = 100f;

    void FixedUpdate() {
        PullObjects();
	}

    void PullObjects() {
        foreach (Rigidbody rb in GameManager.instance.gravityBodies) {
            if (rb.useGravity && !rb.isKinematic) {
                Vector3 difference = transform.position - rb.transform.position;
                Vector3 gravity = (transform.position - rb.transform.position).normalized
                    / (difference.magnitude * difference.magnitude);
                rb.AddForce(gravity * gravityStrength * GameManager.GRAVITY_CONSTANT, ForceMode.Acceleration);
            }
        }
    }
}

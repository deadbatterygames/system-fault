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
            if (rb && rb.useGravity) {
                Vector3 gravity = (transform.position - rb.transform.position).normalized
                    / Mathf.Pow(Vector3.Distance(transform.position, rb.transform.position), 2f);
                rb.AddForce(gravity * gravityStrength * GameManager.GRAVITY_CONSTANT, ForceMode.Acceleration);
            }
        }
    }
}

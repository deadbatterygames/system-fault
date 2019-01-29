using UnityEngine;

//
// Missile.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Moves towards target (if any) and explodes
//

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class Missile : MonoBehaviour {

    Rigidbody rb;

    static float thrust = 50f;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        rb.AddForce(transform.up * thrust, ForceMode.Acceleration);
    }
}

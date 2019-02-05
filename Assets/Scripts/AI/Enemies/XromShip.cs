using UnityEngine;

public class XromShip : MonoBehaviour {

    Rigidbody rb;

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate() {
        rb.AddForce(transform.forward, ForceMode.Acceleration);
    }
}

using UnityEngine;
using System.Collections;

//
// Missile.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Moves towards target (if any) and explodes
//

public class Missile : MonoBehaviour {

    Rigidbody rb;
    CapsuleCollider capsule;

    const float THRUST = 50f;

    void Start() {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        if (!rb) Debug.LogError("Missile: No Rigidbody attached to missile");
        if (!capsule) Debug.LogError("Missile: No Rigidbody attached to missile");
    }

    void FixedUpdate() {
        if (rb) rb.AddForce(transform.up * THRUST, ForceMode.Acceleration);
    }

    void OnCollisionEnter(Collision collision) {
        StartCoroutine("DestroyMissile");
    }

    IEnumerator DestroyMissile() {
        Destroy(GetComponent<MeshRenderer>());
        Destroy(GetComponentInChildren<ParticleSystem>());
        Destroy(GetComponent<CapsuleCollider>());
        Destroy(rb);

        Instantiate(GameData.instance.explosionPrefab, transform.position, transform.rotation);

        yield return new WaitForSeconds(GetComponentInChildren<TrailRenderer>().time);

        Destroy(gameObject);
    }
}

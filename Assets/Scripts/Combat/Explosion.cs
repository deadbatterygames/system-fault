using System.Collections;
using UnityEngine;

//
// Explosion.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: An explosion that damages anything in a certain radius
//

public class Explosion : MonoBehaviour {

    static float damage = 500f;
    static float force = 100f;

    float damageRadius;

    void Start() {
        SphereCollider sphere = GetComponent<SphereCollider>();
        if (!sphere) Debug.LogError("Explosion: No sphere collider attached to explosion");

        damageRadius = sphere.radius;
        Destroy(sphere);
        StartCoroutine("Explode");
    }

    IEnumerator Explode() {
        Collider[] colliders = Physics.OverlapSphere(transform.position, damageRadius);
        foreach (Collider col in colliders) {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb) rb.AddExplosionForce(force, transform.position, damageRadius, 0f, ForceMode.Impulse);

            IDamageable damageable = col.GetComponentInParent<IDamageable>();
            if (damageable != null) {
                Vector3 centerToObject = col.transform.position - transform.position;

                Vector3 damageForce = centerToObject.normalized * force;
                float damageAmount = damage / centerToObject.magnitude;

                damageable.Damage(damageAmount, GameTypes.DamageType.Physical, damageForce);
            }
        }

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}

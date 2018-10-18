using UnityEngine;
using System.Collections;

//
// BulletScript.cs
//
// Author: Eric Thompson & Gabriel Cimolino (Dead Battery Games)
// Purpose: Deals damage to damageable interfaces
//

public class Bullet : MonoBehaviour {

    [SerializeField] GameTypes.DamageType damageType;
    [SerializeField] float bulletDamage = 10f;
    [SerializeField] float bulletLifetime = 3f;

    ParticleSystem explosion;

    void Awake() {
        explosion = GetComponentInChildren<ParticleSystem>();
        StartCoroutine("WaitThenExplode");
    }

    void OnCollisionEnter(Collision collision) {
        IDamageable damageableObject = collision.collider.GetComponent<IDamageable>();
        if (damageableObject != null) damageableObject.Damage(bulletDamage, -collision.relativeVelocity * GetComponent<Rigidbody>().mass);

        StartCoroutine("Explode");
    }

    IEnumerator WaitThenExplode() {
        yield return new WaitForSeconds(bulletLifetime);
        StartCoroutine("Explode");
    }

    IEnumerator Explode() {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        explosion.Play();

        yield return new WaitForSeconds(explosion.main.duration);

        Destroy(gameObject);
    }
}

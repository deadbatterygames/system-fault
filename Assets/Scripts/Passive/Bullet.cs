using UnityEngine;

//
// Bullet.cs
//
// Author: Eric Thompson & Gabriel Cimolino (Dead Battery Games)
// Purpose: Deals damage to damageable interfaces
//

public class Bullet : MonoBehaviour {

    [SerializeField] GameTypes.DamageType damageType;
    [SerializeField] float bulletDamage = 10f;

    ParticleSystem explosion;

    void Awake() {
        explosion = GetComponentInChildren<ParticleSystem>();
    }

    void OnCollisionEnter(Collision collision) {
        IDamageable damageableObject = collision.collider.GetComponent<IDamageable>();
        if (damageableObject != null) damageableObject.Damage(bulletDamage, -collision.relativeVelocity * GetComponent<Rigidbody>().mass);

        Explode();
    }

    void Explode() {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        explosion.Play();
    }

    public void RecycleBullet() {
        GetComponent<Rigidbody>().isKinematic = false;

        explosion.Stop();

        GetComponent<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;
    }
}

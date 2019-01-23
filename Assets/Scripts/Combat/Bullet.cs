using UnityEngine;

//
// Bullet.cs
//
// Author: Eric Thompson & Gabriel Cimolino (Dead Battery Games)
// Purpose: Deals damage to damageable interfaces
//

public class Bullet : MonoBehaviour {

    [SerializeField] GameTypes.DamageType damageType;
    [SerializeField] float baseDamage = 10f;

    ParticleSystem explosion;
    Rigidbody rb;

    void Awake() {
        enabled = true;
        explosion = GetComponentInChildren<ParticleSystem>();
        rb = GetComponent<Rigidbody>();
        if (GetComponent<Rigidbody>().collisionDetectionMode != CollisionDetectionMode.Continuous)
            Debug.LogError("Bullet: " + gameObject.name + "'s collision detection is not set to Continuous");

        
    }

    void OnCollisionEnter(Collision collision) {
        IDamageable damageableObject = collision.collider.GetComponent<IDamageable>();
        if (damageableObject != null) damageableObject.Damage(baseDamage, damageType, -collision.relativeVelocity * GetComponent<Rigidbody>().mass);
        Explode();
    }

    public void Explode() {
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.drag = 5f;

        GetComponentInChildren<MeshRenderer>().enabled = false;
        GetComponent<Collider>().enabled = false;

        enabled = false;

        explosion.Play();
    }

    public void ResetBullet() {
        rb.drag = 0f;

        explosion.Stop();

        GetComponentInChildren<MeshRenderer>().enabled = true;
        GetComponent<Collider>().enabled = true;

        enabled = true;
    }

    public GameTypes.DamageType GetDamageType() {
        return damageType;
    }
}

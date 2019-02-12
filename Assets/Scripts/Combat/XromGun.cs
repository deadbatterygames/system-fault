using UnityEngine;

//
// XromGun.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Weapon used by the Xroms and Xrom Ships
//

public class XromGun : MonoBehaviour, IWeapon {

    [SerializeField] float bulletVelocity = 150f;

    ParticleSystem particles;

    void Start() {
        particles = GetComponent<ParticleSystem>();
    }

    public void Fire() {
        FireBullet(GameData.instance.GetBullet(GameTypes.DamageType.Purple).GetComponent<Bullet>(), transform.forward, bulletVelocity);
    }

    void FireBullet(Bullet bullet, Vector3 fireDirection, float bulletVelocity) {
        // Position
        bullet.transform.position = transform.position;

        // Rigidbody
        Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();
        bulletRB.angularVelocity = Vector3.zero;
        bulletRB.velocity = GetComponentInParent<Rigidbody>().velocity + fireDirection * bulletVelocity;

        // Bullet Recycle
        bullet.RecycleBullet();

        // Particles
        particles.Play();
    }
}

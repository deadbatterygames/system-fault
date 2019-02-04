using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempGun : MonoBehaviour, IWeapon {

    [SerializeField] public Transform firePoint;

    [SerializeField] GameObject bulletPrefab;
    [SerializeField] ParticleSystem particles;

    [SerializeField] float bulletVelocity = 150f;
    [SerializeField] int maxBullets = 20;

    //[SerializeField] ParticleSystem particles;

    List<GameObject> bullets = new List<GameObject>();
    int bulletIndex;

    //GameTypes.DamageType currentWeaponType = GameTypes.DamageType.Yellow;

    bool recoil;
    bool resetWeapon;
    Vector3 resetPosition;
    Quaternion resetRotation;

    public void Start() {
        //StartCoroutine("FirePattern");
    }

    public void Fire() {
        FireBullet(MakeBullet(), firePoint.forward, bulletVelocity);
        //particles.Play();
    }

    GameObject MakeBullet() {
        if (bullets.Count >= maxBullets) {
            // Recycle bullet
            int index = CycleBulletIndex();
            bullets[index].GetComponent<Bullet>().ResetBullet();
            return bullets[index];
        } else {
            // Make new bullet
            GameObject newBullet = Instantiate(bulletPrefab);
            bullets.Add(newBullet);
            return newBullet;
        }
    }

    int CycleBulletIndex() {
        if (bulletIndex >= maxBullets - 1) bulletIndex = 0;
        else bulletIndex++;
        return bulletIndex;
    }

    void FireBullet(GameObject bullet, Vector3 fireDirection, float bulletVelocity) {
        // Position
        bullet.transform.position = firePoint.position;

        // Rigidbody
        Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();
        bulletRB.angularVelocity = Vector3.zero;
        bulletRB.velocity = GetComponentInParent<Rigidbody>().velocity + fireDirection * bulletVelocity;

        // Particles
        particles.Play();
    }

    IEnumerator FirePattern() {
        for (int i = 0; i < 5f; i++) {
            Fire();
            yield return new WaitForSeconds(1f);
        }

        yield return new WaitForSeconds(1f);

        StartCoroutine("FirePattern");
    }
}

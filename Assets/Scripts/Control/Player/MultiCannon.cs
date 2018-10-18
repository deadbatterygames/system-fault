using UnityEngine;
using UnityEngine.UI;

//
// MultiCannon.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Fires multiple weapon types based on attachments collected
//

public class MultiCannon : MonoBehaviour, IWeapon {

    [Header("General")]
    [SerializeField] Transform mesh;
    [SerializeField] Transform firePoint;
    [SerializeField] Transform recoilPoint;
    [SerializeField] Transform loweredPoint;

    [Space]
    [SerializeField] float aimRange = 150f;
    [SerializeField] float recoilSpeed = 10000f;
    [SerializeField] float resetSpeed = 10f;

    [Header("Weapon Types")]
    [SerializeField] Image[] weaponTypeIndicators;

    [Space]
    [SerializeField] GameObject[] bullets;
    [SerializeField] float[] bulletVelocities;

    [Space]
    [SerializeField] Material[] lightMaterials;

    [Header("Particles")]
    [SerializeField] ParticleSystem[] particles;

    GameTypes.DamageType currentWeaponType = GameTypes.DamageType.Pulse;

    bool recoil;
    bool resetWeapon;
    Vector3 resetPosition;
    Quaternion resetRotation;

    void Awake() {
        resetPosition = mesh.localPosition;
        resetRotation = mesh.localRotation;
    }

    public void Fire() {
        switch (currentWeaponType) {
            case GameTypes.DamageType.Pulse:
                FirePulse();
                break;
        }

        recoil = true;
        resetWeapon = false;
    }

    void Update() {
        if (recoil) Recoil();
        if (resetWeapon) ResetWeapon();
    }

    void Recoil() {
        if (mesh.localPosition != recoilPoint.localPosition || mesh.localRotation != recoilPoint.localRotation) {
            mesh.localPosition = Vector3.MoveTowards(mesh.localPosition, recoilPoint.localPosition, recoilSpeed / 30f * Time.deltaTime);
            mesh.localRotation = Quaternion.RotateTowards(mesh.localRotation, recoilPoint.localRotation, recoilSpeed * Time.deltaTime);
        } else {
            recoil = false;
            resetWeapon = true;
        }
    }

    void ResetWeapon() {
        if (mesh.localPosition != resetPosition || mesh.localRotation != resetRotation) {
            mesh.localPosition = Vector3.Lerp(mesh.localPosition, resetPosition, resetSpeed * Time.deltaTime);
            mesh.localRotation = Quaternion.Lerp(mesh.localRotation, resetRotation, resetSpeed * Time.deltaTime);
        } else {
            resetWeapon = false;
        }
    }

    void FirePulse() {
        SpawnBullet(bullets[0], GetFireDirection(), bulletVelocities[0]);
        particles[0].Play();
    }

    Vector3 GetFireDirection() {
        RaycastHit? hit = PlayerCamera.instance.GetPhysicalTarget(aimRange);
        if (hit != null && (hit.Value.point - firePoint.position).magnitude > 2f)
            return (hit.Value.point - firePoint.position).normalized;
        else {
            return firePoint.forward;
        }
    }

    void SpawnBullet(GameObject bulletPrefab, Vector3 fireDirection, float bulletVelocity) {
        Rigidbody bulletRB = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation).GetComponent<Rigidbody>();
        bulletRB.velocity = GetComponentInParent<Rigidbody>().velocity + fireDirection * bulletVelocity;
    }
}

using UnityEngine;
using UnityEngine.UI;
using System.Collections;

//
// MultiCannon.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Fires multiple weapon types based on attachments collected
//

public class Multicannon : MonoBehaviour, IWeapon {

    [Header("General")]
    [SerializeField] Transform meshTransform;
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] Transform firePoint;
    [SerializeField] Transform recoilPoint;
    [SerializeField] Transform loweredPoint;

    [Space]
    [SerializeField] float aimRange = 150f;
    [SerializeField] float recoilSpeed = 10000f;
    [SerializeField] float resetSpeed = 10f;

    [Header("Weapon Types")]
    [SerializeField] Image[] weaponTypeIndicators;

    [SerializeField] float[] typeCooldowns;

    [Space]
    [SerializeField] float[] bulletVelocities;

    [Space]
    [SerializeField] Light[] lights;
    [SerializeField] Material[] lightMaterials;
    

    [Header("Particles")]
    [SerializeField] ParticleSystem[] particles;

    GameTypes.DamageType currentWeaponType = GameTypes.DamageType.Yellow;

    bool canFire = true;

    float yellowFireForce = 10f;

    bool recoil;
    bool resetWeapon;
    Vector3 resetPosition;
    Quaternion resetRotation;

    void Awake() {
        resetPosition = meshTransform.localPosition;
        resetRotation = meshTransform.localRotation;
    }

    public void Fire() {
        if (canFire) {
            StartCoroutine(Cooldown(typeCooldowns[(int)currentWeaponType]));

            switch (currentWeaponType) {
                case GameTypes.DamageType.Yellow:
                    FireYellow();
                    break;
                case GameTypes.DamageType.Blue:
                    FireBlue();
                    break;
                case GameTypes.DamageType.Red:
                    FireRed(); // Best Pokemon game
                    break;
            }

            recoil = true;
            resetWeapon = false;
        }
    }

    void Update() {
        if (recoil) Recoil();
        if (resetWeapon) ResetWeapon();
    }

    void Recoil() {
        if (meshTransform.localPosition != recoilPoint.localPosition || meshTransform.localRotation != recoilPoint.localRotation) {
            meshTransform.localPosition = Vector3.MoveTowards(meshTransform.localPosition, recoilPoint.localPosition, recoilSpeed / 30f * Time.deltaTime);
            meshTransform.localRotation = Quaternion.RotateTowards(meshTransform.localRotation, recoilPoint.localRotation, recoilSpeed * Time.deltaTime);
        } else {
            recoil = false;
            resetWeapon = true;
        }
    }

    void ResetWeapon() {
        if (meshTransform.localPosition != resetPosition || meshTransform.localRotation != resetRotation) {
            meshTransform.localPosition = Vector3.Lerp(meshTransform.localPosition, resetPosition, resetSpeed * Time.deltaTime);
            meshTransform.localRotation = Quaternion.Lerp(meshTransform.localRotation, resetRotation, resetSpeed * Time.deltaTime);
        } else {
            resetWeapon = false;
        }
    }

    public void ChangeType(GameTypes.DamageType type) {
        if (GameData.instance.IsTypeUnlocked(type)) {
            WeaponSlot slot = GetComponentInParent<WeaponSlot>();
            if (slot.GetCurrentWeaponType() != GameTypes.PlayerWeaponType.Multicannon) slot.ChangeEquipment();

            switch (type) {
                case GameTypes.DamageType.Yellow:
                    weaponTypeIndicators[0].enabled = true;
                    weaponTypeIndicators[1].enabled = false;
                    weaponTypeIndicators[2].enabled = false;
                    break;
                case GameTypes.DamageType.Blue:
                    weaponTypeIndicators[0].enabled = false;
                    weaponTypeIndicators[1].enabled = true;
                    weaponTypeIndicators[2].enabled = false;
                    break;
                case GameTypes.DamageType.Red:
                    weaponTypeIndicators[0].enabled = false;
                    weaponTypeIndicators[1].enabled = false;
                    weaponTypeIndicators[2].enabled = true;
                    break;
                default:
                    Debug.LogError("MultiCannon: Invalid type requested");
                    break;
            }

            // Lights
            ChangeLight(type);
            Material[] mats = meshRenderer.materials;
            mats[0] = lightMaterials[(int)type];
            meshRenderer.materials = mats;

            currentWeaponType = type;
        }
    }

    void FireYellow() {
        FireBullet(GameData.instance.GetBullet(GameTypes.DamageType.Yellow).GetComponent<Bullet>(), GetFireDirection(), bulletVelocities[0]);
        particles[0].Play();
    }

    void FireBlue() {
        FireBullet(GameData.instance.GetBullet(GameTypes.DamageType.Blue).GetComponent<Bullet>(), GetFireDirection(), bulletVelocities[1]);
        particles[1].Play();
    }

    void FireRed() {
        FireBullet(GameData.instance.GetBullet(GameTypes.DamageType.Red).GetComponent<Bullet>(), GetFireDirection(), bulletVelocities[2], false);
        particles[2].Play();
    }

    void ChangeLight(GameTypes.DamageType type) {
        switch (type) {
            case GameTypes.DamageType.Yellow:
                lights[0].enabled = true;
                lights[1].enabled = false;
                lights[2].enabled = false;
                break;
            case GameTypes.DamageType.Blue:
                lights[0].enabled = false;
                lights[1].enabled = true;
                lights[2].enabled = false;
                break;
            case GameTypes.DamageType.Red:
                lights[0].enabled = false;
                lights[1].enabled = false;
                lights[2].enabled = true;
                break;
            default:
                Debug.LogError("MultiCannon: Invalid type requested");
                break;
        }
    }

    Vector3 GetFireDirection() {
        RaycastHit? hit = PlayerCamera.instance.GetPhysicalTarget(aimRange);
        if (hit != null && (hit.Value.point - firePoint.position).magnitude > 2f)
            return (hit.Value.point - firePoint.position).normalized;
        else {
            return firePoint.forward;
        }
    }

    void FireBullet(Bullet bullet, Vector3 fireDirection, float bulletVelocity, bool addPlayerVelocity = true) {
        bullet.transform.position = firePoint.position;
        bullet.transform.rotation = firePoint.rotation;

        // Rigidbody
        Rigidbody bulletRB = bullet.GetComponent<Rigidbody>();

        // Velocity
        bulletRB.velocity = fireDirection * bulletVelocity;
        if (addPlayerVelocity) bulletRB.velocity += GetComponentInParent<Rigidbody>().velocity;

        // Rotation
        switch (bullet.GetDamageType()) {
            case GameTypes.DamageType.Blue:
                bulletRB.AddTorque(Random.Range(-20f, 20f), Random.Range(-20f, 20f), Random.Range(-20f, 20f), ForceMode.VelocityChange);
                break;
            case GameTypes.DamageType.Red:
                bulletRB.angularVelocity = firePoint.forward * -10f;
                break;
            default:
                break;
        }

        bullet.RecycleBullet();
    }

    IEnumerator Cooldown(float time) {
        canFire = false;
        yield return new WaitForSeconds(time);
        canFire = true;
    }
}

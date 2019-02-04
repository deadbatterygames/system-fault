using UnityEngine;

//
// LaserCannon.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Shoots lasers
//

public class LaserCannon : ShipModule, IWeapon {

    [Header("Cannon Specs")]
    [SerializeField] float damage = 5f;
    [SerializeField] float fireFrequency = 0.25f;
    [SerializeField] float laserForce = 10f;
    [SerializeField] float aimRange = 1000f;

    [Header("Laser Positions")]
    [SerializeField] LineRenderer leftLaser;
    [SerializeField] LineRenderer rightLaser;
    [SerializeField] Transform leftFirePoint;
    [SerializeField] Transform rightFirePoint;

    Vector3[] leftLaserPositions = new Vector3[2];
    Vector3[] rightLaserPositions = new Vector3[2];

    bool canFire = true;
    float laserAlpha = 0f;

    Material laserMaterial;

    protected override void Awake() {
        base.Awake();
        moduleType = GameTypes.ModuleType.LaserCannon;

        laserMaterial = leftLaser.material;
        UpdateLaserOpacity();
    }

    public Vector3 GetAimPoint() {
        RaycastHit? hit = PlayerCamera.instance.GetPhysicalTarget(aimRange);
        if (hit != null && Vector3.Angle(PlayerCamera.instance.transform.forward, transform.up) < 10f) return hit.Value.point;
        else return transform.position + transform.up * aimRange;
    }

    public Vector3 GetHitPoint(Vector3 firePoint, Vector3 aimPoint) {
        RaycastHit hit;
        if (Physics.Raycast(firePoint, (aimPoint - firePoint).normalized, out hit)) {
            IDamageable damageable = hit.collider.GetComponentInParent<IDamageable>();
            if (damageable != null) damageable.Damage(damage, GameTypes.DamageType.Yellow, (aimPoint - firePoint).normalized * laserForce);
            Instantiate(PlayerData.instance.laserParticles, hit.point, Quaternion.identity);
            return hit.point;
        } else return aimPoint;
    }

    void Update() {
        if (!canFire) {
            leftLaserPositions[0] = leftFirePoint.position;
            rightLaserPositions[0] = rightFirePoint.position;
            UpdateLaserPositions();
            UpdateLaserOpacity();
        }
    }

    public void Fire() {
        if (canFire) {
            // Make visible
            ToggleLasers(true);

            // Find start and end point of each laser
            leftLaserPositions[0] = leftFirePoint.position;
            leftLaserPositions[1] = GetHitPoint(leftFirePoint.position, GetAimPoint());
            rightLaserPositions[0] = rightFirePoint.position;
            rightLaserPositions[1] = GetHitPoint(rightFirePoint.position, GetAimPoint());

            canFire = false;
        }
    }

    void UpdateLaserPositions() {
        leftLaser.SetPositions(leftLaserPositions);
        rightLaser.SetPositions(rightLaserPositions);
    }

    void ToggleLasers(bool toggle) {
        leftLaser.useWorldSpace = toggle;
        rightLaser.useWorldSpace = toggle;

        if (!toggle) {
            laserAlpha = 0;
            leftLaserPositions = new Vector3[2];
            rightLaserPositions = new Vector3[2];
            UpdateLaserPositions();
        } else {
            laserAlpha = 1f;
        }

        leftLaser.enabled = toggle;
        rightLaser.enabled = toggle;
    }

    void UpdateLaserOpacity() {
        if (laserAlpha <= 0) {
            ToggleLasers(false);

            canFire = true;
        }

        laserMaterial.color = new Color(laserMaterial.color.r, laserMaterial.color.g, laserMaterial.color.b, laserAlpha);

        leftLaser.material = laserMaterial;
        rightLaser.material = laserMaterial;

        laserAlpha -= Time.deltaTime / fireFrequency;
    }
}

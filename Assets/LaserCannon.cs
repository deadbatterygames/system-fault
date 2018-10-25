using System.Collections;
using UnityEngine;

public class LaserCannon : ShipModule, IWeapon {

    [SerializeField] float damage = 5f;
    [SerializeField] float fireFrequency = 0.25f;
    [SerializeField] float laserForce = 10f;
    [SerializeField] float aimRange = 1000f;

    [SerializeField] LineRenderer leftLaser;
    [SerializeField] LineRenderer rightLaser;
    [SerializeField] Transform leftFirePoint;
    [SerializeField] Transform rightFirePoint;

    Vector3[] leftLaserPositions = new Vector3[2];
    Vector3[] rightLaserPositions = new Vector3[2];

    bool canFire;

    protected override void Awake() {
        base.Awake();
        moduleType = GameTypes.ModuleType.LaserCannon;
        ToggleLasers(false);
    }

    public Vector3 GetAimPoint() {
        RaycastHit? hit = PlayerCamera.instance.GetPhysicalTarget(aimRange);
        if (hit != null) return hit.Value.point;
        else return transform.position + transform.up * aimRange;
    }

    public Vector3 GetHitPoint(Vector3 firePoint, Vector3 aimPoint) {
        RaycastHit hit;
        if (Physics.Linecast(firePoint, aimPoint, out hit)) {
            IDamageable damageable = hit.collider.GetComponent<IDamageable>();
            if (damageable != null) {
                damageable.Damage(damage, (aimPoint - firePoint).normalized * laserForce);
            }

            return hit.point;
        } else return aimPoint;
    }

    void Update() {
        if (!canFire) {
            leftLaserPositions[0] = leftFirePoint.position;
            rightLaserPositions[0] = rightFirePoint.position;
            UpdateLaserPositions();
        }
    }

    public void Fire() {
        if (canFire) {
            // Find start and end point of each laser
            leftLaserPositions[0] = leftFirePoint.position;
            leftLaserPositions[1] = GetHitPoint(leftFirePoint.position, GetAimPoint());
            rightLaserPositions[0] = rightFirePoint.position;
            rightLaserPositions[1] = GetHitPoint(rightFirePoint.position, GetAimPoint());

            // Fire lasers
            StopCoroutine("ShowLasers");
            StartCoroutine("ShowLasers");
        }
    }

    void UpdateLaserPositions() {
        leftLaser.SetPositions(leftLaserPositions);
        rightLaser.SetPositions(rightLaserPositions);
    }

    void ToggleLasers(bool toggle) {
        leftLaser.enabled = toggle;
        rightLaser.enabled = toggle;
        canFire = !toggle;
    }

    IEnumerator ShowLasers() {
        ToggleLasers(true);

        yield return new WaitForSeconds(0.25f);

        ToggleLasers(false);
    }
}

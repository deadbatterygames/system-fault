using UnityEngine;
using System.Collections;

//
// Player.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Controls the player
//

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]

public class Player : MonoBehaviour, IControllable, IGroundable, IDamageable {

    [SerializeField] float maxHealth = 10f;
    float health;

    [Header("Movement")]
    [SerializeField] float jumpForce = 20f;
    [SerializeField] float walkSpeed = 8f;
    [SerializeField] float walkForce = 1f;
    [SerializeField] float airForce = 0.2f;
    [SerializeField] float airTorque = 0.2f;

    [Header("Mouse")]
    [SerializeField] float lookSensitivity = 1f;
    [SerializeField] float minYAngle = -60f;
    [SerializeField] float maxYAngle = 60f;

    [Space]
    [Header("Camera")]
    [SerializeField] Transform cameraRig;
    [SerializeField] float useRange = 5f;

    ModuleSlot fuelSlot;
    WeaponSlot weaponSlot;

    bool grounded;
    bool snapping;
    float yAngle;

    Rigidbody rb;
    Vector3 moveDirection;
    Vector3 lookRotation;

    [HideInInspector] public Ship ship;

    bool canEquip;
    bool alive = true;

    void OnCollisionEnter(Collision collision) {
        if (alive) {
            float collisionVelocity = collision.relativeVelocity.magnitude;
            if (collisionVelocity > PlayerData.instance.fallTolerance) Damage(collisionVelocity, Vector3.zero);
        }
    }

    void Start() {
        health = maxHealth;

        rb = GetComponent<Rigidbody>();

        fuelSlot = GetComponentInChildren<ModuleSlot>();
        if (fuelSlot == null || fuelSlot.slotType != GameTypes.ModuleType.FuelPack) Debug.LogError("Player: No fuel slot set as child");

        weaponSlot = GetComponentInChildren<WeaponSlot>();
        if (weaponSlot == null) Debug.LogError("Player: No weapon slot set as child");

        if (cameraRig == null)  Debug.LogError("Player: No camera rig set in inspector");

        StartCoroutine("WeaponTimer");
    }

    void FixedUpdate() {
        Move();
        if (!snapping) Rotate();
        if (snapping) Look();

        if (ship) UpdateShipRadar(); else ship = FindObjectOfType<Ship>();

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, GameManager.MAX_PLAYER_SPEED + 3f);
    }

    void UpdateShipRadar() {
        // TODO: Make a utility function for this...
        Vector3 playerToShip = (ship.transform.position - transform.position).normalized;
        float rightLeft = Vector3.Dot(transform.right, playerToShip);
        float forwardBack = Vector3.Dot(transform.forward, playerToShip);

        if (rightLeft > 0) {
            if (forwardBack > 0 && rightLeft < 0.1) PlayerHUD.instance.UpdateShipRadar(0);
            else PlayerHUD.instance.UpdateShipRadar(1);
        } else if (rightLeft < 0) {
            if (forwardBack > 0 && rightLeft > -0.1) PlayerHUD.instance.UpdateShipRadar(0);
            else PlayerHUD.instance.UpdateShipRadar(-1);
        }
    }

    public void CheckInput(ControlObject controlObject) {
        // Move
        if (snapping) moveDirection = (controlObject.rightLeft * transform.right + controlObject.forwardBack * transform.forward).normalized;
        else moveDirection = (controlObject.rightLeft * transform.right + controlObject.forwardBack * transform.forward + controlObject.upDown * transform.up).normalized;

        // Look/Rotate
        lookRotation += new Vector3(-controlObject.verticalLook, controlObject.horizontalLook, controlObject.roll);

        // Jump
        if (controlObject.jump && grounded) Jump();

        // Interact
        if (controlObject.interact) {
            RaycastHit? hit = GetComponentInChildren<PlayerCamera>().GetUsableTarget(useRange);
            if (hit != null && PlayerCamera.instance.checkForUsable) hit.Value.collider.GetComponent<IUsable>().Use();
        }

        // Shield Cells
        if (controlObject.chargeShieldCell && fuelSlot.connectedModule) fuelSlot.connectedModule.GetComponent<FuelPack>().ChargeShields();

        // Weapons
        if (controlObject.fire && weaponSlot.currentWeapon != null) {
            weaponSlot.currentWeapon.Fire();
        }
        if (controlObject.aim && weaponSlot.GetCurrentWeaponType() == GameTypes.PlayerWeaponType.MatterManipulator) {
            weaponSlot.GetComponentInChildren<MatterManipulator>().EquipFuelPack(fuelSlot);
        }
        if (controlObject.matterManipilator) {
            if ((weaponSlot.GetCurrentWeaponType() == GameTypes.PlayerWeaponType.MatterManipulator && !GetComponentInChildren<MatterManipulator>().IsHoldingObject())
            || weaponSlot.GetCurrentWeaponType() != GameTypes.PlayerWeaponType.MatterManipulator
            && canEquip)
                weaponSlot.SwitchWeapons();
        }

        // TODO: REMOVE
        if (Input.GetKeyDown(KeyCode.L) && fuelSlot.connectedModule) {
            GetComponentInChildren<FuelPack>().AddFuel(10000000000f);
        }
    }

    public void ResetPlayer(Transform location) {
        moveDirection = Vector3.zero;
        lookRotation = Vector3.zero;
        yAngle = 0f;

        transform.position = location.position;
        transform.rotation = location.rotation;
        cameraRig.localRotation = Quaternion.identity;

        rb.velocity = Vector3.zero;
    }

    void Move() {
        if (grounded) rb.AddForce(Vector3.ClampMagnitude(moveDirection * walkSpeed - rb.velocity, walkForce), ForceMode.VelocityChange);
        else rb.AddForce(moveDirection * airForce, ForceMode.Acceleration);
    }

    void Look() {
        // Horizontal
        Quaternion deltaRot = Quaternion.AngleAxis(lookRotation.y * lookSensitivity, Vector3.up);
        rb.MoveRotation(rb.rotation * deltaRot);

        // Vertical
        yAngle = Mathf.Clamp(yAngle + -lookRotation.x * lookSensitivity, minYAngle, maxYAngle);
        cameraRig.transform.localEulerAngles = new Vector3(-yAngle, 0f, 0f);

        lookRotation = Vector3.zero;
    }

    // TODO: This should probably call a function in fuel pack
    void Rotate() {
        rb.AddRelativeTorque(lookRotation * airTorque, ForceMode.Acceleration);

        lookRotation = Vector3.zero;
    }

    void Jump() {
        rb.AddRelativeForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
    }

    public void SetGrounded(bool grounded) {
        this.grounded = grounded;
    }

    public void SetSnapping(bool snap) {
        if (snap) rb.freezeRotation = true;
        else rb.freezeRotation = false;
        snapping = snap;
    }

    public void SetCam(PlayerCamera controlCam) {
        controlCam.transform.SetParent(cameraRig);
        controlCam.checkForUsable = true;
    }

    public void Damage(float amount, Vector3 damageForce) {
        PlayerHUD.instance.ShowDamageSplash();

        // Check if player has shields
        if (fuelSlot.connectedModule != null) {
            amount = fuelSlot.connectedModule.GetComponent<FuelPack>().AbsorbDamage(amount);
        }

        health -= amount;

        rb.AddForce(damageForce, ForceMode.VelocityChange);

        if (health <= 0) KillPlayer();
    }

    void KillPlayer() {
        PlayerControl.instance.RemoveControl();
        alive = false;

        SetSnapping(false);
        Destroy(GetComponent<CharacterSnap>());

        StopCoroutine("WeaponTimer");
        weaponSlot.UnequipAll();
        canEquip = false;

        Time.timeScale = 0.25f;
        Debug.LogWarning("Player: DEAD!");
        GameManager.instance.StartCoroutine("ReloadScene");
    }

    IEnumerator WeaponTimer() {
        yield return new WaitForSeconds(0.8f);

        weaponSlot.SwitchWeapons();
        canEquip = true;
    }
}

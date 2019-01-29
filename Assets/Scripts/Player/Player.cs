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

    [Header("Health")]
    [SerializeField] float maxHealth = 25f;
    float health;

    [Header("Movement")]
    [SerializeField] float jumpForce = 20f;
    [SerializeField] float walkSpeed = 15f;
    [SerializeField] float walkForce = 1.2f;
    [SerializeField] float airForce = 8f;
    [SerializeField] float airTorque = 5f;

    [Header("Mouse")]
    [SerializeField] float minYAngle = -60f;
    [SerializeField] float maxYAngle = 60f;

    [Space]
    [Header("Camera")]
    [SerializeField] Transform cameraRig;
    [SerializeField] float useRange = 5f;

    ModuleSlot energySlot;
    WeaponSlot weaponSlot;

    bool grounded;
    bool snapping;
    float yAngle;

    Rigidbody rb;
    Vector3 moveDirection;
    Vector3 lookRotation;
    Vector3 forceRotation;

    [HideInInspector] public bool canEquip;

    void Start() {
        PlayerData.instance.alive = true;
        PlayerHUD.instance.ToggleShipRadar(true);
        PlayerHUD.instance.TogglePlayerHelp(true);

        health = maxHealth;

        rb = GetComponent<Rigidbody>();

        energySlot = GetComponentInChildren<ModuleSlot>();
        if (energySlot == null || energySlot.slotType != GameTypes.ModuleType.EnergyPack) Debug.LogError("Player: No energy slot set as child");

        weaponSlot = GetComponentInChildren<WeaponSlot>();
        if (weaponSlot == null) Debug.LogError("Player: No weapon slot set as child");

        if (cameraRig == null)  Debug.LogError("Player: No camera rig set in inspector");

        StartCoroutine("WeaponTimer");

        //Overmind.AddAttractor(new Attractor(gameObject, 9999999999999f, Attractor.Type.None));
    }

    void FixedUpdate() {
        Move();
        if (!snapping) Rotate();

        rb.velocity = Vector3.ClampMagnitude(rb.velocity, GameManager.MAX_PLAYER_SPEED + 3f); // Player can exceed max player speed to catch up to ship

        UpdateShipRadar();
    }

    void UpdateShipRadar() {
        // TODO: Make a utility function for this...
        Vector3 playerToShip = (GameManager.instance.ship.transform.position - transform.position).normalized;
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
        if (snapping) Look(new Vector3(-controlObject.verticalLook, controlObject.horizontalLook, controlObject.roll));
        else forceRotation += new Vector3(-controlObject.verticalLook * PlayerData.instance.mouseForceSensitivity, controlObject.horizontalLook * PlayerData.instance.mouseForceSensitivity, controlObject.roll);

        // Jump
        if (controlObject.jump && grounded) Jump();

        // Interact
        if (controlObject.interact) {
            RaycastHit? hit = GetComponentInChildren<PlayerCamera>().GetUsableTarget(useRange);
            if (hit != null && PlayerCamera.instance.checkForUsable) hit.Value.collider.GetComponent<IUsable>().Use();
        }

        // Shield Cells
        if (controlObject.chargeShieldCell) {
            if (energySlot.connectedModule) energySlot.connectedModule.GetComponent<EnergyPack>().ChargeShields();
            else PlayerHUD.instance.SetInfoPrompt("Equip an Energy Pack to charge Shield Cell");
        }

        // Weapons
        if (controlObject.firePrimary && weaponSlot.currentWeapon != null) {
            weaponSlot.currentWeapon.Fire();
        }
        if (controlObject.aim && weaponSlot.GetCurrentWeaponType() == GameTypes.PlayerWeaponType.MatterManipulator) {
            weaponSlot.GetComponentInChildren<MatterManipulator>().EquipEnergyPack(energySlot);
        }
        if (controlObject.matterManipilator) {
            if ((weaponSlot.GetCurrentWeaponType() == GameTypes.PlayerWeaponType.MatterManipulator && !GetComponentInChildren<MatterManipulator>().IsHoldingObject())
            || weaponSlot.GetCurrentWeaponType() != GameTypes.PlayerWeaponType.MatterManipulator
            && canEquip)
                weaponSlot.SwitchWeapons();
        }

        if (weaponSlot.GetCurrentWeaponType() == GameTypes.PlayerWeaponType.Multicannon) {
            Multicannon mc = weaponSlot.GetComponentInChildren<Multicannon>();
            if (controlObject.weapon0) mc.ChangeType(GameTypes.DamageType.Yellow);
            if (controlObject.weapon1) mc.ChangeType(GameTypes.DamageType.Blue);
            if (controlObject.weapon2) mc.ChangeType(GameTypes.DamageType.Red);

        }

        // Test stuff
        if (GameManager.instance.IsInTestMode()) {
            if (Input.GetKeyDown(KeyCode.L) && energySlot.connectedModule) GetComponentInChildren<EnergyPack>().AddEnergy(1000000f);
            if (Input.GetKeyDown(KeyCode.K)) Damage(1000000f, GameTypes.DamageType.Physical, Vector3.zero);
        }

        if (Input.GetKeyDown(KeyCode.ScrollLock)) {
            GameManager.instance.SetTestMode(!GameManager.instance.IsInTestMode());
            KillPlayer();
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

    void Look(Vector3 rotation) {
        // Horizontal
        Quaternion deltaRot = Quaternion.AngleAxis(rotation.y * PlayerData.instance.lookSensitivity, Vector3.up);
        transform.localRotation = transform.localRotation * deltaRot;

        // Vertical
        yAngle = Mathf.Clamp(yAngle + -rotation.x * PlayerData.instance.lookSensitivity, minYAngle, maxYAngle);
        cameraRig.transform.localEulerAngles = new Vector3(-yAngle, 0f, 0f);
    }

    void Rotate() {
        rb.AddRelativeTorque(Vector3.ClampMagnitude(forceRotation, 1f) * airTorque, ForceMode.Acceleration);
        forceRotation = Vector3.zero;
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

    public void Damage(float amount, GameTypes.DamageType damageType, Vector3 damageForce) {
        PlayerHUD.instance.ShowDamageSplash();

        if (energySlot.connectedModule != null) amount = energySlot.connectedModule.GetComponent<EnergyPack>().AbsorbDamage(amount);
        health -= amount;
        rb.AddForce(damageForce, ForceMode.VelocityChange);

        if (health <= 0) KillPlayer();
    }

    void KillPlayer() {
        PlayerControl.instance.RemoveControl();
        PlayerData.instance.alive = false;

        SetSnapping(false);
        Destroy(GetComponent<CharacterSnap>());

        StopCoroutine("WeaponTimer");
        weaponSlot.UnequipAll();
        canEquip = false;
        
        GameManager.instance.StartCoroutine("PlayerDeath");
    }

    IEnumerator WeaponTimer() {
        yield return new WaitForSeconds(0.75f);

        weaponSlot.SwitchWeapons(false);
        canEquip = true;
    }
}

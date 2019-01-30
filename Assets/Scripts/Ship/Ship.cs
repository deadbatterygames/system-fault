using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//
// Ship.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Controls the player's ship
//

[RequireComponent(typeof(Rigidbody))]

public class Ship : MonoBehaviour, IControllable, IUsable, IPowerable, IDamageable {

    ModuleSlot[] moduleSlots;
    EnergyPack energyPack = null;
    Thrusters thrusters = null;
    Boosters boosters = null;
    QuantumDrive quantumDrive = null;
    LaserCannon laserCannon;
    List<MissileRack> missileRacks = new List<MissileRack>();
    
    IWeapon kineticWeapon;

    LandingGear landingGear;

    [Header("Flight")]
    [SerializeField] float yawMultiplier = 0.5f;
    [SerializeField] float astroThrottleSensitivity = 0.5f;
    [SerializeField] Transform centerOfMass;

    [Header("Damage")]
    [SerializeField] float shipStrength = 5f;
    [SerializeField] float maxHull = 50f;
    float hull;
    bool dead;

    [Header("Camera")]
    [SerializeField] Transform playerExit;
    [SerializeField] Transform playerExitAlt;
    [SerializeField] Transform cameraRig;
    [SerializeField] float cameraResetSpeed = 1f;

    ShipComputer shipComputer;
    ShipLight shipLight;

    bool canopyClear = true;
    bool powered = false;
    bool busy = false;
    bool lightOn = true;
    int missileRackIndex = 0;

    bool freeLook = false;

    bool limitHoverSpeed = false;

    GameTypes.AssistMode assistMode = GameTypes.AssistMode.NoAssist;
    GameTypes.AssistMode previousAssistMode = GameTypes.AssistMode.NoAssist;

    Rigidbody rb;

    void Start() {
        moduleSlots = GetComponentsInChildren<ModuleSlot>();
        foreach (ModuleSlot slot in moduleSlots) ToggleModuleSlots(slot.slotType, false);

        rb = GetComponent<Rigidbody>();
        rb.centerOfMass = centerOfMass.localPosition;

        landingGear = GetComponentInChildren<LandingGear>();
        if (!landingGear) Debug.LogError("Ship: No landing gear set as child");
        shipComputer = GetComponentInChildren<ShipComputer>();
        if (!landingGear) Debug.LogError("Ship: No ship computer set as child");
        shipLight = GetComponentInChildren<ShipLight>();
        if (!landingGear) Debug.LogError("Ship: No ship light set as child");

        if (cameraRig == null) Debug.LogError("Ship: No camera rig set in inspector");
        if (playerExit == null) Debug.LogError("Ship: No player exit set in inspector");

        hull = maxHull;

        shipComputer.TogglePower(false);
    }

    void OnCollisionEnter(Collision collision) {
        float collisionMagnitude = collision.relativeVelocity.magnitude;
        if (PlayerData.instance.alive && PlayerControl.instance.GetControllingActor() == GetComponent<IControllable>()) {
            if (collisionMagnitude > PlayerData.instance.shipDamageTolerance) {
                IDamageable otherDamageable = collision.collider.GetComponentInParent<IDamageable>();
                if (otherDamageable != null) otherDamageable.Damage(collision.relativeVelocity.magnitude, GameTypes.DamageType.Physical, -collision.relativeVelocity);
            }

            if (collisionMagnitude > PlayerData.instance.shipDamageTolerance && !collision.gameObject.GetComponent<Rigidbody>())
                Damage(collisionMagnitude / shipStrength, GameTypes.DamageType.Physical, Vector3.zero);
        }
    }

    public EnergyPack GetEnergyPack() {
        return energyPack;
    }

    public void CheckInput(ControlObject controlObject) {
        // Toggle Power
        if (controlObject.jump && assistMode != GameTypes.AssistMode.Quantum) {
            if (powered) TogglePower(false);
            else if (!powered && !busy && EnergyAvailable()) TogglePower(true);
        }

        if (powered) {
            if (EnergyAvailable()) {
                // Info Prompts
                if (controlObject.forwardBack != 0 && !thrusters) PlayerHUD.instance.SetInfoPrompt("No Thrusters connected");
                if ((controlObject.horizontalLook != 0 || controlObject.verticalLook != 0 || controlObject.roll != 0 || controlObject.upDown != 0) && !boosters) PlayerHUD.instance.SetInfoPrompt("No Boosters connected");
                if (controlObject.firePrimary && !laserCannon) PlayerHUD.instance.SetInfoPrompt("No Laser Cannon connected");
                if (controlObject.fireSecondary && missileRacks.Count == 0) PlayerHUD.instance.SetInfoPrompt("No Missile Racks connected");
                if (controlObject.quantumJump && !quantumDrive) PlayerHUD.instance.SetInfoPrompt("No Quantum Drive connected");

                // Thruster control
                if (thrusters) {
                    switch (assistMode) {
                        case GameTypes.AssistMode.NoAssist:
                            thrusters.SetThrottle(controlObject.forwardBack);
                            shipComputer.UpdateThrottleGauge(thrusters.GetThrottle());
                            energyPack.DrainEnergy(Mathf.Abs(thrusters.GetThrottle()) / thrusters.efficiency * Time.deltaTime);
                            break;
                        case GameTypes.AssistMode.Hover:
                            thrusters.SetThrottle(controlObject.forwardBack / 4f);
                            shipComputer.UpdateThrottleGauge(thrusters.GetThrottle());
                            energyPack.DrainEnergy(Mathf.Abs(thrusters.GetThrottle() / thrusters.efficiency * Time.deltaTime));
                            break;
                        case GameTypes.AssistMode.Astro:
                            thrusters.AdjustAstroThrottle(controlObject.forwardBack * astroThrottleSensitivity * Time.deltaTime);
                            shipComputer.UpdateThrottleGauge(thrusters.GetAstroThrottle());
                            energyPack.DrainEnergy(thrusters.GetAstroEnergy() * Time.deltaTime);
                            break;
                    }
                }

                // Booster control
                if (boosters) {
                    Vector3 torque;
                    if (!freeLook) torque = new Vector3(-controlObject.verticalLook * PlayerData.instance.mouseForceSensitivity, controlObject.horizontalLook * PlayerData.instance.mouseForceSensitivity * yawMultiplier, controlObject.roll);
                    else torque = new Vector3(0f, 0f, controlObject.roll);

                    switch (assistMode) {
                        case GameTypes.AssistMode.NoAssist:
                        case GameTypes.AssistMode.Astro:
                            boosters.SetThrottle(controlObject.rightLeft, controlObject.upDown, torque);
                            energyPack.DrainEnergy((Mathf.Abs(controlObject.rightLeft) + Mathf.Abs(controlObject.upDown)) / boosters.efficiency * Time.deltaTime);
                            energyPack.DrainEnergy(1f / boosters.efficiency * Time.deltaTime); // Idle burn rate
                            break;
                        case GameTypes.AssistMode.Hover:
                            boosters.SetThrottle(controlObject.rightLeft / 2f, controlObject.upDown / 2f, torque);
                            energyPack.DrainEnergy((Mathf.Abs(controlObject.rightLeft) / 2f + Mathf.Abs(controlObject.upDown)) / 2f / boosters.efficiency * Time.deltaTime);
                            energyPack.DrainEnergy(1f / boosters.efficiency * Time.deltaTime); // Idle burn rate
                            break;
                        default: break;
                    }
                }

                // Quantum Drive
                if (controlObject.quantumJump) {
                    if (thrusters && boosters && quantumDrive) quantumDrive.PickTarget();
                    else PlayerHUD.instance.SetInfoPrompt("Connect Thrusters, Boosters and Quantum Drive to jump");
                }

                if (assistMode == GameTypes.AssistMode.Quantum && quantumDrive.IsJumping()) {
                    energyPack.DrainEnergy(1f / quantumDrive.efficiency * Time.deltaTime); // Quantum burn rate
                    shipComputer.UpdateEnergyGauge(energyPack.GetEnergyPercentage());
                }


            // Assist Modes
            if (controlObject.changeAssist) {
                    switch (assistMode) {
                        case GameTypes.AssistMode.NoAssist:
                            if (previousAssistMode == GameTypes.AssistMode.NoAssist)
                                PlayerHUD.instance.SetInfoPrompt("Connect Boosters to use Hover Assist");
                            ChangeAssistMode(previousAssistMode);
                            break;
                        case GameTypes.AssistMode.Hover:
                            if (thrusters && boosters) ChangeAssistMode(GameTypes.AssistMode.Astro);
                            else PlayerHUD.instance.SetInfoPrompt("Connect Thrusters and Boosters to use Astro Assist");
                            break;
                        case GameTypes.AssistMode.Astro:
                            ChangeAssistMode(GameTypes.AssistMode.Hover);
                            break;
                    }
                }
                if (controlObject.toggleAssist && assistMode != GameTypes.AssistMode.Quantum) {
                    if (assistMode != GameTypes.AssistMode.NoAssist) {
                        ChangeAssistMode(GameTypes.AssistMode.NoAssist);
                        PlayerHUD.instance.SetInfoPrompt("Flight Assist off");
                    } else ChangeAssistMode(previousAssistMode);
                }

                // Weapons
                if (laserCannon != null && controlObject.firePrimary) laserCannon.Fire();
                if (missileRacks.Count > 0 && controlObject.fireSecondary) {
                    missileRacks[missileRackIndex].Fire();
                    if (missileRacks.Count > 1) {
                        if (missileRackIndex == 0) missileRackIndex = 1;
                        else if (missileRackIndex == 1) missileRackIndex = 0;
                    }
                }

                // Speedometer
                shipComputer.UpdateSpeedometer(rb.velocity.magnitude, transform.InverseTransformDirection(rb.velocity).z < -0.5f);

                // Light
                if (controlObject.light) {
                    if (lightOn) {
                        shipLight.TogglePower(false);
                        lightOn = false;
                    } else {
                        shipLight.TogglePower(true);
                        lightOn = true;
                    }
                }
            } else TogglePower(false);
        } else if (controlObject.interact && !busy) StartCoroutine("ExitShip");

        // Shield Cells
        if (controlObject.chargeShieldCell) {
            if (energyPack) energyPack.ChargeShields();
            else PlayerHUD.instance.SetInfoPrompt("Connect an Energy Pack to charge Shield Cell");
        }

        // Camera
        if (controlObject.aim) {
            if (freeLook) freeLook = false;
            else freeLook = true;
        }
        if (controlObject.changeCamera && !busy) PlayerCamera.instance.TogglePerspective();

        if (freeLook) FreeLook(controlObject.horizontalLook, controlObject.verticalLook);
        else ResetCameraRig();
    }

    public void UpdateQuantumCountDown(int time) {
        shipComputer.UpdateCountDown(time);
    }

    void Hover() {
        rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, boosters.hoverDampaning * Time.fixedDeltaTime);
        if (limitHoverSpeed) rb.velocity = Vector3.ClampMagnitude(rb.velocity, boosters.maxHoverSpeed);

        // Start to limit hover speed once ship is slowed down enough (prevents instant slowdowns from other flight modes)
        if (rb.velocity.magnitude < boosters.maxHoverSpeed) limitHoverSpeed = true;
    }

    void Astro() {
        rb.velocity = Vector3.Lerp(rb.velocity, transform.forward * thrusters.GetAstroSpeed(), thrusters.astroAcceleration * Time.fixedDeltaTime);
    }

    void FreeLook(float horizontal, float vertical) {
        cameraRig.Rotate(cameraRig.parent.up, horizontal * PlayerData.instance.lookSensitivity, Space.World);
        cameraRig.Rotate(cameraRig.right, -vertical * PlayerData.instance.lookSensitivity, Space.World);
    }

    void ResetCameraRig() {
        cameraRig.localRotation = Quaternion.Slerp(cameraRig.localRotation, Quaternion.identity, cameraResetSpeed * Time.deltaTime);
    }

    void FixedUpdate() {
        switch (assistMode) {
            case GameTypes.AssistMode.Hover:
                Hover();
                break;
            case GameTypes.AssistMode.Astro:
                Astro();
                break;
            case GameTypes.AssistMode.Quantum:
                break;
        }

        if (assistMode != GameTypes.AssistMode.Quantum) rb.velocity = Vector3.ClampMagnitude(rb.velocity, GameManager.MAX_PLAYER_SPEED);
    }

    void ChangeAssistMode(GameTypes.AssistMode mode) {
        switch (mode) {
            case GameTypes.AssistMode.NoAssist:
                rb.useGravity = true;
                if (thrusters) thrusters.SetAstroThrottle(0f);
                if (powered) landingGear.Retract();
                break;
            case GameTypes.AssistMode.Hover:
                rb.useGravity = false;
                limitHoverSpeed = false;
                if (thrusters) thrusters.SetAstroThrottle(0f);
                landingGear.Extend();
                PlayerHUD.instance.SetInfoPrompt("Hover Assist active");
                break;
            case GameTypes.AssistMode.Astro:
                rb.useGravity = false;
                thrusters.SetThrottle(0f);
                thrusters.SetAstroThrottle(transform.InverseTransformDirection(rb.velocity).z / thrusters.maxAstroSpeed);
                landingGear.Retract();
                PlayerHUD.instance.SetInfoPrompt("Astro Assist active");
                break;
            case GameTypes.AssistMode.Quantum:
                rb.useGravity = false;
                thrusters.SetThrottle(0f);
                thrusters.SetAstroThrottle(0f);
                boosters.SetThrottle(0f, 0f, Vector3.zero);
                landingGear.Retract();
                break;
        }

        if (thrusters && mode != GameTypes.AssistMode.Astro) thrusters.SetThrottle(Input.GetAxis("Move Forward/Back"));

        shipComputer.ChangeAssistMode(mode);
        if (assistMode != GameTypes.AssistMode.NoAssist) previousAssistMode = assistMode;
        assistMode = mode;
    }

    public void ToggleQuantum(bool toggle) {
        if (toggle) ChangeAssistMode(GameTypes.AssistMode.Quantum);
        else if (powered) {
            if (thrusters && boosters) {
                ChangeAssistMode(GameTypes.AssistMode.Astro);
                thrusters.SetAstroThrottle(0f);
            } else if (boosters) ChangeAssistMode(GameTypes.AssistMode.Hover);
            else ChangeAssistMode(GameTypes.AssistMode.NoAssist);
        }
    }

    public void UpdateModuleStatus(ShipModule module, bool connected) {
        switch (module.moduleType) {
            case GameTypes.ModuleType.EnergyPack:
                if (connected) energyPack = module.GetComponent<EnergyPack>();
                else energyPack = null;
                break;
            case GameTypes.ModuleType.Thrusters:
                if (connected) thrusters = module.GetComponent<Thrusters>();
                else thrusters = null;
                break;
            case GameTypes.ModuleType.Boosters:
                if (connected) boosters = module.GetComponent<Boosters>();
                else boosters = null;
                break;
            case GameTypes.ModuleType.QuantumDrive:
                if (connected) quantumDrive = module.GetComponent<QuantumDrive>();
                else quantumDrive = null;
                break;
            case GameTypes.ModuleType.LaserCannon:
                if (connected) laserCannon = module.GetComponent<LaserCannon>();
                else laserCannon = null;
                break;
            case GameTypes.ModuleType.MissileRack:
                if (connected) missileRacks.Add(module.GetComponent<MissileRack>());
                else {
                    missileRacks.Remove(module.GetComponent<MissileRack>());
                    missileRackIndex = 0;
                }
                break;
        }

        shipComputer.UpdateModuleStatus(module.moduleType, connected);
        if (module.moduleType == GameTypes.ModuleType.MissileRack && missileRacks.Count > 0) shipComputer.UpdateModuleStatus(module.moduleType, true);
    }

    public void DetachModule(ModuleSlot slot) {
        Rigidbody rb = slot.connectedModule.gameObject.AddComponent<Rigidbody>();
        rb.mass = slot.connectedModule.mass;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        GameManager.instance.AddGravityBody(rb);
        GameManager.instance.ship.UpdateModuleStatus(slot.connectedModule, false);

        slot.connectedModule.connected = false;
        slot.connectedModule = null;
    }

    public void Use() {
        if (!FindObjectOfType<Player>().GetComponentInChildren<ModuleSlot>().connectedModule) {
            PlayerControl.instance.TakeControl(this);
            PlayerHUD.instance.UpdateShipRadar(0);

            if (!busy) StartCoroutine("EnterShip");

            GameManager.instance.DespawnPlayer();
            FlockingController.CreateAttractor(new Attractor(gameObject, 100000f, Attractor.Type.PlayerShip));
        } else PlayerHUD.instance.SetInfoPrompt("Unequip Energy Pack before entering");
    }

    public void Damage(float amount, GameTypes.DamageType damageType, Vector3 damageForce) {
        PlayerHUD.instance.ShowDamageSplash();

        if (energyPack) amount = energyPack.AbsorbDamage(amount);
        hull -= amount;
        rb.AddForce(damageForce, ForceMode.Impulse);

        if (hull <= 0 && !dead) KillPlayer();
    }

    void KillPlayer() {
        TogglePower(false);

        if (PlayerCamera.instance.IsThirdPerson()) PlayerCamera.instance.transform.parent = null;
        PlayerControl.instance.RemoveControl();

        PlayerData.instance.alive = false;

        GameManager.instance.StartCoroutine("PlayerDeath");

        dead = true;
    }

    public void TogglePower(bool toggle) {
        if (toggle) {
            powered = true;

            // Assist & Modules
            if (boosters) {
                boosters.TogglePower(true);
                ChangeAssistMode(GameTypes.AssistMode.Hover);
            }
            if (thrusters) thrusters.TogglePower(true);
            if (quantumDrive) quantumDrive.TogglePower(true);

            rb.angularDrag = 2f;

            // Computer
            shipComputer.UpdateThrottleGauge(0f);
            shipComputer.UpdateEnergyGauge(0f);
            shipComputer.TogglePower(true);

            // Light
            if (lightOn) shipLight.TogglePower(true);

            Debug.Log("Ship: Powered on");
        } else {
            powered = false;

            // Assist & Modules
            ChangeAssistMode(GameTypes.AssistMode.NoAssist);
            previousAssistMode = GameTypes.AssistMode.NoAssist;

            rb.angularDrag = 0f;
            if (boosters) boosters.TogglePower(false);
            if (thrusters) thrusters.TogglePower(false);
            if (quantumDrive) quantumDrive.TogglePower(false);

            // Computer
            shipComputer.UpdateThrottleGauge(0f);
            shipComputer.UpdateEnergyGauge(0f);
            shipComputer.TogglePower(false);

            // Light
            shipLight.TogglePower(false);

            Debug.Log("Ship: Powered off");
        }
    }

    bool EnergyAvailable() {
        if (energyPack == null) {
            PlayerHUD.instance.SetInfoPrompt("No Energy Pack connected");
            return false;
        }

        if (energyPack.IsEmpty()) PlayerHUD.instance.SetInfoPrompt("Energy Pack empty");
        return !energyPack.IsEmpty();
    }

    public bool IsPowered() {
        return powered;
    }

    public void SetCam(PlayerCamera controlCam) {
        controlCam.transform.SetParent(cameraRig);

        PlayerHUD.instance.ToggleUsePrompt(false);
        PlayerHUD.instance.ToggleDematPrompt(false);
        controlCam.checkForUsable = false;
        controlCam.checkForMaterializable = false;
    }

    public void ToggleModuleSlots(GameTypes.ModuleType moduleType, bool toggle) {
        foreach (ModuleSlot slot in moduleSlots) if (!slot.connectedModule && moduleType == slot.slotType) slot.ToggleSlot(toggle);
    }

    public void SetCanopyClear(bool exitClear) {
        canopyClear = exitClear;
    }

    IEnumerator EnterShip() {
        busy = true;

        yield return new WaitForSeconds(0.75f);

        Canopy canopy = GetComponentInChildren<Canopy>();
        if (canopy.IsOpen()) canopy.Use();

        SetCanopyClear(true);

        if (energyPack) PlayerHUD.instance.EnableEnergyPackHUD(energyPack);
        PlayerHUD.instance.TogglePlayerHelp(false);
        PlayerHUD.instance.ToggleShipHelp(true);
        PlayerHUD.instance.ToggleCrosshair(true);

        busy = false;

    }

    IEnumerator ExitShip() {
        busy = true;

        Canopy canopy = GetComponentInChildren<Canopy>();
        if (!canopy.IsOpen()) canopy.Use();

        if (energyPack) {
            PlayerHUD.instance.DisableEnergyPackHUD();
            PlayerHUD.instance.SetInfoPrompt("Don't forget your Energy Pack!");
        }

        if (PlayerCamera.instance.IsThirdPerson()) PlayerCamera.instance.SetFirstPerson();
        freeLook = false;

        yield return new WaitForSeconds(0.75f);

        PlayerHUD.instance.ToggleShipHelp(false);

        if (canopyClear) GameManager.instance.SpawnPlayer(playerExit, rb.velocity);
        else GameManager.instance.SpawnPlayer(playerExitAlt, rb.velocity);

        busy = false;
    }

    public float GetSpeed(){
        return rb.velocity.magnitude;
    }

    public GameTypes.AssistMode GetAssistMode() {
        return assistMode;
    }

    public string GetName() {
        Debug.LogError("Ship: Use ship enterance instead");
        return null;
    }
}

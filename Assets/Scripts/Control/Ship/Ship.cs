using UnityEngine;
using System.Collections;

//
// Ship.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Controls the player's ship
//

[RequireComponent(typeof(Rigidbody))]

public class Ship : MonoBehaviour, IControllable, IUsable, IPowerable {

    ModuleSlot[] moduleSlots;
    FuelPack fuelPack = null;
    Thrusters thrusters = null;
    Boosters boosters = null;
    QuantumDrive quantumDrive = null;

    LandingGear landingGear;

    [Header("Flight")]
    [SerializeField] float yawMultiplier = 0.5f;
    [SerializeField] float astroThrottleSensitivity = 0.5f;

    [Header("Camera")]
    [SerializeField] Transform playerExit;
    [SerializeField] Transform cameraRig;
    [SerializeField] float cameraSensitivity = 1f;
    [SerializeField] float cameraResetSpeed = 1f;

    ShipComputer shipComputer;
    ShipLight shipLight;

    ParticleSystem spaceParticles;

    bool powered = false;
    bool busy = false;
    bool lightOn = false;

    bool freeLook = false;

    bool limitHoverSpeed = false;

    GameTypes.AssistMode assistMode = GameTypes.AssistMode.NoAssist;
    GameTypes.AssistMode previousAssistMode = GameTypes.AssistMode.NoAssist;

    Rigidbody rb;

    void Start() {
        moduleSlots = GetComponentsInChildren<ModuleSlot>();
        ToggleModuleSlots(false);

        rb = GetComponent<Rigidbody>();

        landingGear = GetComponentInChildren<LandingGear>();
        if (!landingGear) Debug.LogError("Ship: No landing gear set as child");
        shipComputer = GetComponentInChildren<ShipComputer>();
        if (!landingGear) Debug.LogError("Ship: No ship computer set as child");
        shipLight = GetComponentInChildren<ShipLight>();
        if (!landingGear) Debug.LogError("Ship: No ship light set as child");
        spaceParticles = GetComponentInChildren<ParticleSystem>();
        if (!spaceParticles) Debug.LogError("Ship: No space particles set as child");

        if (cameraRig == null) Debug.LogError("Ship: No camera rig set in inspector");
        if (playerExit == null) Debug.LogError("Ship: No player exit set in inspector");

        shipComputer.TogglePower(false);
    }

    public void CheckInput(ControlObject controlObject) {
        // Toggle Power
        if (controlObject.jump && assistMode != GameTypes.AssistMode.Quantum) {
            if (powered) TogglePower(false);
            else if (!powered && !busy && FuelAvailable()) TogglePower(true);
        }

        if (powered) {
            if (FuelAvailable()) {
                // Thruster control
                if (thrusters) {
                    switch (assistMode) {
                        case GameTypes.AssistMode.NoAssist:
                            thrusters.SetThrottle(controlObject.forwardBack);
                            shipComputer.UpdateThrottleGauge(thrusters.GetThrottle());
                            fuelPack.DrainFuel(Mathf.Abs(thrusters.GetThrottle()) / thrusters.efficiency * Time.deltaTime);
                            break;
                        case GameTypes.AssistMode.Hover:
                            thrusters.SetThrottle(controlObject.forwardBack/2f);
                            shipComputer.UpdateThrottleGauge(thrusters.GetThrottle());
                            fuelPack.DrainFuel(Mathf.Abs(thrusters.GetThrottle() / thrusters.efficiency * Time.deltaTime));
                            break;
                        case GameTypes.AssistMode.Astro:
                            thrusters.AdjustAstroThrottle(controlObject.forwardBack * astroThrottleSensitivity * Time.deltaTime);
                            shipComputer.UpdateThrottleGauge(thrusters.GetAstroThrottle());
                            fuelPack.DrainFuel(thrusters.GetAstroFuel() * Time.deltaTime);
                            break;
                    }
                }

                // Booster control
                if (boosters) {
                    Vector3 torque;
                    if (!freeLook) torque = new Vector3(-controlObject.verticalLook, controlObject.horizontalLook * yawMultiplier, controlObject.roll);
                    else torque = new Vector3(0f, 0f, controlObject.roll);

                    switch (assistMode) {
                        case GameTypes.AssistMode.NoAssist:
                        case GameTypes.AssistMode.Astro:
                            boosters.SetThrottle(controlObject.rightLeft, controlObject.upDown, torque);
                            fuelPack.DrainFuel((Mathf.Abs(controlObject.rightLeft) + Mathf.Abs(controlObject.upDown)) / boosters.efficiency * Time.deltaTime);
                            if (assistMode == GameTypes.AssistMode.Astro) fuelPack.DrainFuel(1f / thrusters.efficiency * Time.deltaTime); // Idle burn rate
                            break;
                        case GameTypes.AssistMode.Hover:
                            boosters.SetThrottle(controlObject.rightLeft / 2f, controlObject.upDown / 2f, torque);
                            fuelPack.DrainFuel((Mathf.Abs(controlObject.rightLeft)/2f + Mathf.Abs(controlObject.upDown))/2f / boosters.efficiency * Time.deltaTime);
                            fuelPack.DrainFuel(1f / boosters.efficiency * Time.deltaTime); // Idle burn rate
                            break;
                    }
                }

                // Quantum Drive
                if (thrusters && boosters && quantumDrive) {
                    if (controlObject.quantumJump) quantumDrive.PickTarget();

                    if (assistMode == GameTypes.AssistMode.Quantum && quantumDrive.IsJumping()) {
                        fuelPack.DrainFuel(1f / quantumDrive.efficiency * Time.deltaTime); // Quantum burn rate
                    }
                }

                shipComputer.UpdateFuelGauge(fuelPack.GetFuelPercentage());

                // Assist Modes
                if (controlObject.changeAssist) {
                    switch (assistMode) {
                        case GameTypes.AssistMode.NoAssist:
                            ChangeAssistMode(previousAssistMode);
                            break;
                        case GameTypes.AssistMode.Hover:
                            if (boosters && thrusters) ChangeAssistMode(GameTypes.AssistMode.Astro);
                            break;
                        case GameTypes.AssistMode.Astro:
                            ChangeAssistMode(GameTypes.AssistMode.Hover);
                            break;
                    }
                }
                if (controlObject.toggleAssist && assistMode != GameTypes.AssistMode.Quantum) {
                    if (assistMode != GameTypes.AssistMode.NoAssist) {
                        previousAssistMode = assistMode;
                        ChangeAssistMode(GameTypes.AssistMode.NoAssist);
                    } else ChangeAssistMode(previousAssistMode);
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
        if (controlObject.chargeShieldCell && fuelPack) fuelPack.ChargeShields();

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
        cameraRig.Rotate(cameraRig.parent.up, horizontal * cameraSensitivity, Space.World);
        cameraRig.Rotate(cameraRig.right, -vertical * cameraSensitivity, Space.World);
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
                Debug.Log("Ship: Assist off");
                break;
            case GameTypes.AssistMode.Hover:
                rb.useGravity = false;
                limitHoverSpeed = false;
                if (thrusters) thrusters.SetAstroThrottle(0f);
                landingGear.Extend();
                Debug.Log("Ship: Hover Mode on");
                break;
            case GameTypes.AssistMode.Astro:
                rb.useGravity = false;
                thrusters.SetThrottle(0f);
                thrusters.SetAstroThrottle(transform.InverseTransformDirection(rb.velocity).z / thrusters.maxAstroSpeed);
                landingGear.Retract();
                Debug.Log("Ship: Astro Flight on");
                break;
            case GameTypes.AssistMode.Quantum:
                rb.useGravity = false;
                thrusters.SetThrottle(0f);
                thrusters.SetAstroThrottle(0f);
                boosters.SetThrottle(0f, 0f, Vector3.zero);
                landingGear.Retract();
                Debug.Log("Ship: Quantum Flight on");
                break;
        }

        if (thrusters && mode != GameTypes.AssistMode.Astro) thrusters.SetThrottle(Input.GetAxis("Move Forward/Back"));

        shipComputer.ChangeAssistMode(mode);
        assistMode = mode;
    }

    public void ToggleQuantum(bool toggle) {
        if (toggle) ChangeAssistMode(GameTypes.AssistMode.Quantum);
        else if (powered) ChangeAssistMode(GameTypes.AssistMode.Hover);
    }

    public void UpdateModuleStatus(ShipModule module, GameTypes.ModuleType type, bool connected) {
        switch (type) {
            case GameTypes.ModuleType.FuelPack:
                if (connected) {
                    fuelPack = module.GetComponent<FuelPack>();
                    Debug.Log("Ship: Fuel pack connected");
                } else {
                    fuelPack = null;
                    Debug.Log("Ship: Fuel pack disconnected");
                }
                break;
            case GameTypes.ModuleType.Thrusters:
                if (connected) {
                    thrusters = module.GetComponent<Thrusters>();
                    Debug.Log("Ship: Thrusters connected");
                } else {
                    thrusters = null;
                    Debug.Log("Ship: Thrusters disconnected");
                }
                break;
            case GameTypes.ModuleType.Boosters:
                if (connected) {
                    boosters = module.GetComponent<Boosters>();
                    Debug.Log("Ship: Boosters connected");
                } else {
                    boosters = null;
                    Debug.Log("Ship: Boosters disconnected");
                }
                break;
            case GameTypes.ModuleType.QuantumDrive:
                if (connected) {
                    quantumDrive = module.GetComponent<QuantumDrive>();
                    Debug.Log("Ship: Quantum Drive connected");
                } else {
                    quantumDrive = null;
                    Debug.Log("Ship: Quantum Drive disconnected");
                }
                break;
        }
    }

    public void Use() {
        if (!FindObjectOfType<Player>().GetComponentInChildren<ModuleSlot>().connectedModule) {
            PlayerControl.instance.TakeControl(this);
            PlayerHUD.instance.UpdateShipRadar(0);

            if (!busy) StartCoroutine("EnterShip");

            GameManager.instance.DespawnPlayer();
        } else PlayerHUD.instance.SetInfoPrompt("Remove Fuel Pack before entering");
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
            shipComputer.UpdateFuelGauge(0f);
            shipComputer.TogglePower(true);

            // Light
            if (lightOn) shipLight.TogglePower(true);

            Debug.Log("Ship: Powered on");
        } else {
            powered = false;

            // Assist & Modules
            ChangeAssistMode(GameTypes.AssistMode.NoAssist);
            rb.angularDrag = 0f;
            if (boosters) boosters.TogglePower(false);
            if (thrusters) thrusters.TogglePower(false);
            if (quantumDrive) quantumDrive.TogglePower(false);

            // Computer
            shipComputer.UpdateThrottleGauge(0f);
            shipComputer.UpdateFuelGauge(0f);
            shipComputer.TogglePower(false);

            // Light
            shipLight.TogglePower(false);

            Debug.Log("Ship: Powered off");
        }
    }

    bool FuelAvailable() {
        if (fuelPack == null) {
            PlayerHUD.instance.SetInfoPrompt("No Fuel Pack connected");
            return false;
        }

        if (fuelPack.IsEmpty()) PlayerHUD.instance.SetInfoPrompt("Fuel Pack empty");
        return !fuelPack.IsEmpty();
    }

    public void SetCam(PlayerCamera controlCam) {
        controlCam.transform.SetParent(cameraRig);

        PlayerHUD.instance.ToggleUsePrompt(false);
        PlayerHUD.instance.ToggleDematPrompt(false);
        controlCam.checkForUsable = false;
        controlCam.checkForMaterializable = false;
    }

    public void ToggleModuleSlots(bool toggle) {
        foreach (ModuleSlot slot in moduleSlots) {
            if (!slot.connectedModule) slot.GetComponent<MeshRenderer>().enabled = toggle;
        }
    }

    IEnumerator EnterShip() {
        busy = true;
        spaceParticles.Play();

        yield return new WaitForSeconds(0.75f);

        Canopy canopy = GetComponentInChildren<Canopy>();
        if (canopy.IsOpen()) canopy.Use();

        if (fuelPack) PlayerHUD.instance.EnableFuelPackHUD(fuelPack);

        busy = false;
    }

    IEnumerator ExitShip() {
        busy = true;

        Canopy canopy = GetComponentInChildren<Canopy>();
        if (!canopy.IsOpen()) canopy.Use();

        if (fuelPack) PlayerHUD.instance.DisableFuelPackHUD();

        if (PlayerCamera.instance.IsThirdPerson()) PlayerCamera.instance.FirstPerson();
        freeLook = false;

        yield return new WaitForSeconds(0.75f);

        GameManager.instance.SpawnPlayer(playerExit, rb.velocity);
        spaceParticles.Stop();
        spaceParticles.Clear();
        busy = false;
    }
}

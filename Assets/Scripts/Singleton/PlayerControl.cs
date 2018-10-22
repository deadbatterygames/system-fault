using UnityEngine;

//
// PlayerControl.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Handles input for all types of controls
//

public struct ControlObject {
    // Movement
    public float forwardBack;
    public float rightLeft;
    public float upDown;
    public float roll;
    public bool jump;
    public bool run;
    public bool changeAssist;
    public bool toggleAssist;
    public bool quantumJump;

    // Aiming
    public float horizontalLook;
    public float verticalLook;
    public bool changeCamera;

    // Player action
    public bool fire;
    public bool aim;
    public bool interact;
    public bool attachShieldCell;
    public bool chargeShieldCell;
    public bool light;

    // Weapon types
    public bool matterManipilator;
    public bool weapon0;
    public bool weapon1;
    public bool weapon2;
    public bool weapon3;

    // UI
    public bool menuRight;
    public bool menuLeft;
    public bool menuUp;
    public bool menuDown;
}

public class PlayerControl : MonoBehaviour {

    public static PlayerControl instance = null;

    IControllable controlActor = null;

    void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    ControlObject currentInput;

    void Start() {
        currentInput = new ControlObject();
        LockMouse();
    }

    void OnApplicationFocus(bool focus) {
        if (focus) LockMouse();
    }

    void LockMouse() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update() {
        if (controlActor != null) {
            currentInput.forwardBack = Input.GetAxis("Move Forward/Back");
            currentInput.rightLeft = Input.GetAxis("Move Right/Left");
            currentInput.upDown = Input.GetAxis("Move Up/Down");
            currentInput.roll = Input.GetAxis("Roll");
            currentInput.jump = Input.GetButtonDown("Jump");
            currentInput.run = Input.GetButton("Run/Change Assist Mode");
            currentInput.changeAssist = Input.GetButtonDown("Run/Change Assist Mode");
            currentInput.toggleAssist = Input.GetButtonDown("Toggle Flight Assist");
            currentInput.quantumJump = Input.GetButtonDown("Quantum Jump");

            currentInput.horizontalLook = Input.GetAxis("Horizontal Look");
            currentInput.verticalLook = Input.GetAxis("Vertical Look");
            currentInput.changeCamera = Input.GetButtonDown("Change Camera");

            currentInput.fire = Input.GetButtonDown("Fire");
            currentInput.aim = Input.GetButtonDown("Aim/Equip Fuel Pack");
            currentInput.interact = Input.GetButtonDown("Interact");
            currentInput.attachShieldCell = Input.GetButtonDown("Attach Shield Cell");
            currentInput.chargeShieldCell = Input.GetButton("Charge Shield Cell");
            currentInput.light = Input.GetButtonDown("Light");

            currentInput.matterManipilator = Input.GetButtonDown("Matter Manipulator");
            currentInput.weapon0 = Input.GetButtonDown("Pulse Cannon/Energy Weapon");
            currentInput.weapon1 = Input.GetButtonDown("Foam Cannon/Kinetic Weapon");
            currentInput.weapon2 = Input.GetButtonDown("Flame Cannon");
            currentInput.weapon3 = Input.GetButtonDown("Ice Cannon");

            currentInput.menuRight = Input.GetButtonDown("Menu Right");
            currentInput.menuLeft = Input.GetButtonDown("Menu Left");
            currentInput.menuUp = Input.GetButtonDown("Menu Up");
            currentInput.menuDown = Input.GetButtonDown("Menu Down");

            controlActor.CheckInput(currentInput);
        }
    }

    public void TakeControl(IControllable actor) {
        controlActor = actor;
        actor.SetCam(PlayerCamera.instance);
    }

    public void RemoveControl() {
        controlActor = null;
    }

    public bool InControl() {
        return controlActor != null;
    }

    public IControllable GetControllingActor() {
        return controlActor;
    }
}

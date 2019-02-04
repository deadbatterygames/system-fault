using UnityEngine;

//
// WeaponSlot.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Switches between weapons and hides weapons in certain scenarios
//

public class WeaponSlot : MonoBehaviour {

    [SerializeField] Transform loweredTransform;
    [SerializeField] float resetSpeed = 3f;

    [HideInInspector] public IWeapon currentWeapon;
    GameTypes.PlayerWeaponType currentWeaponType = GameTypes.PlayerWeaponType.None;

    // Weapons
    public MatterManipulator matterManipulator;
    public Multicannon multicannon;

    // Weapon Positions
    Vector3 defaultPosition;
    Quaternion defaultRotation;
    Vector3 loweredPosition;
    Quaternion loweredRotation;

    bool refresh = false;

    void Awake () {
        defaultPosition = transform.localPosition;
        defaultRotation = transform.localRotation;
        loweredPosition = loweredTransform.localPosition;
        loweredRotation = loweredTransform.localRotation;
    }

    void Update() {
        if (refresh) ResetWeapon();
    }

    public void SwitchWeapons(bool showPrompt = true) {
        if (currentWeaponType != GameTypes.PlayerWeaponType.MatterManipulator && PlayerData.instance.hasMatterManipulator) EquipMatterManipulator(showPrompt);
        else if (currentWeaponType != GameTypes.PlayerWeaponType.Multicannon && PlayerData.instance.hasMulticannon) EquipMulticannon(showPrompt);
    }

    public void EquipMatterManipulator(bool showPrompt) {
        if (currentWeapon != matterManipulator.GetComponent<IWeapon>()) {
            // Objects
            currentWeapon = matterManipulator;
            currentWeaponType = GameTypes.PlayerWeaponType.MatterManipulator;
            multicannon.gameObject.SetActive(false);
            matterManipulator.gameObject.SetActive(true);

            // HUD
            PlayerCamera.instance.checkForMaterializable = true;
            PlayerHUD.instance.ToggleCrosshair(true);
            if (showPrompt) PlayerHUD.instance.SetInfoPrompt("Matter Manipulator equipped");

            RefreshWeapon();
        }
    }
    
    public void EquipMulticannon(bool showPrompt) {
        if (currentWeapon != multicannon.GetComponent<IWeapon>()) {
            // Objects
            currentWeapon = multicannon;
            currentWeaponType = GameTypes.PlayerWeaponType.Multicannon;
            matterManipulator.gameObject.SetActive(false);
            multicannon.gameObject.SetActive(true);

            // HUD
            PlayerCamera.instance.checkForMaterializable = false;
            PlayerHUD.instance.ToggleDematPrompt(false);
            PlayerHUD.instance.ToggleCrosshair(true);
            if (showPrompt) PlayerHUD.instance.SetInfoPrompt("Multicannon equipped");

            RefreshWeapon();
        }
    }

    public void UnequipAll() {
        if (currentWeapon != null) {
            // Objects
            currentWeapon = null;
            currentWeaponType = GameTypes.PlayerWeaponType.None;
            GetComponentInParent<Player>().canEquip = false;
            matterManipulator.gameObject.SetActive(false);
            multicannon.gameObject.SetActive(false);

            // HUD
            PlayerCamera.instance.checkForMaterializable = false;
            PlayerHUD.instance.ToggleCrosshair(false);
        }
    }

    void ResetWeapon() {
        if (transform.localPosition != defaultPosition || transform.localRotation != defaultRotation) {
            transform.localPosition = Vector3.Lerp(transform.localPosition, defaultPosition, resetSpeed * Time.deltaTime);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, defaultRotation, resetSpeed * Time.deltaTime);
        } else refresh = false;
    }

    void RefreshWeapon() {
        transform.localPosition = loweredPosition;
        transform.localRotation = loweredRotation;

        refresh = true;
    }

    public GameTypes.PlayerWeaponType GetCurrentWeaponType() {
        return currentWeaponType;
    }
}

using UnityEngine;

//
// WeaponPickup.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Unlocks weapons for the player
//

public class WeaponPickup : MonoBehaviour, IUsable {

    [SerializeField] GameTypes.PlayerWeaponType weaponType;
    [SerializeField] string pickupName;

    void Start() {
        if (weaponType == GameTypes.PlayerWeaponType.MatterManipulator) pickupName = "Matter Manipulator";
        else if (weaponType == GameTypes.PlayerWeaponType.Multicannon) pickupName = "Multicannon";
        else Debug.LogError("WeaponPickup: No type specified");
    }

    public string GetName() {
        return pickupName;
    }

    public void Use() {
        WeaponSlot weaponSlot = FindObjectOfType<WeaponSlot>();
        switch (weaponType) {
            case GameTypes.PlayerWeaponType.MatterManipulator:
                PlayerData.instance.hasMatterManipulator = true;
                weaponSlot.EquipMatterManipulator(true);
                break;
            case GameTypes.PlayerWeaponType.Multicannon:
                PlayerData.instance.hasMulticannon = true;
                weaponSlot.EquipMulticannon(true);
                break;
        }

        if (PlayerData.instance.hasMatterManipulator && PlayerData.instance.hasMulticannon) PlayerHUD.instance.SetInfoPrompt("Press TAB to change equipment");

        GameManager.instance.RemoveGravityBody(GetComponentInParent<Rigidbody>());
        GameManager.instance.RemoveGravityBody(GetComponentInParent<Rigidbody>());
        Destroy(transform.parent.gameObject);
    }
}

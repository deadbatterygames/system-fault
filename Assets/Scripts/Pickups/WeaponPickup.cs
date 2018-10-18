using UnityEngine;

//
// WeaponPickup.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Unlocks weapons for the player
//

public class WeaponPickup : MonoBehaviour, IUsable {

    [SerializeField] GameTypes.PlayerWeaponType weaponType;

    public void Use() {

        WeaponSlot weaponSlot = FindObjectOfType<WeaponSlot>();
        if (weaponType == GameTypes.PlayerWeaponType.MatterManipulator) {
            PlayerData.instance.hasMatterManipulator = true;
            weaponSlot.EquipMatterManipulator();
        } else if (weaponType == GameTypes.PlayerWeaponType.MultiCannon) {
            PlayerData.instance.hasMultiCannon = true;
            weaponSlot.EquipMultiCannon();
        }

        SceneManager.instance.RemoveGravityBody(GetComponentInParent<Rigidbody>());
        Destroy(transform.parent.gameObject);
    }
}

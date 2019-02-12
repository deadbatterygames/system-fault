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

    static float rotationSpeed = 180f;

    public string GetName() {
        return pickupName;
    }

    public void Use() {
        WeaponSlot weaponSlot = FindObjectOfType<WeaponSlot>();
        switch (weaponType) {
            case GameTypes.PlayerWeaponType.MatterManipulator:
                GameData.instance.hasMatterManipulator = true;
                weaponSlot.EquipMatterManipulator(true);
                break;
            case GameTypes.PlayerWeaponType.Multicannon:
                GameData.instance.hasMulticannon = true;
                weaponSlot.EquipMulticannon(true);
                break;
        }

        if (GameData.instance.hasMatterManipulator && GameData.instance.hasMulticannon) PlayerHUD.instance.SetInfoPrompt("Press TAB to change equipment");

        Destroy(transform.parent.gameObject);
    }

    public void Update() {
        transform.Rotate(transform.parent.up, rotationSpeed * Time.deltaTime, Space.World);
        transform.Translate(0f, 0f, Mathf.Sin(Time.time * 5f) * Time.deltaTime, Space.Self);
    }
}

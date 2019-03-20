using UnityEngine;

//
// MulticannonUpgrade.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Unlocks weapon types for the player's Multicannon
//

public class MulticannonUpgrade : MonoBehaviour, IUsable
{
    public GameTypes.DamageType upgradeType;
    public Transform bulletModel;
    public Material disabledMaterial;
    public float shrinkSpeed = 0.3f;
    public ParticleSystem explosion;
    public Canvas screens;

    MeshRenderer mesh;
    ParticleSystem[] particleSystems;
    bool used;

    void Start() {
        if (upgradeType != GameTypes.DamageType.Red && upgradeType != GameTypes.DamageType.Blue)
            Debug.LogError("MulticannonUpgrade: Upgrade Type must be Red or Blue");

        mesh = GetComponent<MeshRenderer>();

        particleSystems = transform.parent.GetComponentsInChildren<ParticleSystem>();
    }

    void FixedUpdate() {
        bulletModel.Rotate(1f, 2f, 3f);

        if (used) {
            float localScale = bulletModel.localScale.x - Time.fixedDeltaTime * shrinkSpeed;
            bulletModel.localScale = new Vector3(localScale, localScale, localScale);
            if (localScale <= 0f) Disable();
        }
    }

    public string GetName() {
        return "Multicannon Upgrade";
    }

    public void Disable() {
        // Unlock weapon type
        if (upgradeType == GameTypes.DamageType.Blue) GameData.instance.blueUnlocked = true;
        else if (upgradeType == GameTypes.DamageType.Red) GameData.instance.redUnlocked = true;

        // Equip weapon type
        WeaponSlot weaponSlot = FindObjectOfType<WeaponSlot>();
        weaponSlot.EquipMulticannon(false);
        weaponSlot.multicannon.ChangeType(upgradeType);

        PlayerHUD.instance.SetInfoPrompt(upgradeType.ToString() + " Energy unlocked");

        explosion.Play();

        Destroy(bulletModel.gameObject);
        Destroy(screens.gameObject);

        Destroy(this);
    }

    public void Use() {
        if (GameData.instance.hasMulticannon) {
            gameObject.layer = LayerMask.GetMask("Default");
            foreach (ParticleSystem ps in particleSystems) if (ps != explosion) ps.Stop();

            Material[] mats = mesh.materials;
            mats[0] = disabledMaterial;
            mesh.materials = mats;

            used = true;
        } else PlayerHUD.instance.SetInfoPrompt("You do not have a Multicannon");
    }
}

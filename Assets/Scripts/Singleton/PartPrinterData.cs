using UnityEngine;

//
// PartPrinterData.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Holds array of module prefabs
//

public class PartPrinterData : MonoBehaviour {

    public static PartPrinterData instance;

    public GameObject[] modulePrefabs;
    public GameObject printDrivePrefab;
    public Material printMaterial;
    public float printTime = 2f;
    public float[] printCosts;

    public const int MODULE_TYPES = 6;
    public const int MODULE_TIERS = 3;

    bool[,] unlockedModules = new bool[MODULE_TYPES, MODULE_TIERS];

    void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        if (!printMaterial) Debug.LogError("PartPrinterData: No print material set");
        if (printCosts.Length != modulePrefabs.Length) Debug.LogError("PartPrinterData: modulePrefabs and printCosts sizes do not match");

        DontDestroyOnLoad(gameObject);

        if (GameManager.instance.IsInTestMode()) UnlockAllModules();
        else LockAllModules();
    }

    void UnlockAllModules() {
        for (int x = 0; x < MODULE_TYPES; x++) {
            for (int y = 0; y < MODULE_TIERS; y++) {
                unlockedModules[x, y] = true;
            }
        }

        Debug.LogWarning("PartPrinterData: All modules unlocked");
    }

    void LockAllModules() {
        for (int x = 0; x < MODULE_TYPES; x++) {
            for (int y = 0; y < MODULE_TIERS; y++) {
                unlockedModules[x, y] = false;
            }
        }

        UnlockModule(GameTypes.ModuleType.EnergyPack, 1);

        Debug.LogWarning("PartPrinterData: All modules locked");
    }

    public void UnlockModule(GameTypes.ModuleType moduleType, int tier) {
        unlockedModules[(int)moduleType, tier-1] = true;
        string tierString;
        switch (tier) {
            case 1: tierString = "Bronze "; break;
            case 2: tierString = "Silver "; break;
            case 3: tierString = "Gold "; break;
            default: tierString = "Unknown Tier "; break;
        }
        PlayerHUD.instance.SetInfoPrompt(tierString + GameManager.instance.GetModuleTypeString(moduleType) + " unlocked");
    }

    public bool IsUnlocked(GameTypes.ModuleType moduleType, int tier) {
        return unlockedModules[(int)moduleType, tier-1];
    }

    public Vector2Int FirstIndex() {
        for (int x = 0; x < MODULE_TYPES; x++) {
            for (int y = MODULE_TIERS-1; y >= 0; y--) {
                if (unlockedModules[x, y]) {
                    return new Vector2Int(x, y);
                }
            }
        }

        return new Vector2Int(0, 0);
    }

    public Vector2Int NextModuleIndex(Vector2Int index) {
        for (int x = index.x + 1; x < MODULE_TYPES; x++) {
            for (int y = MODULE_TIERS-1; y >= 0; y--) {
                if (unlockedModules[x, y]) {
                    return new Vector2Int(x, y);
                }
            }
        }

        return index;
    }

    public Vector2Int PreviousModuleIndex(Vector2Int index) {
        for (int x = index.x -1 ; x >= 0; x--) {
            for (int y = MODULE_TIERS-1; y >= 0; y--) {
                if (unlockedModules[x, y]) {
                    return new Vector2Int(x, y);
                }
            }
        }

        return index;
    }

    public Vector2Int NextTierIndex(Vector2Int index) {
        for (int y = index.y + 1; y < MODULE_TIERS; y++) {
            if (unlockedModules[index.x, y]) {
                return new Vector2Int(index.x, y);
            }
        }

        return index;
    }

    public Vector2Int PreviousTierIndex(Vector2Int index) {
        for (int y = index.y -1; y >= 0; y--) {
            if (unlockedModules[index.x, y]) {
                return new Vector2Int(index.x, y);
            }
        }

        return index;
    }
}

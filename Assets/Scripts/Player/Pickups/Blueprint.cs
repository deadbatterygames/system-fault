using UnityEngine;

//
// Mainframe.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Holds a blueprint index to unlock in the Print Drive
//

public class Blueprint : MonoBehaviour {

    GameTypes.ModuleType moduleType;
    int moduleTier;
    bool unlocked;

    public void SetBlueprintType(GameTypes.ModuleType type, int tier) {
        moduleType = type;
        moduleTier = tier;
    }

    public void Unlock(PrintDrive drive) {
        if (!unlocked) {
            PartPrinterData.instance.UnlockModule(moduleType, moduleTier);
            drive.ShowBlueprint(moduleType, moduleTier);

            unlocked = true;
        }
    }
}

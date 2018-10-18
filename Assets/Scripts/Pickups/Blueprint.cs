using UnityEngine;

//
// Mainframe.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Holds a blueprint index to unlock in the Print Drive
//

public class Blueprint : MonoBehaviour {

    [SerializeField] GameTypes.ModuleType moduleType;
    [SerializeField] [Range(1, 3)] int moduleTier;

    bool unlocked;

    public void Unlock(PrintDrive drive) {
        if (!unlocked) {
            PartPrinterData.instance.UnlockModule(moduleType, moduleTier);
            drive.ShowBlueprint(moduleType, moduleTier);

            unlocked = true;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

//
// PrintDrive.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Displays Part Printer Blueprint information to the player
//

public class PrintDrive : MonoBehaviour, IControllable {

    [Header("Camera")]
    [SerializeField] Transform cameraRig;

    [Header("Screen")]
    [SerializeField] Canvas screen;
    [SerializeField] Material screenOnMaterial;
    [SerializeField] Material screenOffMaterial;
    [SerializeField] MeshRenderer mesh;

    [Header("Icons")]
    [SerializeField] Image energyPack;
    [SerializeField] Image thrusters;
    [SerializeField] Image boosters;
    [SerializeField] Image quantumDrive;
    [SerializeField] Image laserCannon;

    [SerializeField] Image tier1;
    [SerializeField] Image tier2;
    [SerializeField] Image tier3;

    Vector2Int currentIndex;

    PrintDrivePort printDrivePort;
    bool inserted;

    public void Start() {
        currentIndex = PartPrinterData.instance.FirstIndex();
        ShowBlueprint((GameTypes.ModuleType)currentIndex.x, currentIndex.y + 1);
    }

    public void ShowBlueprint(GameTypes.ModuleType moduleType, int tier) {
        ShowModule(moduleType);
        ShowTier(tier);
    }

    public void AssignPort(PrintDrivePort port) {
        printDrivePort = port;
    }

    public void CheckInput(ControlObject controlObject) {
        if (controlObject.menuRight) ChangeModule(true);
        if (controlObject.menuLeft) ChangeModule(false);
        if (controlObject.menuUp) ChangeTier(true);
        if (controlObject.menuDown) ChangeTier(false);

        if (controlObject.jump) {
            printDrivePort.SendPrintSignal(currentIndex);
        }

        if (controlObject.interact) {
            printDrivePort.RemovePrintDrive();
        }
    }

    public void SetCam(PlayerCamera controlCam) {
        controlCam.transform.SetParent(cameraRig);
    }

    void ShowModule(GameTypes.ModuleType module) {
        energyPack.enabled = false;
        thrusters.enabled = false;
        boosters.enabled = false;
        quantumDrive.enabled = false;
        laserCannon.enabled = false;

        switch (module) {
            case GameTypes.ModuleType.EnergyPack:
                energyPack.enabled = true;
                break;
            case GameTypes.ModuleType.Thrusters:
                thrusters.enabled = true;
                break;
            case GameTypes.ModuleType.Boosters:
                boosters.enabled = true;
                break;
            case GameTypes.ModuleType.QuantumDrive:
                quantumDrive.enabled = true;
                break;
            case GameTypes.ModuleType.LaserCannon:
                laserCannon.enabled = true;
                break;
        }
    }

    public void PowerScreen(bool power) {
        Material[] mats = mesh.GetComponent<MeshRenderer>().materials;

        if (power) {
            screen.enabled = true;
            mats[0] = screenOnMaterial;
        } else {
            screen.enabled = false;
            mats[0] = screenOffMaterial;
        }

        mesh.GetComponent<MeshRenderer>().materials = mats;
    }

    void ShowTier(int tier) {
        switch (tier) {
            case 1:
                tier1.enabled = true;
                tier2.enabled = false;
                tier3.enabled = false;
                break;
            case 2:
                tier1.enabled = true;
                tier2.enabled = true;
                tier3.enabled = false;
                break;
            case 3:
                tier1.enabled = true;
                tier2.enabled = true;
                tier3.enabled = true;
                break;
        }
    }

    void ChangeModule(bool next) {
        if (next) currentIndex = PartPrinterData.instance.NextModuleIndex(currentIndex);
        else currentIndex = PartPrinterData.instance.PreviousModuleIndex(currentIndex);

        ShowBlueprint((GameTypes.ModuleType)currentIndex.x, currentIndex.y + 1);
    }

    void ChangeTier(bool next) {
        if (next) currentIndex = PartPrinterData.instance.NextTierIndex(currentIndex);
        else currentIndex = PartPrinterData.instance.PreviousTierIndex(currentIndex);

        ShowBlueprint((GameTypes.ModuleType)currentIndex.x, currentIndex.y + 1);
    }
}

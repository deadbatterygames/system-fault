using UnityEngine;
using System.Collections;

//
// PartPrinter.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Prints ship modules for the player to use
//

public class PartPrinter : MonoBehaviour {

    [SerializeField] Transform printPoint;

    MeshRenderer meshRenderer;
    ParticleSystem[] printParticles;

    ShipModule newPart = null;
    bool printSurfaceClear = true;
    float partOpacity = 0f;

    private void Start() {
        if (!printPoint) Debug.LogError("PartPrinter: No print point set");
        printParticles = GetComponentsInChildren<ParticleSystem>();
    }

    void Update () {
        // TODO: This should probably be in a shader
        if (newPart) {
            partOpacity += Time.deltaTime / PartPrinterData.instance.printTime;
            SetPartOpacity(partOpacity);
        }
    }

    void SetPartOpacity(float opacity) {
        foreach (Material mat in meshRenderer.materials) {
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, opacity);
        }
    }

    public void ProcessPrintSignal(Vector2Int index) {
        int prefabIndex;

        if (index.x < PartPrinterData.MODULE_TYPES) prefabIndex = index.x * 3 + index.y;
        else prefabIndex = 9;

        Debug.Log("PartPrinter: Print request for index " + index + " / " + prefabIndex);

        float printCost = PartPrinterData.instance.printCosts[prefabIndex];

        if (printSurfaceClear) {
            if (printCost > 0f) {
                Player player = FindObjectOfType<Player>();
                if (player.GetComponentInChildren<ModuleSlot>().connectedModule) {
                    FuelPack fuelPack = player.GetComponentInChildren<FuelPack>();
                    if (fuelPack.GetFuel() >= printCost) {
                        fuelPack.DrainFuel(printCost);
                        StartCoroutine(PrintPart(PartPrinterData.instance.modulePrefabs[prefabIndex]));
                    } else Debug.LogWarning("PartPrinter: Not enough energy");
                } else Debug.LogWarning("PartPrinter: Not wearing Fuel Pack");
            } else StartCoroutine(PrintPart(PartPrinterData.instance.modulePrefabs[prefabIndex]));
        } else Debug.LogWarning("PartPrinter: Print surface obstructed");
    }

    void OnTriggerStay(Collider other) {
        printSurfaceClear = false;
    }

    void OnTriggerExit(Collider other) {
        printSurfaceClear = true;
    }

    IEnumerator PrintPart(GameObject shipModule) {
        foreach (ParticleSystem particleSystem in printParticles) particleSystem.Play();
        yield return new WaitForSeconds(0.2f);

        newPart = Instantiate(shipModule, printPoint.position, printPoint.rotation).GetComponent<ShipModule>();
        SceneManager.instance.AddGravityBody(newPart.GetComponent<Rigidbody>());
        newPart.Dematerialize(PartPrinterData.instance.printMaterial);
        meshRenderer = newPart.GetComponent<MeshRenderer>();
        SetPartOpacity(partOpacity);
        yield return new WaitForSeconds(PartPrinterData.instance.printTime);

        foreach (ParticleSystem particleSystem in printParticles) particleSystem.Stop();
        yield return new WaitForSeconds(0.2f);

        newPart.Materialize();
        newPart = null;
        partOpacity = 0f;
    }
}

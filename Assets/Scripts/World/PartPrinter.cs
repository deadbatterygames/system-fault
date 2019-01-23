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
    bool printing;
    float alpha = 0f;

    private void Start() {
        if (!printPoint) Debug.LogError("PartPrinter: No print point set");
        printParticles = GetComponentsInChildren<ParticleSystem>();
    }

    void Update () {
        // TODO: This should probably be in a shader
        if (newPart) {
            alpha += Time.deltaTime / PartPrinterData.instance.printTime;
            SetPartAlpha(alpha);
        }
    }

    void SetPartAlpha(float alpha) {
        foreach (Material mat in meshRenderer.materials) {
            mat.color = new Color(mat.color.r, mat.color.g, mat.color.b, alpha);
        }
    }

    public void ProcessPrintSignal(Vector2Int index) {
        int prefabIndex;

        if (index.x < PartPrinterData.MODULE_TYPES) {
            prefabIndex = index.x * PartPrinterData.MODULE_TIERS + index.y;

            Debug.Log("PartPrinter: Print request for index " + index + " / " + prefabIndex);

            float printCost = PartPrinterData.instance.printCosts[prefabIndex];

            if (!printing) {
                if (printSurfaceClear) {
                    if (printCost > 0f) {
                        Player player = FindObjectOfType<Player>();
                        if (player.GetComponentInChildren<ModuleSlot>().connectedModule) {
                            EnergyPack energyPack = player.GetComponentInChildren<EnergyPack>();
                            if (energyPack.GetEnergy() >= printCost) {
                                energyPack.DrainEnergy(printCost);
                                StartCoroutine(PrintPart(PartPrinterData.instance.modulePrefabs[prefabIndex]));
                            } else PlayerHUD.instance.SetInfoPrompt("Not enough Energy to print module");
                        } else PlayerHUD.instance.SetInfoPrompt("Equip an Energy Pack to print modules");
                    } else StartCoroutine(PrintPart(PartPrinterData.instance.modulePrefabs[prefabIndex]));
                } else PlayerHUD.instance.SetInfoPrompt("Print area obstructed - Clear surface and try again");
            }
        } else Debug.LogError("PartPrinter: Invalid module type index (" + index + ")");
    }

    void OnTriggerStay(Collider other) {
        if (!other.isTrigger) printSurfaceClear = false;
    }

    void OnTriggerExit(Collider other) {
        printSurfaceClear = true;
    }

    IEnumerator PrintPart(GameObject shipModule) {
        printing = true;

        foreach (ParticleSystem particleSystem in printParticles) particleSystem.Play();
        yield return new WaitForSeconds(0.2f);

        newPart = Instantiate(shipModule, printPoint.position, printPoint.rotation).GetComponent<ShipModule>();
        GameManager.instance.AddGravityBody(newPart.GetComponent<Rigidbody>());
        newPart.Dematerialize(PartPrinterData.instance.printMaterial);
        meshRenderer = newPart.GetComponent<MeshRenderer>();
        SetPartAlpha(alpha);
        yield return new WaitForSeconds(PartPrinterData.instance.printTime);

        foreach (ParticleSystem particleSystem in printParticles) particleSystem.Stop();
        yield return new WaitForSeconds(0.2f);

        newPart.Materialize();
        newPart = null;
        alpha = 0f;

        printing = false;
    }
}

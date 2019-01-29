using UnityEngine;

//
// ModuleSlot.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Connects ship modules to the ship
//

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]

public class ModuleSlot : MonoBehaviour {

    public GameTypes.ModuleType slotType;

    static float moduleSnapSpeed = 5f;

    [HideInInspector] public ShipModule connectedModule;

    Material[] defaultMaterials;
    Material[] reservedMaterials;

    MeshRenderer meshRenderer;
    Collider slotTrigger;

    void Awake() {
        meshRenderer = GetComponent<MeshRenderer>();
        defaultMaterials = meshRenderer.materials;
        reservedMaterials = new Material[meshRenderer.materials.Length];
        for (int i = 0; i < reservedMaterials.Length; i++) reservedMaterials[i] = PlayerData.instance.reservedSlotMaterial;

        slotTrigger = GetComponent<Collider>();
    }

    void OnTriggerStay(Collider other) {
        ShipModule module = other.GetComponent<ShipModule>();
        if (module && !connectedModule && module.moduleType == slotType && !module.IsMaterialized()) {
            MatterManipulator matterManipulator = module.GetComponentInParent<MatterManipulator>();
            if (matterManipulator) matterManipulator.AssignModuleSlot(this);
            meshRenderer.materials = reservedMaterials;
        }
    }

    void OnTriggerExit(Collider other) {
        ShipModule module = other.GetComponent<ShipModule>();
        if (module && !connectedModule && module.moduleType == slotType && !module.IsMaterialized()) {
            MatterManipulator matterManipulator = module.GetComponentInParent<MatterManipulator>();
            if (matterManipulator) matterManipulator.AssignModuleSlot(null);
            meshRenderer.materials = defaultMaterials;
        }
    }

    void FixedUpdate() {
        if (connectedModule) SnapModule();
    }

    void SnapModule() {
        if (Vector3.Distance(connectedModule.transform.localPosition, Vector3.zero) > 0.05f || Quaternion.Angle(connectedModule.transform.localRotation, Quaternion.identity) > 1f) {
            connectedModule.transform.localPosition = Vector3.Lerp(connectedModule.transform.localPosition, Vector3.zero, moduleSnapSpeed * Time.fixedDeltaTime);
            connectedModule.transform.localRotation = Quaternion.Slerp(connectedModule.transform.localRotation, Quaternion.identity, moduleSnapSpeed * Time.fixedDeltaTime);
        } else {
            if (!connectedModule.IsMaterialized()) {
                connectedModule.transform.localPosition = Vector3.zero;
                connectedModule.transform.localRotation = Quaternion.identity;
                connectedModule.Materialize();
            }
        }
    }

    public void ToggleSlot(bool toggle) {
        meshRenderer.enabled = toggle;
        slotTrigger.enabled = toggle;

        if (!toggle) meshRenderer.materials = defaultMaterials;
    }
}

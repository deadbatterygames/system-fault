using UnityEngine;

//
// MatterManipulator.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Picks up materializable objects, and connects ship modules
//

public class MatterManipulator : MonoBehaviour, IWeapon {

    [SerializeField] float dematRange = 5f;
    [SerializeField] float snapSpeed = 5f;
    [SerializeField] Material dematMaterial;
    [SerializeField] ParticleSystem energyParticles;

    [SerializeField] Transform snapPoint;
    GameObject heldObject;
    ShipModule heldModule;

    ModuleSlot assignedSlot;

	void Start () {
        if (snapPoint == null) Debug.LogError("MatterManipulator: No snap point set");
		if (dematMaterial == null) Debug.LogError("MatterManipulator: No demat material set");
	}

    public void ConnectAllModules() {
        foreach (ShipModule module in FindObjectsOfType<ShipModule>()) foreach (ModuleSlot moduleSlot in GameManager.instance.ship.GetComponentsInChildren<ModuleSlot>()) {
            if (!moduleSlot.connectedModule && moduleSlot.slotType == module.moduleType) {
                ConnectModule(module, moduleSlot);
                break;
            }
        }
    }

    public void Fire() {
        if (heldObject == null) {
            RaycastHit? hit = FindObjectOfType<PlayerCamera>().GetMaterializableTarget(dematRange);
            if (hit != null) GrabObject(hit.Value.collider.gameObject);
        } else {
            if (assignedSlot) ConnectModule(heldModule, assignedSlot);
            else DropObject();
        }
    }

    public void AssignModuleSlot(ModuleSlot slot) {
        assignedSlot = slot;
    }

    public void EquipEnergyPack(ModuleSlot playerSlot) {
        if (heldObject != null) {
            if (heldObject.GetComponent<EnergyPack>()) {
                if (playerSlot.connectedModule == null) {
                    PlayerHUD.instance.EnableEnergyPackHUD(heldObject.GetComponent<EnergyPack>());
                    string moduleName = heldModule.GetName();
                    ConnectModule(heldModule, playerSlot);
                    PlayerHUD.instance.SetInfoPrompt(moduleName + " equipped");
                } else PlayerHUD.instance.SetInfoPrompt("Another Energy Pack is already equipped");
            } else PlayerHUD.instance.SetInfoPrompt("Not holding a Energy Pack");
        } else {
            if (playerSlot.connectedModule != null) {
                PlayerHUD.instance.DisableEnergyPackHUD();
                GrabObject(playerSlot.connectedModule.gameObject);
                assignedSlot = null;
            }
        }
    }

    void GrabObject(GameObject gameObject) {
        heldObject = gameObject;

        heldModule = gameObject.GetComponent<ShipModule>();
        if (heldModule) {
            if (heldModule.connected) {
                DisconnectModule(gameObject.GetComponentInParent<ModuleSlot>());
                heldModule.shipRB = null;
                GameManager.instance.ship.UpdateModuleStatus(heldModule, false);
            }

            GameManager.instance.ship.ToggleModuleSlots(heldModule.moduleType, true);

            if (heldModule.moduleType == GameTypes.ModuleType.EnergyPack) PlayerHUD.instance.SetInfoPrompt("Press RMB to equip/unequip Energy Pack");
        }

        gameObject.transform.parent = transform;

        IMaterializeable materializable = gameObject.GetComponent<IMaterializeable>();
        materializable.Dematerialize(dematMaterial);

        PlayerHUD.instance.ToggleCrosshair(false);
        PlayerHUD.instance.ToggleUsePrompt(false);
        PlayerHUD.instance.ToggleDematPrompt(false);
        PlayerCamera.instance.checkForUsable = false;
        PlayerCamera.instance.checkForMaterializable = false;

        energyParticles.Play();
    }

    void DropObject() {
        IMaterializeable mObject = heldObject.GetComponent<IMaterializeable>();

        if (!Physics.Linecast(transform.position, snapPoint.position) && !mObject.IsColliding()) {
            if (heldModule) GameManager.instance.ship.ToggleModuleSlots(heldModule.moduleType, false);

            heldObject.transform.parent = null;
            mObject.Materialize();
            heldObject.GetComponent<Rigidbody>().velocity = GetComponentInParent<Rigidbody>().velocity;
            heldObject = null;

            PlayerHUD.instance.ToggleCrosshair(true);
            PlayerCamera.instance.checkForUsable = true;
            PlayerCamera.instance.checkForMaterializable = true;

            energyParticles.Stop();
        } else {
            PlayerHUD.instance.SetInfoPrompt("Cannot materialize object here");
        }
    }

    void ConnectModule(ShipModule module, ModuleSlot slot) {
        Rigidbody rb = module.GetComponent<Rigidbody>();
        GameManager.instance.RemoveGravityBody(rb);
        Destroy(rb);

        assignedSlot = null;
        GameManager.instance.ship.ToggleModuleSlots(slot.slotType, false);
        PlayerHUD.instance.SetInfoPrompt(module.GetName() + " connected");

        module.transform.SetParent(slot.transform);
        slot.connectedModule = module;
        module.connected = true;

        GameManager.instance.ship.UpdateModuleStatus(module, true);
        module.shipRB = GameManager.instance.ship.GetComponent<Rigidbody>();

        heldObject = null;
        module = null;

        PlayerHUD.instance.ToggleCrosshair(true);

        PlayerCamera.instance.checkForUsable = true;
        PlayerCamera.instance.checkForMaterializable = true;

        energyParticles.Stop();
    }

    void DisconnectModule(ModuleSlot slot) {
        Rigidbody rb = heldObject.AddComponent<Rigidbody>();
        rb.mass = heldModule.mass;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        GameManager.instance.AddGravityBody(rb);

        slot.connectedModule = null;
        heldObject.GetComponent<ShipModule>().connected = false;
        assignedSlot = slot;

        GameManager.instance.ship.ToggleModuleSlots(slot.slotType, true);
    }

    void FixedUpdate() {
        if (heldObject) {
            SnapObject();
        }
    }

    void SnapObject() {
        if (Vector3.Distance(heldObject.transform.localPosition, snapPoint.localPosition) > 0.02f) {
            heldObject.transform.localPosition = Vector3.Lerp(heldObject.transform.localPosition, snapPoint.localPosition, snapSpeed * Time.fixedDeltaTime);
            heldObject.transform.localRotation = Quaternion.Slerp(heldObject.transform.localRotation, snapPoint.localRotation, snapSpeed * Time.fixedDeltaTime);
        }
    }

    public bool IsHoldingObject() {
        if (heldObject) return true;
        return false;
    }
}

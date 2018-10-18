using UnityEngine;

//
// ShipModule.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Base class for all the ship modules
//

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]

public class ShipModule : MonoBehaviour, IMaterializeable {

    [HideInInspector] public GameTypes.ModuleType moduleType;

    [HideInInspector] public float mass;

    Material[] defaultMaterials;

    bool materialized = true;
    bool colliding;

    [HideInInspector] public bool connected = false;
    [HideInInspector] public Rigidbody shipRB = null;

    protected virtual void Awake() {
        defaultMaterials = GetComponent<MeshRenderer>().materials;

        if (GetComponent<Rigidbody>()) {
            mass = GetComponent<Rigidbody>().mass;
        } else {
            Debug.LogError("ShipModule: No attached rigidbody");
        }
    }

    public void Materialize() {
        // Collider
        if (!GetComponentInParent<Player>()) GetComponent<Collider>().isTrigger = false;

        // Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        // Texture
        GetComponent<MeshRenderer>().materials = defaultMaterials;
        materialized = true;
    }

    public void Dematerialize(Material demat) {
        // Collider
        GetComponent<Collider>().isTrigger = true;

        // Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        // Texture
        Material[] demats = new Material[defaultMaterials.Length];
        for (int i = 0; i < demats.Length; i++) demats[i] = demat;
        GetComponent<MeshRenderer>().materials = demats;
        materialized = false;
    }

    void OnTriggerStay(Collider other) {
        if (!other.isTrigger) colliding = true;
    }

    void OnTriggerExit(Collider other) {
        if (!other.isTrigger) colliding = false;
    }

    public bool IsColliding() {
        return colliding;
    }

    public bool IsMaterialized() {
        return materialized;
    }
}

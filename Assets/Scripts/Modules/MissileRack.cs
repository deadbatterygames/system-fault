using UnityEngine;

//
// MissileRack.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Fires missiles
//

public class MissileRack : ShipModule, IWeapon {

    [SerializeField] int missiles = 1;
    [SerializeField] GameObject missilePrefab;
    [SerializeField] Transform[] firePoints;
    [SerializeField] Mesh[] meshes;

    MeshFilter meshFilter;
    MeshCollider meshCollider;


    protected override void Awake() {
        base.Awake();
        moduleType = GameTypes.ModuleType.MissileRack;

        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void Fire() {
        if (missiles > 0) {
            missiles--;

            meshFilter.mesh = meshes[missiles];
            meshCollider.sharedMesh = meshes[missiles];

            Rigidbody rb = Instantiate(missilePrefab, firePoints[missiles].position, firePoints[missiles].rotation).GetComponent<Rigidbody>();
            rb.velocity = GameManager.instance.ship.GetComponent<Rigidbody>().velocity;

            GameManager.instance.ship.UpdateMissileCount(-1);

            if (missiles == 0) GameManager.instance.ship.DetachModule(GetComponentInParent<ModuleSlot>());
        }
    }

    public int GetMissileCount() {
        return missiles;
    }
}

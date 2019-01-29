using UnityEngine;

//
// MissileRack.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Fires missiles
//

public class MissileRack : ShipModule, IWeapon {

    [SerializeField] GameObject missilePrefab;
    [SerializeField] Transform[] firePoints;
    [SerializeField] Mesh[] meshes;

    MeshFilter meshFilter;
    MeshCollider meshCollider;

    int fireCount = 0;

    protected override void Awake() {
        base.Awake();
        moduleType = GameTypes.ModuleType.MissileRack;

        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public void Fire() {
        if (fireCount < firePoints.Length) {
            meshFilter.mesh = meshes[fireCount];
            meshCollider.sharedMesh = meshes[fireCount];

            Rigidbody rb = Instantiate(missilePrefab, firePoints[fireCount].position, firePoints[fireCount].rotation).GetComponent<Rigidbody>();
            rb.velocity = GameManager.instance.ship.GetComponent<Rigidbody>().velocity;

            fireCount++;

            if (fireCount == firePoints.Length) GameManager.instance.ship.DetachModule(GetComponentInParent<ModuleSlot>());
        }
    }
}

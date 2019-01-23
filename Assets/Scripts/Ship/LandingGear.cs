using UnityEngine;

//
// LandingGear.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Controls landing gear on the ship
//

[RequireComponent(typeof(SkinnedMeshRenderer))]

public class LandingGear : MonoBehaviour {

    [SerializeField] float extensionSpeed = 100f;

    SkinnedMeshRenderer mesh;
    BoxCollider[] boxColliders;

    bool extend = true;

	void Start () {
        mesh = GetComponent<SkinnedMeshRenderer>();

        boxColliders = GetComponents<BoxCollider>();
        if (boxColliders.Length == 0) Debug.LogWarning("LandingGear: No attached box colliders");
	}
	
	void FixedUpdate () {
		if (extend) {
            mesh.SetBlendShapeWeight(0, Mathf.MoveTowards(mesh.GetBlendShapeWeight(0), 100f, extensionSpeed * Time.fixedDeltaTime));
            if (mesh.GetBlendShapeWeight(0) > 95f) {
                mesh.SetBlendShapeWeight(1, Mathf.MoveTowards(mesh.GetBlendShapeWeight(1), 100f, extensionSpeed * Time.fixedDeltaTime));
            }
        } else {
            mesh.SetBlendShapeWeight(1, Mathf.MoveTowards(mesh.GetBlendShapeWeight(1), 0f, extensionSpeed * Time.fixedDeltaTime));
            if (mesh.GetBlendShapeWeight(1) < 5f) {
                mesh.SetBlendShapeWeight(0, Mathf.MoveTowards(mesh.GetBlendShapeWeight(0), 0f, extensionSpeed * Time.fixedDeltaTime));
            }
        }
    }

    public void Extend() {
        extend = true;
        foreach (BoxCollider bc in boxColliders) bc.enabled = true;
    }

    public void Retract() {
        extend = false;
        foreach (BoxCollider bc in boxColliders) bc.enabled = false;
    }
}

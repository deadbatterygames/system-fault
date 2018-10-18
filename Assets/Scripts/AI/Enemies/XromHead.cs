using UnityEngine;

//
// XromHead.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Controls the Xrom's head
//

[RequireComponent(typeof(XromPart))]

public class XromHead : MonoBehaviour {

    [SerializeField] Material onMaterial;
    [SerializeField] Material warnMaterial;
    [SerializeField] Material offMaterial;

    MeshRenderer headLightMesh;

    void Start() {
        headLightMesh = GetComponent<MeshRenderer>();
    }

    public void EyeOn() {
        Material[] mats = headLightMesh.materials;
        mats[0] = onMaterial;
        headLightMesh.materials = mats;
    }

    public void EyeWarn() {
        Material[] mats = headLightMesh.materials;
        mats[0] = warnMaterial;
        headLightMesh.materials = mats;
    }

    public void EyeOff() {
        Material[] mats = headLightMesh.materials;
        mats[0] = offMaterial;
        headLightMesh.materials = mats;
    }
}

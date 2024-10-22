﻿using UnityEngine;

public class ShipLight : MonoBehaviour, IPowerable {

    [SerializeField] float lightExtensionSpeed = 200f;

    [SerializeField] Material onMaterial;
    [SerializeField] Material offMaterial;

    Light spotLight;
    SkinnedMeshRenderer lightMesh;

    bool powered = false;

    public void TogglePower(bool toggle) {
        ToggleLight(toggle);
        powered = toggle;
    }

    void Start () {
        spotLight = GetComponentInChildren<Light>();
        lightMesh = GetComponent<SkinnedMeshRenderer>();
	}

    void FixedUpdate() {
        // Extend/retract light
        if (powered) {
            lightMesh.SetBlendShapeWeight(0, Mathf.MoveTowards(lightMesh.GetBlendShapeWeight(0), 100f, lightExtensionSpeed * Time.fixedDeltaTime));
        } else {
            lightMesh.SetBlendShapeWeight(0, Mathf.MoveTowards(lightMesh.GetBlendShapeWeight(0), 0f, lightExtensionSpeed * Time.fixedDeltaTime));
        }
    }

    void ToggleLight(bool toggle) {
        Material[] mats = lightMesh.materials;

        if (toggle) {
            spotLight.enabled = true;
            mats[2] = onMaterial;
        } else {
            spotLight.enabled = false;
            mats[2] = offMaterial;
        }

        lightMesh.materials = mats;
    }
}

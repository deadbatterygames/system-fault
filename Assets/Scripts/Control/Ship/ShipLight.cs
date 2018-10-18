using UnityEngine;

public class ShipLight : MonoBehaviour, IPowerable {

    [SerializeField] float lightExtensionSpeed = 200f;

    [SerializeField] Material onMaterial;
    [SerializeField] Material offMaterial;

    Light spotLight;
    SkinnedMeshRenderer lightMesh;

    bool powered = false;
    bool lit = false;

    public void TogglePower(bool toggle) {
        powered = toggle;
        if (!powered) ToggleLight(false);
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

        // Turn on light once extended
        if (lightMesh.GetBlendShapeWeight(0) == 100f && !lit) ToggleLight(true);
    }

    void ToggleLight(bool toggle) {
        Material[] mats = lightMesh.materials;

        if (toggle) {
            spotLight.enabled = true;
            mats[2] = onMaterial;
            lit = true;
        } else {
            spotLight.enabled = false;
            mats[2] = offMaterial;
            lit = false;
        }

        lightMesh.materials = mats;
    }
}

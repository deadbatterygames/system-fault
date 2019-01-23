using UnityEngine;

//
// XromHeatSink.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Allows players to explode Xroms to damage other enemies
//

public class XromHeatSink : MonoBehaviour, IDamageable {

    static float heatIntensity = 3f;
    static float maxHeatLevel = 18f;
    static float cooldownRate = 5f;

    float heatLevel = 0f;
    bool cooldown;

    XromWalker walker;
    MeshRenderer meshRenderer;
    Color heatSinkColour;

    void Start() {
        walker = GetComponentInParent<XromWalker>();
        meshRenderer = GetComponent<MeshRenderer>();
        heatSinkColour = meshRenderer.materials[0].GetColor("_EmissionColor");
    }

    void Update() {
        if (cooldown) Cooldown();
    }

    public void Damage(float amount, GameTypes.DamageType damageType, Vector3 damageForce) {
        heatLevel += amount;
        if (heatLevel >= maxHeatLevel) DestroyHeatSink();
        else {
            cooldown = true;
        }
    }

    void UpdateMaterial() {
        float heatValue = (heatLevel / maxHeatLevel) * heatIntensity;
        meshRenderer.materials[0].SetColor("_EmissionColor", new Color(heatSinkColour.r + heatValue, heatSinkColour.g + heatValue, heatSinkColour.b + heatValue));
    }

    void Cooldown() {
        heatLevel -= cooldownRate * Time.deltaTime;
        if (heatLevel <= 0f) heatLevel = 0f;

        UpdateMaterial();
    }

    void DestroyHeatSink() {
        GetComponent<ParticleSystem>().Play();
        walker.DestroyHeatSink();

        DisableHeatSink();
    }

    public void DisableHeatSink() {
        Destroy(GetComponent<Collider>());
        meshRenderer.materials[0].SetColor("_EmissionColor", Color.black);
        Destroy(this);
    }
}

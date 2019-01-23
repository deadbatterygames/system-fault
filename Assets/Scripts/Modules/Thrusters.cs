using UnityEngine;

//
// Thrusters.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Powers forward/back movement of the ship
//

public class Thrusters : ShipModule, IPowerable {

    [Header("Thruster Specs")]
    [SerializeField] float acceleration = 50f;
    public float maxAstroSpeed = 50f;
    public float astroAcceleration = 1f;
    public float efficiency = 1f;

    float throttle = 0f;
    float astroThrottle = 0f;

    [Header("Trails")]
    TrailRenderer[] trails;
    bool trailsEmitting = false;

    [Header("Engine Glow")]
    [SerializeField] Material onMaterial;
    [SerializeField] Material offMaterial;
    Light engineLight;

    protected override void Awake() {
        base.Awake();
        moduleType = GameTypes.ModuleType.Thrusters;
        trails = GetComponentsInChildren<TrailRenderer>();
        engineLight = GetComponentInChildren<Light>();
    }

    void FixedUpdate() {
        if (connected) {
            Thrust();

            if (throttle > 0f || astroThrottle > 0f && !trailsEmitting) ToggleTrails(true);
            else if (throttle <= 0f && astroThrottle <= 0f && trailsEmitting) ToggleTrails(false);
        }
    }

    public void SetThrottle(float amount) {
        throttle = amount;
    }

    public void SetAstroThrottle(float amount) {
        astroThrottle = Mathf.Clamp(amount, -0.5f, 1f);
    }

    public void AdjustAstroThrottle(float amount) {
        astroThrottle += amount;
        astroThrottle = Mathf.Clamp(astroThrottle, -0.5f, 1f);
        if (PlayerCamera.instance.IsThirdPerson() && amount != 0f) PlayerHUD.instance.SetInfoPrompt("Throttle: " + (astroThrottle * 100f).ToString("F0") + "%");

    }

    public float GetThrottle() {
        return throttle;
    }

    public float GetAstroThrottle() {
        return astroThrottle;
    }

    public float GetAstroSpeed() {
        return astroThrottle * maxAstroSpeed;
    }

    public float GetAstroEnergy() {
        return Mathf.Abs(astroThrottle) / efficiency;
    }

    void Thrust() {
        shipRB.AddForce(shipRB.transform.forward * throttle * acceleration, ForceMode.Acceleration);
    }

    public void ToggleTrails(bool toggle) {
        if (toggle) {
            foreach (TrailRenderer trail in trails) trail.emitting = true;
            trailsEmitting = true;
        } else {
            foreach (TrailRenderer trail in trails) trail.emitting = false;
            trailsEmitting = false;
        }
    }

    public void TogglePower(bool toggle) {
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        Material[] mats = mesh.materials;

        if (toggle) {
            engineLight.enabled = true;
            mats[0] = onMaterial;
        } else {
            SetThrottle(0f);
            SetAstroThrottle(0f);
            engineLight.enabled = false;
            mats[0] = offMaterial;
        }

        mesh.materials = mats;
    }
}

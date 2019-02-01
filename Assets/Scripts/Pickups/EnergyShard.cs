using UnityEngine;
using System.Collections;

//
// Energy.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Pickup script for energy
//

public class EnergyShard : MonoBehaviour, ICollectable {

    Rigidbody rb;
    MeshCollider mc;
    const float ENERGY_UNITS = 1f;

    bool magnetismEnabled;
    const float MAGNETISM_FORCE = 50f;
    const float MAGNETISM_DELAY = 1f;
    const float MAGNETISM_SPEED = 50f;
    const float MAGNETISM_DISTANCE = 150f;
    const float MINIMUM_RIGIDBODY_SPEED = 0.2f;

    bool collected;
    bool root;

    public GameTypes.EnergyState currentState = GameTypes.EnergyState.Normal;

    void Awake() {
        rb = GetComponent<Rigidbody>();
        if (rb) GameManager.instance.AddGravityBody(rb);
        else root = true;

        mc = GetComponent<MeshCollider>();

        StartCoroutine("MagnetizeTimer");
    }

    public void Pickup() {
        StartCoroutine("Vapourize");
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && !collected) {
            if (other.GetComponentInChildren<ModuleSlot>().connectedModule) {
                EnergyPack fp = other.GetComponentInChildren<EnergyPack>();
                AddEnergyToPack(fp);
            } else PlayerHUD.instance.SetInfoPrompt("Equip an Energy Pack to collect energy");
        }

        if (other.CompareTag("Ship") && !collected) {
            EnergyPack fp = GameManager.instance.ship.GetEnergyPack();
            if (fp) {
                AddEnergyToPack(fp);
                if (fp.IsFull()) Pickup();
            }
        }
    }

    void AddEnergyToPack(EnergyPack energyPack) {
        if (!energyPack.IsFull()) {
            energyPack.AddEnergy(ENERGY_UNITS);
            PlayerHUD.instance.ShowEnergySplash();
            Pickup();
        } else PlayerHUD.instance.SetInfoPrompt("Energy Pack full");
    }

    void ChangeEnergyState(GameTypes.EnergyState state) {
        switch (state) {
            case GameTypes.EnergyState.Normal:
                if (!rb) CreateGraviyBody();
                rb.useGravity = true;
                mc.enabled = true;
                rb.drag = 0f;
                break;
            case GameTypes.EnergyState.Magnetize:
                if (!rb) CreateGraviyBody();
                rb.useGravity = false;
                rb.drag = 1f;
                mc.enabled = true;
                break;
            case GameTypes.EnergyState.Sleep:
                if (rb) DestroyGravityBody();
                mc.enabled = false;
                break;
        }

        currentState = state;
    }

    void FixedUpdate() {
        if (!root && !collected && magnetismEnabled) {
            switch (currentState) {
                case GameTypes.EnergyState.Normal:
                    if (ShipMagnetizing()) {
                        ChangeEnergyState(GameTypes.EnergyState.Magnetize);
                        break;
                    }
                    if (rb.velocity.sqrMagnitude < MINIMUM_RIGIDBODY_SPEED * MINIMUM_RIGIDBODY_SPEED)
                        ChangeEnergyState(GameTypes.EnergyState.Sleep);
                    break;
                case GameTypes.EnergyState.Magnetize:
                    if (rb) rb.AddForce(VectorToShip().normalized * MAGNETISM_FORCE, ForceMode.Acceleration);
                    if (!ShipMagnetizing()) ChangeEnergyState(GameTypes.EnergyState.Normal);
                    break;
                case GameTypes.EnergyState.Sleep:
                    if (ShipMagnetizing()) ChangeEnergyState(GameTypes.EnergyState.Magnetize);
                    break;
                
            }
        }
    }

    bool ShipMagnetizing() {
        return VectorToShip().sqrMagnitude < MAGNETISM_DISTANCE * MAGNETISM_DISTANCE && GameManager.instance.ship.IsPowered();
    }

    Vector3 VectorToShip() {
        return GameManager.instance.ship.transform.position - transform.position;
    }

    void CreateGraviyBody() {
        rb = gameObject.AddComponent<Rigidbody>();
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;
        GameManager.instance.AddGravityBody(rb);
    }

    void DestroyGravityBody() {
        GameManager.instance.RemoveGravityBody(rb);
        Destroy(rb);
    }

    IEnumerator MagnetizeTimer() {
        yield return new WaitForSeconds(MAGNETISM_DELAY);
        magnetismEnabled = true;
    }

    IEnumerator Vapourize() {
        collected = true;

        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<SphereCollider>().enabled = false;
        if (mc) mc.enabled = false;
        if (rb) DestroyGravityBody();

        GetComponent<ParticleSystem>().Emit(50);

        yield return new WaitForSeconds(0.5f);

        Destroy(gameObject);
    }
}

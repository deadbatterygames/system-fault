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
    const float ENERGY_UNITS = 1f;

    bool magnetizeToShip = false;
    const float SQR_MAGNETIZE_DISTANCE = 22500f;

    void Awake() {
        // Add rigidbody to scene
        rb = GetComponent<Rigidbody>();
        if (rb) {
            GameManager.instance.AddGravityBody(rb);
            StartCoroutine("DestroyRigidbody");
        }
    }

    public void Pickup() {
        StartCoroutine("Vapourize");
    }

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            if (other.GetComponentInChildren<ModuleSlot>().connectedModule) {
                EnergyPack fp = other.GetComponentInChildren<EnergyPack>();
                AddEnergyToPack(fp);
            } else PlayerHUD.instance.SetInfoPrompt("Equip an Energy Pack to collect energy");
        }

        if (other.CompareTag("Ship")) {
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

    void FixedUpdate() {
        if (magnetizeToShip && GameManager.instance.ship.IsPowered()) {
            Vector3 toShip = GameManager.instance.ship.transform.position - transform.position;
            float sqrDistance = toShip.sqrMagnitude;
            if (sqrDistance < SQR_MAGNETIZE_DISTANCE) transform.Translate(toShip.normalized * Time.fixedDeltaTime * Mathf.Clamp(SQR_MAGNETIZE_DISTANCE / sqrDistance, 10f, 150f), Space.World);
        }
    }

    IEnumerator DestroyRigidbody() {
        yield return new WaitForSeconds(2f);

        Rigidbody rb = GetComponent<Rigidbody>();
        GameManager.instance.RemoveGravityBody(rb);
        Destroy(rb);
        Destroy(GetComponent<MeshCollider>());

        magnetizeToShip = true;
    }

    IEnumerator Vapourize() {
        Destroy(GetComponent<SphereCollider>());
        Destroy(GetComponent<MeshRenderer>());
        Destroy(GetComponent<MeshCollider>());

        if (rb) {
            GameManager.instance.RemoveGravityBody(rb);
            Destroy(rb);
        }

        GetComponent<ParticleSystem>().Emit(50);

        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}

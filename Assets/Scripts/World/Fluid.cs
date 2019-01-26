using UnityEngine;
using System.Collections.Generic;
using System.Linq;

//
// Fluid.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Simulates fluid physics
//

[RequireComponent(typeof(SphereCollider))]

public class Fluid : MonoBehaviour
{
    [SerializeField] float buoyancy = 1f;
    [SerializeField] float viscosity = 3f;
    [SerializeField] float angularViscosity = 0.5f;

    List<Rigidbody> rigidbodies = new List<Rigidbody>();

    float sqrBuoyancy;

    void Start() {
        if (!GetComponent<SphereCollider>().isTrigger) Debug.LogError("Fluid: " + gameObject.name + "'s collider is not a trigger");
        sqrBuoyancy = buoyancy * buoyancy * GameManager.GRAVITY_CONSTANT;
    }

    void FixedUpdate() {
        if (rigidbodies.Count > 0) {
            foreach (Rigidbody rb in rigidbodies) {
                if (rb) {
                    if (!rb.isKinematic) {
                        Vector3 toObject = rb.position - transform.position;
                        rb.AddForce(toObject.normalized * sqrBuoyancy / toObject.sqrMagnitude, ForceMode.Acceleration);
                    }
                } else rigidbodies = rigidbodies.Where(x => x != null).ToList();
            }
        }
    }

    void OnTriggerEnter(Collider other) {
        Bullet bullet = other.GetComponent<Bullet>();
        if (bullet) {
            bullet.Explode();
            return;
        }

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb) AddFluidBody(rb);

        Player player = other.GetComponent<Player>();
        if (player) {
            player.StopCoroutine("WeaponTimer");
            player.GetComponentInChildren<WeaponSlot>().UnequipAll();
        }
    }

    void OnTriggerExit(Collider other) {
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb) RemoveFluidBody(rb);

        Player player = other.GetComponent<Player>();
        if (player) {
            player.StopCoroutine("WeaponTimer");
            player.StartCoroutine("WeaponTimer");
        }
    }

    public void AddFluidBody(Rigidbody rb) {
        rb.angularDrag = angularViscosity;
        rb.drag = viscosity;
        rigidbodies.Add(rb);
    }

    public void RemoveFluidBody(Rigidbody rb) {
        Ship ship = rb.GetComponent<Ship>();
        if (ship) {
            switch (ship.GetAssistMode()) {
                case GameTypes.AssistMode.NoAssist:
                    if (!ship.IsPowered()) rb.angularDrag = 0f;
                    else rb.angularDrag = 2f;
                    break;
                default:
                    rb.angularDrag = 2f;
                    break;
            }
        } else rb.angularDrag = 0f;
        rb.drag = 0f;
        rigidbodies.Remove(rb);
    }
}

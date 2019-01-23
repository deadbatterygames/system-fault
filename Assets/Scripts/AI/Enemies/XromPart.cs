using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//
// XromPart.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Detaches parts of the Xrom model
//

public class XromPart : MonoBehaviour, IDamageable {

    [SerializeField] GameTypes.XromPartType type;
    [SerializeField] float partHealth = 20f;
    [SerializeField] float partMass = 1f;
    [SerializeField] SkinnedMeshRenderer mesh;
    [SerializeField] List<XromPart> parts;
    float currentHealth;
    bool detached;

    void Start() {
        currentHealth = partHealth;
    }

    public void Damage(float amount, GameTypes.DamageType damageType, Vector3 damageForce) {
        currentHealth -= amount;
        if (currentHealth <= 0f) BreakXrom(damageForce);
    }

    void BreakXrom(Vector3 detachForce) {
        XromWalker xromWalker = GetComponentInParent<XromWalker>();

        SeparatePart(detachForce);

        DetachPartWithVelocity(detachForce);

        // if (xromWalker) {
        //     switch (type) {
        //         case GameTypes.XromPartType.Vital:
        //             xromWalker.DisableXrom();
        //             break;
        //         case GameTypes.XromPartType.Separator:
        //             //xromWalker.SeparateXrom();
        //             break;
        //     }
        // }

        transform.SetParent(null);
    }

    public GameObject SeparatePart(Vector3 detachForce){
        foreach(XromPart part in parts){
            GameObject meshObject = part.SeparatePart(detachForce);
            if(true){
                part.transform.SetParent(transform);
            }
        }

        mesh.transform.SetParent(transform);
        mesh.rootBone = null;

        return null;
    }

    public void DetachPart() {
        Vector3 detachVelocity = GetComponentInParent<Rigidbody>().velocity;
        DetachPartWithVelocity(detachVelocity);
    }

    public void DetachPartWithVelocity(Vector3 detachForce) {
        if (!detached){
            CreatePartRigidbody().velocity = detachForce;
        }
    }

    Rigidbody CreatePartRigidbody() {
        transform.parent = null;

        if (mesh) {
            mesh.transform.parent = transform;

            mesh.rootBone = null;
            mesh.transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
            mesh.transform.localScale = Vector3.one;
            mesh.localBounds = new Bounds(Vector3.zero, Vector3.one * 5f);
        }

        detached = true;

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = partMass;
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc) rb.centerOfMass = bc.center;

        GameManager.instance.AddGravityBody(rb);

        return rb;
    }
}

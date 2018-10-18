using UnityEngine;

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

    float currentHealth;
    bool detached;

    void Start() {
        currentHealth = partHealth;
    }

    public void Damage(float amount, Vector3 damageForce) {
        currentHealth -= amount;
        if (currentHealth <= 0f) BreakXrom(damageForce);
    }

    void BreakXrom(Vector3 detachForce) {
        XromWalker xromWalker = GetComponentInParent<XromWalker>();

        DetachPartWithVelocity(detachForce);

        if (xromWalker) {
            switch (type) {
                case GameTypes.XromPartType.Vital:
                    xromWalker.DisableXrom();
                    break;
                case GameTypes.XromPartType.Separator:
                    xromWalker.SeparateXrom();
                    break;
            }
        }
    }

    public void DetachPart() {
        Vector3 detachVelocity = GetComponentInParent<Rigidbody>().velocity;
        if (!detached) CreatePartRigidbody().velocity = detachVelocity;
    }

    public void DetachPartWithVelocity(Vector3 detachForce) {
        if (!detached) CreatePartRigidbody().velocity = detachForce;
    }

    Rigidbody CreatePartRigidbody() {
        transform.parent = null;
        detached = true;

        Rigidbody rb = gameObject.AddComponent<Rigidbody>();
        rb.mass = partMass;
        BoxCollider bc = GetComponent<BoxCollider>();
        if (bc) rb.centerOfMass = bc.center;

        GameManager.instance.AddGravityBody(rb);

        return rb;
    }
}

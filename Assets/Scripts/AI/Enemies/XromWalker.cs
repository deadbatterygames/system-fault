using UnityEngine;
using System.Collections;

//
// XromWalker.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Moves and destroys Xroms
//

public class XromWalker : MonoBehaviour, IGroundable {

    [Header("General")]
    [SerializeField] float walkSpeed = 5f;
    [SerializeField] float torsoRotationSpeed = 100f;

    [Header("Parts")]
    [SerializeField] XromHead head;
    [SerializeField] XromPart torso;
    [SerializeField] XromPart legs;

    [Header("Explosion")]
    [SerializeField] float explosionBaseDamage = 100f;
    [SerializeField] float explosionRadius = 8f;
    [SerializeField] float explosionForce = 1000f;
    [SerializeField] float explosionDelay = 3f;
    [SerializeField] int warningFlashes = 5;

    Rigidbody rb;

    // TODO: This will be passed by various discovery functions
    Transform target;

    int heatSinks = 2;
    bool xromActive;
    bool grounded;

    public void Start() {
        StartCoroutine("WaitAndFindPlayer");
        rb = GetComponent<Rigidbody>();
        xromActive = true;
    }

    public void Update() {
        if (xromActive && grounded) {
            if (target) RotateToPlayer();
            else ResetTorsoRotation();

            WalkInCircles();
        }
    }

    public void ResetTorsoRotation() {
    }

    public void RotateToPlayer() {
        Vector3 torsoEulerAngles = transform.localEulerAngles;
        torso.transform.Rotate(transform.up, GetTargetDirection() * torsoRotationSpeed * Time.deltaTime, Space.World);
    }

    void WalkInCircles() {
        transform.Rotate(Vector3.up, torsoRotationSpeed * Time.deltaTime);
        torso.transform.Rotate(transform.up, -torsoRotationSpeed * Time.deltaTime, Space.World);
        rb.AddForce(transform.forward, ForceMode.VelocityChange);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, walkSpeed);
    }

    int GetTargetDirection() {
        // TODO: Make a utility function for this...
        Vector3 xromToTarget = (target.position - transform.position).normalized;
        float rightLeft = Vector3.Dot(-torso.transform.right, xromToTarget);
        float forwardBack = Vector3.Dot(torso.transform.up, xromToTarget);

        if (rightLeft > 0) {
            if (forwardBack > 0 && rightLeft < 0.1) return 0;
            else return 1;
        } else if (rightLeft < 0) {
            if (forwardBack > 0 && rightLeft > -0.1) return 0;
            else return -1;
        } else return 0;
    }

    public void SeparateXrom() {
        DisableXrom();

        if (torso) torso.DetachPart();
        if (legs) legs.DetachPart();

        DestroyRoot();
    }

    public void DisableXrom() {
        // Rotation
        GetComponent<Rigidbody>().freezeRotation = false;
        Destroy(GetComponent<CharacterSnap>());

        // Lights
        XromHeatSink[] heatSinks = GetComponentsInChildren<XromHeatSink>();
        foreach (XromHeatSink heatSink in heatSinks) heatSink.DisableHeatSink();

        head.EyeOff();

        xromActive = false;
    }

    public void DestroyHeatSink() {
        heatSinks--;
        if (heatSinks == 0 && xromActive) StartCoroutine("ExplodeXrom");
    }

    void DestroyRoot() {
        Rigidbody rb = GetComponent<Rigidbody>();
        GameManager.instance.RemoveGravityBody(rb);
        StopCoroutine("ExplodeXrom");
        Destroy(gameObject);
    }

    public bool IsXromActive() {
        return xromActive;
    }

    // TODO: FOR TESTING
    IEnumerator WaitAndFindPlayer() {
        yield return new WaitForSeconds(1f);
        target = FindObjectOfType<Player>().transform;
    }

    IEnumerator ExplodeXrom() {
        // Warning Flash
        float flashInterval = explosionDelay / warningFlashes / 2f;
        for (int i =0; i < warningFlashes; i++) {
            head.EyeOff();
            yield return new WaitForSeconds(flashInterval);
            head.EyeWarn();
            yield return new WaitForSeconds(flashInterval);
        }
        
        DisableXrom();

        // Separation
        XromPart[] parts = GetComponentsInChildren<XromPart>();
        foreach (XromPart part in parts) part.DetachPart();

        // Particles
        GetComponent<Rigidbody>().isKinematic = true;
        ParticleSystem explosionParticles = GetComponent<ParticleSystem>();
        explosionParticles.Play();

        // Damage/Explosion force
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders) {
            Rigidbody rb = col.GetComponent<Rigidbody>();
            if (rb) rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);

            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null) {
                Vector3 centerToObject = col.transform.position - transform.position;

                Vector3 damageForce = centerToObject.normalized * explosionForce / centerToObject.magnitude;
                float damageAmount = explosionBaseDamage / centerToObject.magnitude;

                damageable.Damage(damageAmount, damageForce);
            }
        }

        yield return new WaitForSeconds(explosionParticles.main.duration);
        DestroyRoot();
    }

    public void SetGrounded(bool grounded) {
        this.grounded = grounded;
    }
}

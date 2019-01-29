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
    [SerializeField] float walkSpeed = 3f;
    [SerializeField] float torsoRotationSpeed = 100f;

    [Header("Parts")]
    [SerializeField] XromHead head;
    [SerializeField] XromPart torso;
    [SerializeField] XromPart legs;

    [Header("Explosion")]
    [SerializeField] float explosionBaseDamage = 250f;
    [SerializeField] float explosionRadius = 10f;
    [SerializeField] float explosionForce = 30f;
    [SerializeField] float explosionDelay = 2f;
    [SerializeField] int warningFlashes = 10;

    [SerializeField] Xrom xrom;

    // TODO: This will be passed by various discovery functions
    Transform target;

    int heatSinks = 2;
    bool xromActive;
    bool grounded;

    public void Start() {
        xromActive = true;
        //SearchForPlayer();
    }

    public void Update() {
        if (xromActive && grounded) {
            if (target) RotateToTarget(target);
            else ResetTorsoRotation();

            //WalkInCircles();
        }
    }

    public void ResetTorsoRotation() {
    }

    public void RotateToTarget(Transform target) {
        Vector3 torsoEulerAngles = transform.localEulerAngles;
        torso.transform.Rotate(transform.up, GetTargetDirection() * torsoRotationSpeed * Time.deltaTime, Space.World);
    }

    public void Rotate(float theta){
        transform.Rotate(Vector3.up, theta * 10f * torsoRotationSpeed * Time.deltaTime);
        torso.transform.Rotate(transform.up, theta * 10f * (-torsoRotationSpeed) * Time.deltaTime, Space.World);
        // Vector3 extensionForward = Vector3.Dot(rb.velocity, transform.forward) * transform.forward;
        // transform.LookAt(transform.position + extensionForward);
    }

    // void WalkInCircles() {
    //     transform.Rotate(Vector3.up, torsoRotationSpeed * Time.deltaTime);
    //     torso.transform.Rotate(transform.up, -torsoRotationSpeed * Time.deltaTime, Space.World);
    //     rb.AddForce(transform.forward * walkSpeed, ForceMode.VelocityChange);
    //     rb.velocity = Vector3.ClampMagnitude(rb.velocity, walkSpeed);
    // }

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
        Destroy(GetComponent<Animator>());

        // Rotation
        GetComponent<Rigidbody>().freezeRotation = false;
        Destroy(GetComponent<CharacterSnap>());

        // Lights
        XromHeatSink[] heatSinks = GetComponentsInChildren<XromHeatSink>();
        foreach (XromHeatSink heatSink in heatSinks) heatSink.DisableHeatSink();

        head.EyeOff();
        //xrom.DestroyXrom();

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
        Destroy(this);
    }

    public bool IsXromActive() {
        return xromActive;
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
            if (rb) rb.AddExplosionForce(explosionForce, transform.position, explosionRadius, 0f, ForceMode.Impulse);

            IDamageable damageable = col.GetComponent<IDamageable>();
            if (damageable != null) {
                Vector3 centerToObject = col.transform.position - transform.position;

                Vector3 damageForce = centerToObject.normalized * explosionForce;
                float damageAmount = explosionBaseDamage / centerToObject.magnitude;

                damageable.Damage(damageAmount, GameTypes.DamageType.Physical, damageForce);
            }
        }

        yield return new WaitForSeconds(explosionParticles.main.duration);
        DestroyRoot();
    }

    public void SetGrounded(bool grounded) {
        this.grounded = grounded;
    }
}

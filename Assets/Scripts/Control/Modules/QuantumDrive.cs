using UnityEngine;
using System.Collections;

//
// QuantumDrive.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Allows player to Quantum jump to far away planets
//

public class QuantumDrive : ShipModule {

    [Header("Quantum Drive Specs")]
    [SerializeField] float quantumSpeed = 2500f;
    [SerializeField] float lockSpeed = 1f;
    [SerializeField] float stabilizerSpeed = 1f;
    public float efficiency = 1f;

    [Space]
    [SerializeField] float jumpRange = 20000f;
    [SerializeField] float dropRange = 1000f;
    [SerializeField] float searchPrecision = 100f;

    [Header("Engine Glow")]
    [SerializeField] Material onMaterial;
    [SerializeField] Material offMaterial;
    Light quantumLight;
    ParticleSystem quantumParticles;

    Transform target;

    enum Stage {
        Idle,
        Searching,
        Locking,
        Jumping
    }
    Stage currentStage = Stage.Idle;

    protected override void Awake() {
        base.Awake();
        moduleType = GameTypes.ModuleType.QuantumDrive;
        quantumLight = GetComponentInChildren<Light>();
        quantumParticles = GetComponentInChildren<ParticleSystem>();
    }

    public void PickTarget() {
        switch (currentStage) {
            case Stage.Idle:
            case Stage.Searching:
                RaycastHit? hit = PlayerCamera.instance.GetQuantumTarget(jumpRange);
                if (hit != null) {
                    if ((hit.Value.transform.position - transform.position).magnitude > dropRange) {
                        target = hit.Value.transform;
                        Debug.Log("Quantum Drive: Target found - " + hit.Value.transform.parent);
                        ChangeStage(Stage.Searching);
                    } else Debug.Log("Quantum Drive: Target too close");
                } else {
                    ChangeStage(Stage.Idle);
                    Debug.Log("Quantum Drive: No target found in jump range");
                }
                break;
        }
    }

    void FixedUpdate () {
		switch (currentStage) {
            case Stage.Searching:
                Search(target);
                break;
            case Stage.Locking:
                Lock(target);
                break;
            case Stage.Jumping:
                Jump(target);
                break;
        }
	}

    public void Search(Transform jumpTarget) {
        Vector3 shipToPlanet = (jumpTarget.transform.position - transform.position).normalized;
        float forwardBackDot = Vector3.Dot(transform.parent.parent.forward, shipToPlanet);

        float rightLeftDot = Vector3.Dot(transform.parent.parent.right, shipToPlanet);
        int rightLeft = 0;
        if (rightLeftDot > 0) {
            if (forwardBackDot > 0 && rightLeftDot < 1/searchPrecision) rightLeft = 0;
            else rightLeft = 1;
        } else if (rightLeftDot < 0) {
            if (forwardBackDot > 0 && rightLeftDot > -1/searchPrecision) rightLeft = 0;
            else rightLeft = -1;
        }

        float upDownDot = Vector3.Dot(transform.parent.parent.up, shipToPlanet);
        int upDown = 0;
        if (upDownDot > 0) {
            if (forwardBackDot > 0 && upDownDot < 1/searchPrecision) upDown = 0;
            else upDown = 1;
        } else if (upDownDot < 0) {
            if (forwardBackDot > 0 && upDownDot > -1/searchPrecision) upDown = 0;
            else upDown = -1;
        }

        PlayerHUD.instance.UpdatePlanetRadar(rightLeft, upDown);

        if (upDown == 0 && rightLeft == 0) ChangeStage(Stage.Locking);
    }

    public void Lock(Transform jumpTarget) {
        Vector3 toPlanet = (jumpTarget.position - transform.position).normalized;

        shipRB.velocity = Vector3.Lerp(shipRB.velocity, Vector3.zero, stabilizerSpeed * Time.fixedDeltaTime);

        Quaternion targetRotation = Quaternion.FromToRotation(transform.parent.parent.forward, toPlanet) * transform.parent.parent.rotation;
        shipRB.MoveRotation(Quaternion.Slerp(shipRB.rotation, targetRotation, lockSpeed * Time.fixedDeltaTime));
    }

    public void Jump(Transform jumpTarget) {
        Vector3 toPlanet = (jumpTarget.position - transform.position).normalized;

        if (Physics.Raycast(transform.position, toPlanet, dropRange, LayerMask.GetMask("Celestial"))) ChangeStage(Stage.Idle);
        else shipRB.velocity = Vector3.Lerp(shipRB.velocity, toPlanet * quantumSpeed, stabilizerSpeed * Time.fixedDeltaTime);
    }

    void ChangeStage(Stage stage) {
        switch (stage) {
            case Stage.Idle:
                GetComponentInParent<Ship>().ToggleQuantum(false);
                quantumParticles.Stop();
                PlayerCamera.instance.GetComponent<SceneShift>().threshold = 1000f;
                PlayerHUD.instance.TogglePlanetRadar(false);
                break;
            case Stage.Searching:
                Search(target);
                PlayerHUD.instance.TogglePlanetRadar(true);
                break;
            case Stage.Locking:
                GetComponentInParent<Ship>().ToggleQuantum(true);
                StartCoroutine("JumpCountdown");
                PlayerHUD.instance.TogglePlanetRadar(true);
                PlayerHUD.instance.UpdatePlanetRadar(0, 0);
                break;
            case Stage.Jumping:
                quantumParticles.Play();
                PlayerCamera.instance.GetComponent<SceneShift>().threshold = 5000f;
                PlayerHUD.instance.TogglePlanetRadar(false);
                break;
        }

        currentStage = stage;
    }

    public void TogglePower(bool toggle) {
        MeshRenderer mesh = GetComponent<MeshRenderer>();
        Material[] mats = mesh.materials;

        if (toggle) {
            quantumLight.enabled = true;
            mats[0] = onMaterial;
        } else {
            quantumLight.enabled = false;
            mats[0] = offMaterial;
            ChangeStage(Stage.Idle);
        }

        mesh.materials = mats;
    }

    IEnumerator JumpCountdown() {
        Ship ship = GetComponentInParent<Ship>();

        ship.UpdateQuantumCountDown(3);
        yield return new WaitForSeconds(1f);
        ship.UpdateQuantumCountDown(2);
        yield return new WaitForSeconds(1f);
        ship.UpdateQuantumCountDown(1);
        yield return new WaitForSeconds(1f);
        ship.UpdateQuantumCountDown(0);

        ChangeStage(Stage.Jumping);
    }

    public bool IsJumping() {
        return currentStage == Stage.Jumping;
    }
}

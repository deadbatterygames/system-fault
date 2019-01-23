using UnityEngine;

//
// PlayerCamera.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Controls the player camera
//

[RequireComponent(typeof(TargetingSystem))]
[RequireComponent(typeof(Camera))]

public class PlayerCamera : MonoBehaviour {

    public static PlayerCamera instance = null;

    TargetingSystem targetingSystem;

    [Header("Positioning")]
    [SerializeField] float cameraMoveSpeed = 2f;
    [SerializeField] Vector3 shipOffset;

    [Header("Raycasting")]
    [SerializeField] LayerMask targetMask;
    [SerializeField] float usableUIPromptRange = 5f;
    [SerializeField] float dematUIPromptRange = 5f;

    LayerMask materializeMask;
    LayerMask useMask;
    LayerMask quantumMask;

    [HideInInspector] public bool checkForUsable;
    [HideInInspector] public bool checkForMaterializable;

    bool thirdPerson = false;

    void Awake() {
        if (instance == null)
            instance = this;

        else if (instance != this)
            Destroy(gameObject);

        DontDestroyOnLoad(gameObject);

        materializeMask = LayerMask.GetMask("Materializable");
        useMask = LayerMask.GetMask("Usable");
        quantumMask = LayerMask.GetMask("Celestial");
    }

    void Start() {
        targetingSystem = GetComponent<TargetingSystem>();

        checkForUsable = true;
	}

    public RaycastHit? GetPhysicalTarget(float range) {
        return targetingSystem.Target(range, targetMask);
    }

    public RaycastHit? GetUsableTarget(float range) {
        return targetingSystem.Target(range, useMask);
    }

    public RaycastHit? GetMaterializableTarget(float range) {
        return targetingSystem.Target(range, materializeMask);
    }

    public RaycastHit? GetQuantumTarget(float range) {
        return targetingSystem.Target(range, quantumMask);
    }

    void FixedUpdate() {
        if (checkForUsable) {
            RaycastHit? hit = GetUsableTarget(usableUIPromptRange);
            if (hit != null) PlayerHUD.instance.ToggleUsePrompt(true, hit.Value.collider.GetComponent<IUsable>().GetName());
            else PlayerHUD.instance.ToggleUsePrompt(false);
        }

        if (checkForMaterializable) {
            RaycastHit? hit = GetMaterializableTarget(dematUIPromptRange);
            if (hit != null) PlayerHUD.instance.ToggleDematPrompt(true, hit.Value.collider.GetComponent<IMaterializeable>().GetName());
            else PlayerHUD.instance.ToggleDematPrompt(false);
        }

        if (transform.parent) {
            if (!thirdPerson && (transform.localPosition != Vector3.zero || transform.localRotation != Quaternion.identity)) {
                transform.localPosition = Vector3.Lerp(transform.localPosition, Vector3.zero, cameraMoveSpeed * Time.deltaTime);
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.identity, cameraMoveSpeed * Time.deltaTime);
            }
        }
    }

    public void TogglePerspective() {
        if (thirdPerson) {
            SetFirstPerson();
        } else {
            SetThirdPerson();
        }

        transform.localRotation = Quaternion.identity;
    }

    public void SetFirstPerson() {
        transform.localPosition = Vector3.zero;
        thirdPerson = false;
    }

    public void SetThirdPerson() {
        transform.localPosition = shipOffset;
        thirdPerson = true;
    }

    public bool IsThirdPerson() {
        return thirdPerson;
    }

    public void MoveCamToPlayer() {
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}

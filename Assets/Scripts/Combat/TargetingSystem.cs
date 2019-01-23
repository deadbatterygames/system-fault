using UnityEngine;

//
// TargetingSystem.cs
//
// Author: Eric Thompson & Gabriel Cimolino (Dead Battery Games)
// Purpose: Casts rays from the attached camera in order for the player to target various objects
//

[RequireComponent(typeof(Camera))]

public class TargetingSystem : MonoBehaviour {

    Camera cam;
    
    public void Start() {
        cam = GetComponent<Camera>();
    }

    public RaycastHit? Target(float range) {
        Vector3 aimPoint = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));

        Debug.DrawLine(aimPoint, transform.position + cam.transform.forward * range);

        RaycastHit hit;
        if (Physics.Raycast(aimPoint, cam.transform.forward, out hit, range)) return hit;
        return null;
    }

    public RaycastHit? Target(float range, LayerMask mask) {
        Vector3 aimPoint = cam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));

        Debug.DrawLine(aimPoint, transform.position + cam.transform.forward * range);

        RaycastHit hit;
        if (Physics.Raycast(aimPoint, cam.transform.forward, out hit, range, mask)) return hit;
        return null;
    }
}

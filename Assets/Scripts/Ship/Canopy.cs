using UnityEngine;

//
// Canopy.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Opens and closes the ship canopy
//

public class Canopy : MonoBehaviour, IUsable {

    [SerializeField] float speed = 3f;
    SphereCollider shipUsable;

    bool open;

    Vector3 currentPosition;
    Vector3 closedPosition;
    Vector3 openPosition;

    void Start() {
        closedPosition = transform.localPosition;
        openPosition = transform.localPosition - Vector3.forward * 3f;
        currentPosition = closedPosition;
        shipUsable = GetComponentInParent<Ship>().GetComponentInChildren<ShipEnterance>().GetComponent<SphereCollider>();
        if (!shipUsable) Debug.LogError("Canopy: No ShipEnterance found");
    }

    public void Use() {

        if (open) {
            currentPosition = closedPosition;
            open = false;
            shipUsable.enabled = false;
        } else {
            currentPosition = openPosition;
            open = true;
            shipUsable.enabled = true;
        }
    }

    void FixedUpdate() {
        if (transform.localPosition != currentPosition) {
            transform.localPosition = Vector3.MoveTowards(transform.localPosition, currentPosition, speed * Time.fixedDeltaTime);
        }
    }

    public bool IsOpen() {
        return open;
    }

    public string GetName() {
        return "Canopy";
    }
}

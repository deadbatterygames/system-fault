using UnityEngine;

//
// Refinery.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Controls functionality of the refineries
//

public class Refinery : MonoBehaviour {

    [SerializeField] bool zeroG;

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player") && zeroG) {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb) rb.drag = 1f;
        } else if (other.GetComponent<CharacterSnap>() != null) {
            other.GetComponent<CharacterSnap>().SetSnapUp(transform.up);
        }
    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player") && zeroG) {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb) rb.drag = 0f;
        } else if (other.GetComponent<CharacterSnap>() != null) {
            other.GetComponent<CharacterSnap>().RemoveSnapUp();
        }
    }
}

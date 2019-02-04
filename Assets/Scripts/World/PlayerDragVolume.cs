using UnityEngine;

//
// PlayerDragVolume.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Applies drag to player for easier control in space
//

public class PlayerDragVolume : MonoBehaviour {

    void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb) rb.drag = 1f;
        }

    }

    void OnTriggerExit(Collider other) {
        if (other.CompareTag("Player")) {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb) rb.drag = 0f;
        }
    }
}

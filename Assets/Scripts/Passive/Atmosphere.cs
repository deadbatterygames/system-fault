using UnityEngine;

//
// Atmosphere.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Sets various properties on the player
//

public class Atmosphere : MonoBehaviour {

    void OnTriggerEnter(Collider other) {
        // Character Snap
        if (other.GetComponent<CharacterSnap>() != null) {
            other.GetComponent<CharacterSnap>().SetSnapPoint(transform);
        }
    }

    void OnTriggerExit(Collider other) {
        // Character Snap
        if (other.GetComponent<CharacterSnap>() != null) {
            other.GetComponent<CharacterSnap>().RemoveSnapPoint();
        }
    }
}

using UnityEngine;

//
// Atmosphere.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Sets various properties on the player
//

public class Atmosphere : MonoBehaviour {
    [SerializeField] Planet planet;
    void OnTriggerEnter(Collider other) {
        // Character Snap
        if (other.GetComponent<CharacterSnap>() != null) {
            other.GetComponent<CharacterSnap>().SetSnapPoint(transform);
        }

        //planet.OnTriggerEnter(other);
    }

    void OnTriggerExit(Collider other) {
        // Character Snap
        if (other.GetComponent<CharacterSnap>() != null) {
            other.GetComponent<CharacterSnap>().RemoveSnapPoint();
        }
    }
}

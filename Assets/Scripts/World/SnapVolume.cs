using UnityEngine;

//
// Atmosphere.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Activates/deactivates Character Snaps
//

public class SnapVolume : MonoBehaviour {
    [SerializeField] bool snapUp = false;

    void OnTriggerEnter(Collider other) {
        CharacterSnap character = other.GetComponent<CharacterSnap>();
        if (character != null) {
            if (snapUp) character.SetSnapUp(transform.up);
            else character.SetSnapPoint(transform);
        }
    }

    void OnTriggerExit(Collider other) {
        CharacterSnap character = other.GetComponent<CharacterSnap>();
        if (character != null) {
            if (snapUp) character.RemoveSnapUp();
            else character.RemoveSnapPoint();
        }
    }
}

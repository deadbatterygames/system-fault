using UnityEngine;

//
// Atmosphere.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Activates/deactivates Character Snaps
//

public class SnapVolume : MonoBehaviour {
    void OnTriggerEnter(Collider other) {
        CharacterSnap snap = other.GetComponent<CharacterSnap>();
        if (snap != null) snap.SetSnapPoint(transform);
    }

    void OnTriggerExit(Collider other) {
        CharacterSnap snap = other.GetComponent<CharacterSnap>();
        if (snap != null) snap.RemoveSnapPoint();
    }
}

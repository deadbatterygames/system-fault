using UnityEngine;

//
// PlayerExit.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Checks whether the player can exit the ship
//

public class PlayerExit : MonoBehaviour {
	
    void OnTriggerStay(Collider other) {
        if (!other.isTrigger && !other.CompareTag("Ship")) {
            GameManager.instance.ship.SetCanopyClear(false);
        }
    }

    void OnTriggerExit(Collider other) {
        if (!other.isTrigger && !other.CompareTag("Ship")) GameManager.instance.ship.SetCanopyClear(true);
    }
}

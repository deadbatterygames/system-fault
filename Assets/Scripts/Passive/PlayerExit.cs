using UnityEngine;

//
// PlayerExit.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Checks whether the player can exit the ship
//

public class PlayerExit : MonoBehaviour {

    Ship ship;

	void Start () {
        ship = GetComponentInParent<Ship>();
	}
	
    void OnTriggerStay(Collider other) {
        if (!other.isTrigger && !other.CompareTag("Ship")) ship.SetCanopyClear(false);
    }

    void OnTriggerExit(Collider other) {
        if (!other.isTrigger && !other.CompareTag("Ship")) ship.SetCanopyClear(true);
    }
}

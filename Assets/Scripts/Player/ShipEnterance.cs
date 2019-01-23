using UnityEngine;

//
// ShipEnterance.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Usable object to enter ship
//

public class ShipEnterance : MonoBehaviour, IUsable {

    Ship ship;

    public string GetName() {
        return "Space Ship";
    }

    public void Start() {
        ship = GetComponentInParent<Ship>();
        if (!ship) Debug.LogError("ShipEnterance: No ship in parent");
    }

    public void Use() {
        ship.Use();
    }
}

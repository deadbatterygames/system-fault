using UnityEngine;

//
// InterestPoint.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Spawns blueprints and energy
//

public class InterestPoint : MonoBehaviour {
    public void Start() {
        Destroy(transform.GetChild(0).gameObject);
    }
}

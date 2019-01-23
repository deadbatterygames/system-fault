using UnityEngine;

//
// Bush.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Removes colliders used for bush density
//

public class Bush : MonoBehaviour {
	void Awake () {
        Destroy(GetComponent<SphereCollider>());
	}
}

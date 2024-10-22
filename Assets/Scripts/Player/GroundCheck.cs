﻿using UnityEngine;

//
// GroundCheck.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Checks whether the parent object is grounded
//

public class GroundCheck : MonoBehaviour {

    [SerializeField] Rigidbody characterRigidbody;
    IGroundable groundable;
    int colliders;

	void Awake () {
        groundable = GetComponentInParent<IGroundable>();
        if (groundable == null) {
            Debug.LogError("Ground Check: No IGroundable set as parent");
        }
	}
	
    void OnTriggerStay(Collider other) {
        if (!other.isTrigger && characterRigidbody.drag == 0f) {
            groundable.SetGrounded(true);
        }
    }

    void OnTriggerExit(Collider other) {
        groundable.SetGrounded(false);
    }
}

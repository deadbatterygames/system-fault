using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Missile : MonoBehaviour, IMaterializeable {

    public void Dematerialize(Material demat) {
        GetComponentInParent<IMaterializeable>().Dematerialize(demat);
    }

    public bool IsColliding() {
        throw new System.NotImplementedException();
    }

    public void Materialize() {
        GetComponentInParent<IMaterializeable>().Materialize();

    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

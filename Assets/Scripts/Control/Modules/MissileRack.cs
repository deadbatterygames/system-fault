using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// MissileRack.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Fires heat-seeking missiles at target
//

public class MissileRack : ShipModule, IWeapon {
   
    protected override void Awake() {
        base.Awake();
        moduleType = GameTypes.ModuleType.MissileRack;
    }

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {

    }

    public void Fire() {
        throw new System.NotImplementedException();
    }
}

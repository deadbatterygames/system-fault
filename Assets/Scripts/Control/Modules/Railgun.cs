using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// Railgun.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Ship weapon; fires accurately and fast, high damage
//

public class Railgun : ShipModule, IWeapon {

    protected override void Awake() {
        base.Awake();
        moduleType = GameTypes.ModuleType.Railgun;
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

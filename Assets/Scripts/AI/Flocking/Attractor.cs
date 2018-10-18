using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor {

	public GameObject go;
	public float radius;
	public Attractor(GameObject go, float radius){
		this.go = go;
		this.radius = radius;
	}

}

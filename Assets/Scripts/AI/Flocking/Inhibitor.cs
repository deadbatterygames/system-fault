using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inhibitor {

	public enum Type {
		None,
		Water
	}
	public GameObject go;
	public float radius;
	public Type type;
	public Inhibitor(GameObject go, float radius, Type type = Type.None){
		this.go = go;
		this.radius = radius;
		this.type = type; 
	}
	public Vector3 GetPosition(){
		return go.transform.position;
	}
}

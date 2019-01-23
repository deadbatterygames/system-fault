using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Separator {

	public enum Type {
		None,
		LandingBarrier,
		Obstacle
	}
	public GameObject go;
	public float radius;
	public Type type;
	public Separator(GameObject go, float radius, Type type = Type.None){
		this.go = go;
		this.radius = radius;
		this.type = type; 
	}
	public Vector3 GetPosition(){
		return go.transform.position;
	}
}

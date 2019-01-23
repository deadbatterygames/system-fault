using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Attractor {

	public enum Type
	{
		None,
		PlayerShip,
		Destination,
		LandingPoint,
		LandingPad
	}
	public GameObject go;
	public float radius;
	public Attractor.Type type;
	public GameObject landingPad = null;
	public GameObject landingBody = null;
	const float landingPadOffset = 10.0f;
	public Attractor(GameObject go, float radius, Attractor.Type type = Type.None){
		this.go = go;
		this.radius = radius;
		this.type = type;
	}
	public Attractor(GameObject go, float radius, GameObject landingPad, GameObject landingBody){
		this.go = go;
		this.radius = radius;
		this.type = Attractor.Type.LandingPad;
		this.landingPad = landingPad;
		this.landingBody = landingBody;
	}
	public Vector3 GetPosition(){

		switch((int) type){
			case (int) Attractor.Type.PlayerShip:
				return go.transform.position - (go.transform.forward * GameManager.instance.ship.GetSpeed());
			case (int) Attractor.Type.LandingPad:
				return go.transform.position + (landingPad.transform.position - landingBody.transform.position).normalized * landingPadOffset;
			default:
				return go.transform.position; 
		}
	}
}

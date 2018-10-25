using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//
// Xrom.cs
//
// Author: Gabriel Cimolino (Dead Battery Games)
// Purpose: Making shit go zooooooooooooom
//

public class Xrom : Boid {

	float rotationSpeed = 5;
	bool landing = false;
	bool landed = false;
	bool landingPermission = false;

	void Update(){
		if(landing){
			List<Attractor> landingPads = attractors.Where(x => x.type == Attractor.Type.LandingPad).ToList();

			foreach(Attractor landingPad in landingPads){
				Vector3 differenceVector = landingPad.GetPosition() - transform.position;
				if(differenceVector.magnitude < 0.5f){
					landing = false;
					landed = true;
				}
			}
		}
	}

	public override void Move(Vector3 heading, bool debug){
		rb.velocity += heading.normalized;
		if(rb.velocity.magnitude > flock.Speed(this)) rb.velocity = rb.velocity.normalized * flock.Speed(this);
		if(debug) Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.white);

		Quaternion targetRotation = Quaternion.FromToRotation(transform.forward, rb.velocity) * transform.rotation;
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
	}

	public override void Rotate(Vector3 rotation){
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotation), Time.fixedDeltaTime);
		// transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(Vector3.down), Time.fixedDeltaTime);
	}

	public void OnTriggerEnter(Collider col){
		//Debug.Log("Xrom collided with " + col.gameObject.name);
		// if(col.gameObject.name == "LandingPad" && landing){
		// 	Debug.Log("Xrom is landing");

		// 	landing = false;
		// 	land = true;

		// 	Vector3 differenceVector = landingPad.transform.position - transform.position;

		// 	landingOrigin = transform.position + differenceVector / 2;
		// 	landingVector = transform.position - landingOrigin;
		// 	landingPlane = Linear.CrossProduct(rb.velocity.normalized, differenceVector.normalized);
		// 	landingRadius = differenceVector.magnitude / 2;
		// 	landingTime = 0.0f;

		// 	rb.velocity = Vector3.zero;
		// }
	}

	public void SetLandingPad(LandingPad lp){
		landing = true;
		AddAttractor(lp.GetLandingPad());
	}
	public bool Landing(){
		return landing;
	}
	public bool HasLanded(){
		return landed;
	}
	public void SetLandingPermission(bool value){
		landingPermission = value;
	}
	public bool HasLandingPermission(){
		return landingPermission;
	}
	public bool RequestLanding(){
		return landing;
	}
}

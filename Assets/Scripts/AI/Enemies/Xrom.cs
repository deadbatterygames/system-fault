using System.Collections.Generic;
using UnityEngine;

//
// Xrom.cs
//
// Author: Gabriel Cimolino (Dead Battery Games)
// Purpose: Making shit go zooooooooooooom
//

public class Xrom : MonoBehaviour, IFlocker {

	float maxVelocity = 50;
	float rotationSpeed = 5;
	[SerializeField] Rigidbody rb;
	List<Attractor> attractors;
	bool landing = false;
	GameObject landingBody = null;
	GameObject landingPad = null;
	void Awake(){
		rb = transform.GetComponent<Rigidbody>();
		attractors = new List<Attractor>();
	}

	public void Move(Vector3 heading, bool debug){
		rb.velocity += heading.normalized;
		if(rb.velocity.magnitude > maxVelocity) rb.velocity = rb.velocity.normalized * maxVelocity;
		if(debug) Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.white);

		Quaternion targetRotation = Quaternion.FromToRotation(transform.forward, rb.velocity) * transform.rotation;
		transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
	}

	public void Rotate(Vector3 rotation){
		transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotation), Time.fixedDeltaTime);
		// transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(Vector3.down), Time.fixedDeltaTime);
	}

	public Vector3 GetPosition(){return transform.position;}
	public Vector3 GetRotation(){return transform.rotation.eulerAngles;}
	public Vector3 GetVelocity(){return rb.velocity;}
	public void AddAttractor(Attractor attractor){
		attractors.Add(attractor);
	}
	public void Land(GameObject landingPad, GameObject landingBody){
		this.landing = true;
		AddAttractor(new Attractor(landingPad, float.PositiveInfinity));
		this.landingBody = landingBody;
		this.landingPad = landingPad;
	}
	public bool GetLanding(){
		return this.landing;
	}
	public GameObject GetLandingBody(){
		return landing ? landingBody : null;
	}
	public Vector3 GetLandingPosition(){
		return landingPad != null ? landingPad.transform.position : Vector3.zero;
	}
	public List<Attractor> GetAttractors(){return attractors;}
}

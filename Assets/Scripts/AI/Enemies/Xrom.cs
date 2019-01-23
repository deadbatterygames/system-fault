using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//
// Xrom.cs
//
// Author: Gabriel Cimolino (Dead Battery Games)
// Purpose: Making shit go zooooooooooooom
//

public class Xrom : Boid, IDamageable {

	[SerializeField] bool explode = false;
	float health = 100f;
	float rotationSpeed = 5;
	bool landing = false;
	bool landed = false;
	bool landingPermission = false;
	[SerializeField] XromWalker walker;
	private Vector3 oldVelocity;
	private Vector3 flockingVelocity;
	[SerializeField] bool dummy;

	void Start(){
		
	}

	public void InitializeXrom(bool grounded){
		this.grounded = grounded;

		oldVelocity = Vector3.zero;
		this.perceptiveDistance = (grounded) ? 10f : 100f;

		GameManager.instance.AddGravityBody(GetComponent<Rigidbody>());
	}

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

		if(explode && !dummy) walker.DestroyHeatSink();
	}

	void OnDrawGizmos(){
		//Gizmos.color = Color.cyan;
        //Gizmos.DrawWireSphere(GetPosition(), PerceptiveDistance());
	}

	public void Damage(float damage, GameTypes.DamageType damageType, Vector3 force){
		health -= damage;
		Move(force, false);

		if(health <= 0){
			if(!grounded){
				DestroyXrom();
			}
		}
	}

	public override void Move(Vector3 heading, bool debug){
		
		if(grounded){
			//flockingVelocity += heading;
			//flockingVelocity = Vector3.ClampMagnitude(flockingVelocity, flock.Speed(this));
			//oldVelocity = rb.velocity;

			//rb.velocity += heading;
			// Vector3 projHeading = Vector3.ProjectOnPlane(heading, transform.up);
			// projHeading = projHeading.normalized * heading.magnitude;


			rb.AddForce(heading * 100);

			//rb.velocity += flockingVelocity + (rb.velocity - oldVelocity);
			//heading = heading.normalized * Mathf.Log(heading.magnitude);
			//if(rb.velocity.magnitude > flock.Speed(this)) rb.AddForce(heading, ForceMode.VelocityChange);

			//if(rb.velocity.magnitude > flock.Speed(this)) rb.velocity = rb.velocity.normalized * flock.Speed(this);
			rb.velocity = Vector3.ClampMagnitude(rb.velocity, flock.Speed(this));

			//Project old heading on the Xrom's axis to determine extension in each component vector
			//float oldTheta = Mathf.Atan2(Vector3.Dot(oldVelocity, walker.transform.right), Vector3.Dot(oldVelocity, walker.transform.forward));
			float theta = 0.0f;
			if(!dummy) theta = Mathf.Atan2(Vector3.Dot(rb.velocity, walker.transform.right), Vector3.Dot(rb.velocity, walker.transform.forward));

			//theta -= oldTheta;

			//TODO: ROTATION
			//if(rb.velocity.magnitude > 0.5f && !dummy) walker.Rotate(theta);

			oldVelocity = rb.velocity;
		}
		else{
			//if (debug) Debug.Log("Xrom::Move ~ Adding " + heading.ToString() + " to velocity");
			Vector3 oldVelocity = rb.velocity;
			rb.velocity += heading;//.normalized;
			if(rb.velocity.magnitude > flock.Speed(this)) rb.velocity = rb.velocity.normalized * flock.Speed(this);
			Quaternion targetRotation = Quaternion.FromToRotation(transform.forward, rb.velocity) * transform.rotation;
			transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * rotationSpeed);
		}

		if(debug) Debug.DrawLine(transform.position, transform.position + rb.velocity, Color.white);
	}

	public override void Rotate(Vector3 rotation){
		rb.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotation), Time.fixedDeltaTime);
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
	public void DestroyXrom(){
		RemoveFromFlock();
		Destroy(gameObject);
	}
}

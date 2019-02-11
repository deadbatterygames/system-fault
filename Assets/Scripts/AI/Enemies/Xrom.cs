using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//
// Xrom.cs
//
// Author: Gabriel Cimolino (Dead Battery Games)
// Purpose: Making shit go zooooooooooooom
//

public class Xrom : Boid, IDamageable, IGroundable {

	Animator animator;
	[SerializeField] XromGun gun;
	[SerializeField] List<GameObject> targets;
	[SerializeField] GameObject legs;
	[SerializeField] GameObject torso;
	//[SerializeField] bool explode = false;
	float health = 100f;
	float rotationSpeed = 5f;
	float fireCooldown = 0.5f;
	float fireTimer = 0.0f;
	bool landing = false;
	bool landed = false;
	bool landingPermission = false;
	private Vector3 flockingVelocity;
	private float knockbackScale = 10f;
	private float maxForce = 0.5f;
	[SerializeField] bool dummy;

	void Start(){
		
	}

	public void InitializeXrom(bool grounded){
		animator = GetComponentInChildren<Animator>();
		this.flying = !grounded;

		this.targets = new List<GameObject>();

		this.perceptiveDistance = (grounded) ? 15f : 100f;

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

		fireTimer -= Time.deltaTime;
		//Debug.Log("Update velocity: " + rb.velocity.magnitude);
	}

	void OnDrawGizmos(){
		//Gizmos.color = Color.cyan;
        //Gizmos.DrawWireSphere(GetPosition(), PerceptiveDistance());
	}

	public void MakeTarget(GameObject target){
		if(!this.targets.Contains(target)) this.targets.Add(target);
	}

	public void RemoveTarget(GameObject target){
		this.targets.RemoveAll(x => x == target);
	}

	public void Damage(float damage, GameTypes.DamageType damageType, Vector3 force, Vector3 pointOfImpact, Vector3 directionOfImpact){
		Debug.Log("Xrom::Damage ~ Dealing " + damage.ToString() + " points of " + damageType + " damage to xrom");
		health -= damage;
		rb.AddForce(knockbackScale * force, ForceMode.Impulse);

		if(health <= 0){
			DestroyXrom();
		}
	}

	public override void Move(Vector3 heading, bool debug){
		
		if(!flying){
            if (grounded) {
                Vector3 input = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(heading, transform.up) * Overmind.instance.movementScale, 1f);
                Vector3 dv = Vector3.ClampMagnitude(input * flock.Speed(this) - rb.velocity, maxForce * flock.Speed(this));

                if (input.sqrMagnitude > 0.5f && grounded) rb.AddForce(dv, ForceMode.VelocityChange);

                animator.SetBool("IsMoving", rb.velocity.sqrMagnitude > 0.2f);

                if (rb.velocity.sqrMagnitude > 0.2f) {
                    RotateLegs(rb.velocity.normalized);
                    animator.speed = rb.velocity.magnitude * 0.2f;
                } else {
                    animator.speed = 2f;
                    rb.velocity = Vector3.zero;
                }
            }
			else animator.SetBool("IsMoving", false);

			if(targets.Count > 0){
				GameObject currentTarget = targets.OrderBy(x => (x.transform.position - transform.position).sqrMagnitude).ToArray()[0];
				Vector3 dif = currentTarget.transform.position - gun.transform.position;//transform.position;

				RotateTorso(dif);
				if(fireTimer < 0 && !((XromFlock) flock).peaceful) FireAtTarget(dif);
			}
			else RotateTorso(transform.forward);
		} else {
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
		//rb.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(rotation), Time.fixedDeltaTime);
		// transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(Vector3.down), Time.fixedDeltaTime);
	}

	private void RotateLegs(Vector3 heading){
		//Vector3 newRotation = Vector3.RotateTowards(transform.forward, heading, Time.deltaTime * rotationSpeed, 0.0f);

        Vector3 newForward = Vector3.Slerp(transform.forward, Vector3.ProjectOnPlane(heading, transform.up), rotationSpeed * Time.fixedDeltaTime);
		Vector3 oldForward = torso.transform.forward;
		Vector3 oldUp = torso.transform.up;

		//rb.MoveRotation(Quaternion.LookRotation(newForward, transform.up));
		transform.rotation = Quaternion.LookRotation(newForward, transform.up);
		torso.transform.rotation = Quaternion.LookRotation(oldForward, oldUp);
	}

	private void RotateTorso(Vector3 heading){
		Vector3 newRotation = Vector3.Slerp(gun.transform.forward, Vector3.ProjectOnPlane(heading, transform.up), rotationSpeed * Time.fixedDeltaTime);

		torso.transform.rotation = Quaternion.LookRotation(newRotation, transform.up);
    }

	private void FireAtTarget(Vector3 toTarget){
		if(Vector3.Angle(torso.transform.forward, toTarget) < 1){
			gun.Fire();
			fireTimer = fireCooldown;
		}
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
		Debug.Log("Xrom::DestroyXrom ~ Destroying xrom");
		GameManager.instance.RemoveGravityBody(rb);
		RemoveFromFlock();
		Destroy(gameObject);
	}
}

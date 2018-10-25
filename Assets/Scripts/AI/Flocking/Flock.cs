using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour {

	[SerializeField] protected float maxSpeed;
	[SerializeField] protected float boundingRadius = 0.0f;
	protected List<Boid> boids;
	protected List<Attractor> attractors;
	protected List<Separator> separators;
	[SerializeField] protected bool debug = true;
	[SerializeField] protected bool debugHeading = true;
	[SerializeField] protected bool debugCohesion = true;
	[SerializeField] protected bool debugSeparation = true;
	[SerializeField] protected bool debugAlignment = true;
	[SerializeField] protected bool debugAttraction = true;
	[SerializeField] protected bool debugBounding = true;
	[SerializeField] protected bool cohesion = true;
	[SerializeField] protected bool alignment = true;
	[SerializeField] protected bool separation = true;

	protected void Awake(){
		maxSpeed = 100.0f;
		
		this.boids = new List<Boid>();
		this.attractors = new List<Attractor>();
		this.separators = new List<Separator>();

		debug = true;
		debugHeading = true;
		debugCohesion = true;
		debugSeparation = true;
		debugAlignment = true;
		debugAttraction = true;
		debugBounding = true;
		cohesion = true;
		alignment = true;
		separation = true;
	}
	public virtual void Update(){
		Vector3[] headings = FlockingManager.Headings(boids.ToArray(), attractors.ToArray(), separators.ToArray(), transform.position, boundingRadius, 4, debug, debugHeading, debugCohesion, debugSeparation, debugAlignment, debugAttraction, debugBounding, cohesion, alignment, separation);
	
		for(int i = 0; i < boids.Count && i < headings.Length; i++){
			boids[i].Move(headings[i], debug && debugHeading);
		}

		Vector3 avgRotation = FlockingManager.Rotation(boids.ToArray());

		foreach(Boid boid in boids){
			boid.Rotate(avgRotation);
		}
	}
	void AddBoid(Boid boid){
		boid.SetFlock(this);
		boids.Add(boid);
	}
	void RemoveBoid(Boid boid){
		boids.Remove(boid);
	}
	void TransferBoid(Boid boid, Flock flock){
		Debug.Log("Transferring boid to " + flock.name);
		RemoveBoid(boid);
		flock.AddBoid(boid);
	}
	public virtual float Speed(Boid boid){
		return maxSpeed;
	}
	public int FlockSize(){
		return boids.Count;
	}
	public void OnTriggerEnter(Collider col){
		Boid boid = col.GetComponent<Boid>();
		if(boid != null){
			Debug.Log("A boid collided with flock " + gameObject.name);
			BoidTrigger(boid);
		}
	}
	protected virtual void BoidTrigger(Boid boid){
		Flock flock = boid.GetFlock();

		if(flock != null){
			flock.TransferBoid(boid, this);
		}
	}
}

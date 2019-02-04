using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flock : MonoBehaviour {

	[SerializeField] protected SphereCollider collider;
	[SerializeField] protected bool flockTogether;
	[SerializeField] protected bool groundedFlock;
	[SerializeField] protected bool flyingFlock;
	[SerializeField] protected float maxSpeed;
	[SerializeField] protected float boundingRadius = 0.0f;
	[SerializeField] protected List<Boid> groundedBoids;
	[SerializeField] protected List<Boid> boids;
	protected List<Attractor> attractors;
	protected List<Separator> separators;
	protected List<Inhibitor> inhibitors;
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
	private Vector3[] groundedHeadings;
	private bool groundedHeadingsApplied;
	private Vector3[] flyingHeadings;
	private bool flyingHeadingsApplied;

	protected void InitializeFlock(){
		//maxSpeed = 100.0f;
		
		this.groundedBoids = new List<Boid>();
		this.boids = new List<Boid>();
		this.attractors = new List<Attractor>();
		this.separators = new List<Separator>();
		this.inhibitors = new List<Inhibitor>();

		// debug = true;
		// debugHeading = true;
		// debugCohesion = true;
		// debugSeparation = true;
		// debugAlignment = true;
		// debugAttraction = true;
		// debugBounding = true;
		cohesion = true;
		alignment = true;
		separation = true;

		groundedHeadingsApplied = true;
		flyingHeadingsApplied = true;

		StartCoroutine("UpdateFlock");
	}
	public virtual void FixedUpdate(){
		if(flyingFlock){
			//Debug.Log("Flock::Update ~ Updating flying flock");
			Vector3[] headings = flyingHeadings;//FlockingManager.Headings(boids.ToArray(), attractors.ToArray(), separators.ToArray(), transform.position, boundingRadius, 4, debug, debugHeading, debugCohesion, debugSeparation, debugAlignment, debugAttraction, debugBounding, cohesion, alignment, separation);
		
			for(int i = 0; i < boids.Count && i < headings.Length; i++){
				boids[i].Move(headings[i], debug && debugHeading);
				boids[i].Rotate(headings[i]);
			}

			// Vector3 avgRotation = FlockingManager.Rotation(boids.ToArray());

			// foreach(Boid boid in boids){
				
			// }

			flyingHeadingsApplied = true;
		}
		if(groundedFlock){
			Vector3[] headings = groundedHeadings;//FlockingManager.Headings(groundedBoids.ToArray(), attractors.ToArray(), separators.ToArray(), transform.position, boundingRadius, 4, debug, debugHeading, debugCohesion, debugSeparation, debugAlignment, debugAttraction, debugBounding, cohesion, alignment, separation);

			for(int i = 0; i < groundedBoids.Count && i < headings.Length; i++){
				groundedBoids[i].Move(headings[i], debug && debugHeading);
			}

			groundedHeadingsApplied = true;
		}
	}

	IEnumerator UpdateFlock(){
		while(true){
			if(groundedHeadingsApplied){
				//Debug.Log("Updating headings");
				groundedHeadings = FlockingManager.Headings(groundedBoids.ToArray(), attractors.ToArray(), separators.ToArray(), inhibitors.ToArray(), transform.position, boundingRadius, 4, debug, debugHeading, debugCohesion, debugSeparation, debugAlignment, debugAttraction, debugBounding, cohesion, alignment, separation);
				groundedHeadingsApplied = false;
			}
			if(flyingHeadingsApplied){
				flyingHeadings = FlockingManager.Headings(boids.ToArray(), attractors.ToArray(), separators.ToArray(), inhibitors.ToArray(), transform.position, boundingRadius, 4, debug, debugHeading, debugCohesion, debugSeparation, debugAlignment, debugAttraction, debugBounding, cohesion, alignment, separation);
				flyingHeadingsApplied = false;
			}
			yield return null;
		}
	}

	protected void AddBoid(Boid boid){
		boid.SetFlock(this);
		if(boid.grounded) groundedBoids.Add(boid);
		else boids.Add(boid);
	}
	public void RemoveBoid(Boid boid){
		groundedBoids.Remove(boid);
		boids.Remove(boid);
	}
	void TransferBoid(Boid boid, Flock flock){
		//Debug.Log("Transferring boid to " + flock.name);
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
		//Debug.Log("Flock collided with collider " + col.name);

		Boid boid = col.GetComponent<Boid>();
		if(boid != null){
			//Debug.Log("A boid collided with flock " + gameObject.name);
			//BoidTrigger(boid);
		}

		FlockingObstacle obstacle = col.GetComponent<FlockingObstacle>();
		if(obstacle != null){
			//Debug.Log("Flocking obstacle " + col.gameObject.name + " collided with flock " + gameObject.name);
			ObstacleTriggerEnter(obstacle);
		}
	}
	public void OnTriggerExit(Collider col){
		FlockingObstacle obstacle = col.GetComponent<FlockingObstacle>();
		if(obstacle != null){
			//Debug.Log("Flocking obstacle " + col.gameObject.name + " exited flock " + gameObject.name);
			ObstacleTriggerExit(obstacle);
		}
	}
	protected virtual void BoidTrigger(Boid boid){
		Flock flock = boid.GetFlock();

		if(flock != null && flock != this){
			if((boid.grounded && this.groundedFlock) || (!boid.grounded && this.flyingFlock)) flock.TransferBoid(boid, this);
		}
	}
	protected virtual void ObstacleTriggerEnter(FlockingObstacle obstacle){
		//Debug.Log("Adding obstacle " + obstacle.transform.parent.gameObject.name + " to separators");
		if((groundedFlock && obstacle.grounded) || (!groundedFlock && !obstacle.grounded)){
			if(obstacle.hasSeparator) AddSeparator(obstacle.separator);
			if(obstacle.hasAttractor) AddAttractor(obstacle.attractor);
			if(obstacle.hasInhibitor) AddInhibitor(obstacle.inhibitor);
		}
	}
	protected virtual void ObstacleTriggerExit(FlockingObstacle obstacle){
		RemoveSeparator(obstacle.separator);
		RemoveAttractor(obstacle.attractor);
		RemoveInhibitor(obstacle.inhibitor);
	}

	public virtual void AddAttractor(Attractor attractor){
        switch((int)attractor.type) {
            case (int)Attractor.Type.PlayerShip:
                if (flyingFlock) this.attractors.Add(attractor);
                break;
            default:
                this.attractors.Add(attractor);
                break;
        }
	}
	public virtual void AddSeparator(Separator separator){
		this.separators.Add(separator);
	}
	public virtual void AddInhibitor(Inhibitor inhibitor){
		this.inhibitors.Add(inhibitor);
	}

	public virtual void RemoveAttractor(Attractor attractor){
		this.attractors.Remove(attractor);
	}

	public virtual void RemoveAttractor(GameObject attractor, Attractor.Type type = Attractor.Type.None){
		if(type == Attractor.Type.None) this.attractors.RemoveAll(x => x.go == attractor);
		else this.attractors.RemoveAll(x => x.go == attractor && x.type == type);
	}

	public virtual void RemoveSeparator(Separator separator){
		this.separators.Remove(separator);
	}

	public virtual void RemoveSeparator(GameObject separator, Separator.Type type = Separator.Type.None){
		if(type == Separator.Type.None) this.separators.RemoveAll(x => x.go == separator);
		else this.separators.RemoveAll(x => x.go == separator && x.type == type);
	}

	public virtual void RemoveInhibitor(Inhibitor inhibitor){
		this.inhibitors.Remove(inhibitor);
	}
}

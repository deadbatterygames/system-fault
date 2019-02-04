using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class XromFlock : Flock {

	[SerializeField] private GameObject xromPrefab;
	[SerializeField] private GameObject xromDummyPrefab;
	[SerializeField] private GameObject xromShipPrefab;
	public List<Xrom> startingGroundedBoids;

	void Start(){
		//maxSpeed = 10f;
		// Xrom[] xroms = FindObjectsOfType<Xrom>();

		// foreach(Xrom xrom in xroms){
		// 	if(xrom.gameObject.activeSelf){
		// 		if(groundedFlock && xrom.grounded) AddBoid(xrom);
		// 		if(flyingFlock && !xrom.grounded) AddBoid(xrom);
		// 	}
		// }

		// foreach(Xrom xrom in startingGroundedBoids){
		// 	if(groundedFlock && xrom.grounded) AddBoid(xrom);
		// }

		//separators.Add(new Separator(FindObjectOfType<Player>().gameObject, 10f));
		//attractors.Add(new Attractor(destination, 100f, Attractor.Type.Destination));
	}

	public void InitializeFlock(bool grounded, bool flying){
		base.InitializeFlock();
		groundedFlock = grounded;
		flyingFlock = flying;
	}

	public void CreateXrom(bool grounded, bool dummy){
		GameObject xromGO;
		if(grounded){
			if(dummy) xromGO = Instantiate(xromDummyPrefab, transform.position, Quaternion.identity);
			else xromGO = Instantiate(xromPrefab, transform.position, Quaternion.identity);
		}
		else xromGO = Instantiate(xromShipPrefab, transform.position, Quaternion.identity);
		
		Xrom xrom = xromGO.GetComponent<Xrom>();
		xrom.InitializeXrom(grounded);

		AddBoid(xrom);
	}

	public override void FixedUpdate(){
		base.FixedUpdate();
		
		Vector3 centroid = Vector3.zero;
		float extents = 0f;
		
		foreach(Boid boid in groundedBoids){
			Vector3 position = boid.GetPosition();
			centroid += position;
		}

		foreach(Boid boid in boids){
			Vector3 position = boid.GetPosition();
			centroid += position;
		}

		if(groundedBoids.Count + boids.Count > 0) centroid = centroid / (groundedBoids.Count + boids.Count);

		transform.position = centroid;

		foreach(Boid boid in groundedBoids){
			Vector3 position = boid.GetPosition();
			Vector3 differenceVector = transform.position - position;
			if(differenceVector.magnitude > extents) extents = differenceVector.magnitude;
		}

		foreach(Boid boid in boids){
			Vector3 position = boid.GetPosition();
			Vector3 differenceVector = transform.position - position;
			if(differenceVector.magnitude > extents) extents = differenceVector.magnitude;
		}

		boundingRadius = Mathf.Max(extents / 2.0f, 10f);
		collider.radius = Mathf.Max(extents, 10f);
	}

	private void MakeTarget(GameObject target){
		Debug.Log("XromFLock::MakeTarget ~ Making " + target.name.ToString() + " a xrom target for flock " + gameObject.name.ToString());
		foreach(Xrom xrom in groundedBoids){
			xrom.MakeTarget(target);
		}
		foreach(Xrom xrom in boids){
			xrom.MakeTarget(target);
		}
	}

	private void RemoveTarget(GameObject target){
		Debug.Log("XromFLock::RemoveTarget ~ Removing xrom target " + target.name.ToString() + " for flock " + gameObject.name.ToString());
		foreach(Xrom xrom in groundedBoids){
			xrom.RemoveTarget(target);
		}
		foreach(Xrom xrom in boids){
			xrom.RemoveTarget(target);
		}
	}

	public override void AddAttractor(Attractor attractor){
		base.AddAttractor(attractor);

		if(attractor != null && attractor.type == Attractor.Type.Player){
			Debug.Log("XromFlock::AddAttractor ~ Adding player attractor to list of attractors");

			MakeTarget(attractor.go);
		}
	}

	public override void RemoveAttractor(Attractor attractor){
		base.RemoveAttractor(attractor);

		if(attractor != null && attractor.type == Attractor.Type.Player){

			RemoveTarget(attractor.go);
		}
	}

	public override void AddSeparator(Separator separator){
		base.AddSeparator(separator);

		if(separator != null && separator.type == Separator.Type.Player){
			Debug.Log("XromFlock::AddSeparator ~ Adding player separator to list of separators");

			MakeTarget(separator.go);
		}
	}

	public override void RemoveSeparator(Separator separator){
		base.RemoveSeparator(separator);

		if(separator != null && separator.type == Separator.Type.Player){

			RemoveTarget(separator.go);
		}
	}
}

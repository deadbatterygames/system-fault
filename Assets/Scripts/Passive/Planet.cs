using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : Flock {
	[SerializeField] GameObject planetMesh;
	[SerializeField] SphereCollider atmosphereCollider;
	float planetRadius = 0.0f;

	void Awake(){
		base.Awake();
		//Find the radius
		Bounds bounds = new Bounds(transform.position, Vector3.zero);
		Renderer renderer = planetMesh.GetComponent<Renderer>();
		bounds.Encapsulate(renderer.bounds);
		planetRadius = bounds.extents.magnitude / 2;

		boundingRadius = bounds.extents.magnitude;
		atmosphereCollider.radius = boundingRadius;

		attractors.Add(new Attractor(gameObject, planetRadius * 1.5f));
		separators.Add(new Separator(gameObject, planetRadius * 1.5f));
	}
	public override float Speed(Boid boid){
		return maxSpeed * (((boid.GetPosition() - transform.position).magnitude - planetRadius) / planetRadius);
	}
	public float GetRadius(){
		return planetRadius;
	}
	protected override void BoidTrigger(Boid boid){
		base.BoidTrigger(boid);
	}
}

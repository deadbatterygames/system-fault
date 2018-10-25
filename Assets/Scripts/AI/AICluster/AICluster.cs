using System.Collections.Generic;
using UnityEngine;

//
// AICluster.cs
//
// Author: Gabriel Cimolino (Dead Battery Games)
// Purpose: 
//

public class AICluster : Flock {

	[SerializeField] GameObject xromShipPrefab;
	[SerializeField] GameObject planetMesh;
	[SerializeField] LandingPad lp;
	//[SerializeField] List<Boid> xroms;
	[SerializeField] List<GameObject> asteroids;
	float planetRadius = 0.0f;
	// List<Vector3> attractorPositions;
	// List<float> attractorScales;
	// List<Vector3> separatorPositions;
	// List<float> separatorScales;

	void Start(){
		//xroms = new List<Boid>();
		// attractorPositions = new List<Vector3>();
		// attractorScales = new List<float>();
		// separatorPositions = new List<Vector3>();
		// separatorScales = new List<float>();
		
		maxSpeed = 100.0f;

		Bounds bounds = new Bounds(transform.position, Vector3.zero);

		Renderer renderer = planetMesh.GetComponent<Renderer>();

		bounds.Encapsulate(renderer.bounds);

		planetRadius = bounds.extents.magnitude / 2;

		foreach(Renderer r in GetComponentsInChildren<Renderer>()) {
			bounds.Encapsulate(r.bounds);
		}

		boundingRadius = bounds.extents.magnitude * 1.0f;

		Vector3 clusterPosition = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f));
		clusterPosition = clusterPosition * boundingRadius;

		for(int i = 0; i  < 10; i++){
			Vector3 position = new Vector3(Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f), Random.Range(0.0f, 1.0f)) * asteroids[0].transform.localScale.x;
			position = position.normalized;
			Boid boid = Instantiate(xromShipPrefab, transform.position + clusterPosition + position, Quaternion.identity).GetComponent<Boid>();
			boid.SetFlock(this);
			boids.Add(boid);
		}

		//xroms[7].Land(lp.GetComponent<LandingPad>().GetLandingPad(), lp.GetComponent<LandingPad>().GetBody());
		//xroms[8].Land(lp.GetComponent<LandingPad>().GetLandingPad(), lp.GetComponent<LandingPad>().GetBody());
		//boids[9].Land(lp.GetComponent<LandingPad>().GetLandingPad(), lp.GetComponent<LandingPad>().GetBody());

		//boids[7].AddAttractor(lp.GetLandingPad());
		//boids[8].AddAttractor(lp.GetLandingPad());
		//boids[9].AddAttractor(lp.GetLandingPad());

		((Xrom) boids[7]).SetLandingPad(lp);
		((Xrom) boids[8]).SetLandingPad(lp);
		((Xrom) boids[9]).SetLandingPad(lp);

		//attractorPositions.Add(transform.position);
		//attractorScales.Add(boundingRadius);
		// separatorPositions.Add(transform.position);
		// separatorScales.Add(planetRadius * 2);

		attractors.Add(new Attractor(gameObject, planetRadius * 2));
		separators.Add(new Separator(gameObject, planetRadius * 2));

		for(int i = 0; i < asteroids.Count; i++){
			attractors.Add(new Attractor(asteroids[i], asteroids[i].transform.localScale.x * 10));
			separators.Add(new Separator(asteroids[i], asteroids[i].transform.localScale.x * 2));
			// attractorPositions.Add(asteroids[i].transform.position);
			// attractorScales.Add(asteroids[i].transform.localScale.x * 10);
			// separatorPositions.Add(asteroids[i].transform.position);
			// separatorScales.Add(asteroids[i].transform.localScale.x * 2);
		}
	}
}

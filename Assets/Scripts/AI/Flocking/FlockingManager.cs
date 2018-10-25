using System.Collections.Generic;
using UnityEngine;

//
// FlockingManager.cs
//
// Author: Gabriel Cimolino (Dead Battery Games)
// Purpose: 
//

public static class FlockingManager {

	public static string Vector3ToString(Vector3 vector){
		return "[" + vector.x.ToString() + ", " + vector.y.ToString() + ", " + vector.z.ToString() + "]";
	}

	public static Vector3[] Headings(Boid[] boids, Attractor[] attractors, Separator[] separators, Vector3 boundingOrigin, float boundingScale, int flockSize, bool debug, bool debugHeading, bool debugCohesion, bool debugSeparation, bool debugAlignment, bool debugAttraction, bool debugBounding, bool cohesion = true, bool alignment = true, bool separation = true){
		List<Vector3> newHeadings = new List<Vector3>();

		Vector3[] cohesionVectors = new Vector3[]{};
		if(cohesion) cohesionVectors = Cohesion(boids, flockSize, debug && debugCohesion);
		Vector3[] flockSeparationVectors = new Vector3[]{};
		if(separation) flockSeparationVectors = FlockSeparation(boids, debug && debugSeparation);
		Vector3[] separationVectors = new Vector3[]{};
		separationVectors = Separation(boids, separators, debug && debugSeparation);
		Vector3[] alignmentVectors = new Vector3[]{};
		if(alignment) alignmentVectors = Alignment(boids, flockSize, debug && debugAlignment);
		Vector3[] attractionVectors = Attraction(boids, attractors, debug && debugAttraction);
		Vector3[] boundingVectors = Bounding(boids, boundingOrigin, boundingScale, debug && debugBounding);

		for(int i = 0; i < Mathf.Max(cohesionVectors.Length, flockSeparationVectors.Length, separationVectors.Length, alignmentVectors.Length, attractionVectors.Length, boundingVectors.Length); i++){
			//Debug.Log("FlockingManager::Headings ~ Creating heading from cohesion vector " + Vector3ToString(cohesionVectors[i]));
			// Vector3 vector = ((cohesionVectors.Length < i) ? cohesionVectors[i] : Vector3.zero) 
			// 				+ ((separationVectors.Length < i) ? separationVectors[i] : Vector3.zero) 
			// 				+ ((alignmentVectors.Length < i) ? alignmentVectors[i] : Vector3.zero) 
			// 				+ ((attractionVectors.Length < i) ? attractionVectors[i] : Vector3.zero) 
			// 				+ ((boundingVectors.Length < i) ? boundingVectors[i] : Vector3.zero);

			Vector3 vector = Vector3.zero;

			if(i < cohesionVectors.Length && boids[i].flocking) vector += cohesionVectors[i];
			if(i < flockSeparationVectors.Length && boids[i].flocking) vector += flockSeparationVectors[i];
			if(i < separationVectors.Length) vector += separationVectors[i];
			if(i < alignmentVectors.Length && boids[i].flocking) vector += alignmentVectors[i];
			if(i < attractionVectors.Length) vector += attractionVectors[i];
			if(i < boundingVectors.Length) vector += boundingVectors[i];

			// if(boids[i].GetLanding()){
			// 	GameObject body = boids[i].GetLandingBody();
			// 	Vector3 vectorToBody = body.transform.position - boids[i].GetPosition();
			// 	Vector3 vectorToPad = boids[i].GetLandingPosition() - boids[i].GetPosition();

			// 	if(vectorToPad.magnitude < vectorToBody.magnitude && vectorToBody.magnitude < body.GetComponentInChildren<Renderer>().bounds.extents.magnitude * 2){
			// 		//(((body.GetComponentInChildren<Renderer>().bounds.extents.magnitude * 2 - vectorToBody.magnitude) / body.GetComponentInChildren<Renderer>().bounds.extents.magnitude) / 5)
			// 		Vector3 adjustment = vectorToBody *  Avoid(Avoid(vectorToBody.magnitude / (body.GetComponentInChildren<Renderer>().bounds.extents.magnitude * 2)));
			// 		// if(debug){
			// 		// 	Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + adjustment, Color.gray);
			// 		// 	Debug.Log("Smooth descent");
			// 		// }
			// 		vector += adjustment;
			// 	}
			// }

			newHeadings.Add(vector);
		}

		return newHeadings.ToArray();
	}

	public static Vector3 Rotation(Boid[] boids){
		Vector3 avg = Vector3.zero;

		foreach(Boid boid in boids){
			avg += boid.GetRotation();
		}

		avg = avg / boids.Length;

		return avg;
	}

	private static Vector3[] Cohesion(Boid[] boids, int flockSize, bool debug){

		List<Vector3> cohesionVectors = new List<Vector3>();

		for(int i = 0; i < boids.Length; i++){
			Vector3 vector = Vector3.zero;
			int neighbours = 0;

			//Debug.Log("FlockingManager::Cohesion ~ Calculating cohesion vector of object at position " + Vector3ToString(positions[i]));

			for(int j = 0; j < boids.Length; j++){
				if(i != j && boids[j].flocking){
					Vector3 differenceVector = boids[j].GetPosition() - boids[i].GetPosition();

					if(Mathf.Abs(differenceVector.magnitude) < boids[i].PerceptiveDistance()){
						neighbours++;

						//vector += gorbons[j].transform.position;
						vector += boids[j].GetPosition();//differenceVector;
					}
				}
			}

			//vector = (vector / neighbours) - gorbons[i].transform.position;
			//cohesionVectors.Add(vector.normalized);

			if(neighbours > 0) {
				vector = (vector / neighbours) - boids[i].GetPosition();
				vector = vector * Happiness(neighbours, flockSize);
				if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + vector, Color.green);
			}

			cohesionVectors.Add(vector);
			//Debug.Log("FlockingManager::Cohesion ~ Calculated cohesion vector of " + Vector3ToString(cohesionVectors[i]));
		}

		return cohesionVectors.ToArray();
	}

	private static float Avoid(float x){
		return (1 / (Mathf.Sqrt(0.2f * Mathf.PI))) * Mathf.Exp(-Mathf.Pow(x, 2) / 0.2f);
	}

	private static Vector3[] FlockSeparation(Boid[] boids, bool debug){
		List<Vector3> separationVectors = new List<Vector3>();

		for(int i = 0; i < boids.Length; i++){
			Vector3 vector = Vector3.zero;

			for(int j = 0; j < boids.Length; j++){
				if(i != j && boids[j].flocking){
					Vector3 differenceVector = boids[j].GetPosition() - boids[i].GetPosition();

					if(Mathf.Abs(differenceVector.magnitude) < boids[i].PerceptiveDistance()){

						vector -= differenceVector;
					}
				}
			}

			if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + vector, Color.red);
			separationVectors.Add(vector);
		}

		return separationVectors.ToArray();
	}
	private static Vector3[] Separation(Boid[] boids, Separator[] separators, bool debug){
		List<Vector3> separationVectors = new List<Vector3>();

		for(int i = 0; i < boids.Length; i++){
			Vector3 vector = Vector3.zero;

			for(int j = 0; j < separators.Length; j++){
				Vector3 differenceVector = separators[j].GetPosition() - boids[i].GetPosition();

				if(debug) Debug.DrawLine(separators[j].GetPosition(), separators[j].GetPosition() + Vector3.down * separators[j].radius, Color.red);

				if(Mathf.Abs(differenceVector.magnitude) < separators[j].radius){

					//Debug.Log("FlockingManager::Separation ~ A flock member has entered the influence of a separator");

					Vector3 avoidanceVector = differenceVector * Separate(differenceVector.magnitude, separators[j].radius);//Avoid(Mathf.Abs(differenceVector.magnitude / separators[j].radius));//Mathf.Pow(1 + (separatorScales[j] - Mathf.Abs(differenceVector.magnitude) / separatorScales[j]), 2);
					if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() - avoidanceVector, Color.magenta);
					vector -= avoidanceVector;
				}
			}

			List<Separator> localSeparators = boids[i].GetSeparators();
			for(int j = 0; j < localSeparators.Count; j++){
				Vector3 differenceVector = localSeparators[j].GetPosition() - boids[i].GetPosition();

				if(debug) Debug.DrawLine(localSeparators[j].GetPosition(), localSeparators[j].GetPosition() + Vector3.down * localSeparators[j].radius, Color.red);

				if(Mathf.Abs(differenceVector.magnitude) < localSeparators[j].radius){
					Vector3 avoidanceVector = differenceVector * Separate(differenceVector.magnitude, localSeparators[j].radius);
					if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() - avoidanceVector, Color.magenta);
					vector -= avoidanceVector;
				}
			}

			if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + vector, Color.red);
			separationVectors.Add(vector);
		}

		return separationVectors.ToArray();
	}

	private static Vector3[] Alignment(Boid[] boids, int flockSize, bool debug){
		List<Vector3> alignmentVectors = new List<Vector3>();

		for(int i = 0; i < boids.Length; i++){
			Vector3 vector = Vector3.zero;
			int neighbours = 0;

			for(int j = 0; j < boids.Length; j++){
				if(i != j && boids[j].flocking){
					Vector3 differenceVector = boids[j].GetPosition() - boids[i].GetPosition();

					if(Mathf.Abs(differenceVector.magnitude) < boids[i].PerceptiveDistance()){
						neighbours++;

						vector += boids[j].GetVelocity();
					}
				}
			}

			//if(neighbours > 0) vector = vector / neighbours;
			if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + vector.normalized * boids[i].PerceptiveDistance(), Color.blue);
			//Debug.Log("FlockingManager::Alignment ~ Adding aligment vector " + Vector3ToString(vector));
			alignmentVectors.Add(vector);
		}

		return alignmentVectors.ToArray();
	}

	private static float Attract(float distance, float radius){
		return (radius - distance) / radius;
	}

	private static float Separate(float distance, float radius){
		return Attract(distance, radius) + 1 - (distance / radius);//2 - distance / radius;
	}

	private static Vector3[] Attraction(Boid[] boids, Attractor[] attractors, bool debug){
		List<Vector3> attractionVectors = new List<Vector3>();

		for(int i = 0; i < boids.Length; i++){
			Vector3 vector = Vector3.zero;
			List<Attractor> localAttractors = boids[i].GetAttractors();

			for(int j = 0; j < attractors.Length; j++){
				Vector3 differenceVector = attractors[j].GetPosition() - boids[i].GetPosition();

				if(debug) Debug.DrawLine(attractors[j].GetPosition(), attractors[j].GetPosition() + Vector3.back * attractors[j].radius, Color.green);

				if(Mathf.Abs(differenceVector.magnitude) < attractors[j].radius){

					Vector3 attractionVector = differenceVector * Attract(differenceVector.magnitude, attractors[j].radius);
					if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + attractionVector, Color.green);
					vector += attractionVector;
				}
			}

			for(int j = 0; j < localAttractors.Count; j++){
				Vector3 differenceVector = localAttractors[j].GetPosition() - boids[i].GetPosition();

				if(debug) Debug.DrawLine(localAttractors[j].GetPosition(), localAttractors[j].GetPosition() + Vector3.back * localAttractors[j].radius, Color.green);

				if(Mathf.Abs(differenceVector.magnitude) < localAttractors[j].radius){

					Vector3 attractionVector = differenceVector / Mathf.Log(differenceVector.magnitude);//2 * differenceVector;//differenceVector;//.normalized * Mathf.Log(attractors[j].radius); //differenceVector * (attractors[j].radius - Mathf.Abs(differenceVector.magnitude)) / attractors[j].radius;
					if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + attractionVector, Color.cyan);
					vector += attractionVector;
				}
			}

			attractionVectors.Add(vector);
		}

		return attractionVectors.ToArray();
	}

	private static Vector3[] Bounding(Boid[] boids, Vector3 boundingOrigin, float boundingScale, bool debug){
		List<Vector3> boundingVectors = new List<Vector3>();

		if(debug) Debug.DrawLine(boundingOrigin, boundingOrigin + Vector3.forward * boundingScale, Color.yellow);

		for(int i = 0; i < boids.Length; i++){
			Vector3 differenceVector = boundingOrigin - boids[i].GetPosition();
			Vector3 vector = Vector3.zero;
			if(Mathf.Abs(differenceVector.magnitude) > boundingScale){
				vector = differenceVector.normalized * (Mathf.Abs(differenceVector.magnitude) - boundingScale/ boundingScale);
				if(debug){
					Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + vector, Color.yellow);
					//Debug.Log("FlockingManager")
				}
			}
			
			boundingVectors.Add(vector);
		}

		return boundingVectors.ToArray();
	}

	private static float Happiness(int neighbours, int flockSize){
		float happiness = - Mathf.Pow(neighbours / flockSize, 2) + 2;
		//Debug.Log("FlockingManager::Happiness ~ Happiness given neighbourhood and ideal flock size is " + happiness.ToString());
		return happiness;
	}
}

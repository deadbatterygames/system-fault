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

	public static Vector3[] Headings(IFlocker[] boids, Vector3[] headings, Vector3[] attractorPositions, float[] attractorScales, Vector3[] separatorPositions, float[] separatorScales, Vector3 boundingOrigin, float boundingScale, float perceptiveDistance, int flockSize, bool debug, bool debugHeading, bool debugCohesion, bool debugSeparation, bool debugAlignment, bool debugAttraction, bool debugBounding){
		List<Vector3> newHeadings = new List<Vector3>();
		List<Vector3> positions = new List<Vector3>();

		foreach(IFlocker boid in boids){
			positions.Add(boid.GetPosition());
		}

		Vector3[] cohesionVectors = Cohesion(positions.ToArray(), perceptiveDistance, flockSize, debug && debugCohesion);
		Vector3[] separationVectors = Separation(positions.ToArray(), separatorPositions, separatorScales, perceptiveDistance, debug && debugSeparation);
		Vector3[] alignmentVectors = Alignment(positions.ToArray(), headings, perceptiveDistance, flockSize, debug && debugAlignment);
		Vector3[] attractionVectors = Attraction(boids, positions.ToArray(), attractorPositions, attractorScales, debug && debugAttraction);
		Vector3[] boundingVectors = Bounding(positions.ToArray(), boundingOrigin, boundingScale, debug && debugBounding);

		for(int i = 0; i < cohesionVectors.Length && i < separationVectors.Length && i < alignmentVectors.Length && i < attractionVectors.Length && i < boundingVectors.Length; i++){
			//Debug.Log("FlockingManager::Headings ~ Creating heading from cohesion vector " + Vector3ToString(cohesionVectors[i]));
			Vector3 vector = cohesionVectors[i] + separationVectors[i] + alignmentVectors[i] + attractionVectors[i] + boundingVectors[i];

			if(boids[i].GetLanding()){
				GameObject body = boids[i].GetLandingBody();
				Vector3 vectorToBody = body.transform.position - boids[i].GetPosition();
				Vector3 vectorToPad = boids[i].GetLandingPosition() - boids[i].GetPosition();

				if(vectorToPad.magnitude * 2 < vectorToBody.magnitude && vectorToBody.magnitude < body.GetComponentInChildren<Renderer>().bounds.extents.magnitude * 2){
					Vector3 adjustment = vectorToBody * (((body.GetComponentInChildren<Renderer>().bounds.extents.magnitude * 2 - vectorToBody.magnitude) / body.GetComponentInChildren<Renderer>().bounds.extents.magnitude) / 5) * Avoid(vectorToBody.magnitude / (body.GetComponentInChildren<Renderer>().bounds.extents.magnitude * 2));
					// if(debug){
					// 	Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + adjustment, Color.gray);
					// 	Debug.Log("Smooth descent");
					// }
					vector += adjustment;
				}
			}

			newHeadings.Add(vector);
		}

		return newHeadings.ToArray();
	}

	public static Vector3 Rotation(Vector3[] rotations){
		Vector3 avg = Vector3.zero;

		foreach(Vector3 rotation in rotations){
			avg += rotation;
		}

		avg = avg / rotations.Length;

		return avg;
	}

	private static Vector3[] Cohesion(Vector3[] positions, float perceptiveDistance, int flockSize, bool debug){

		List<Vector3> cohesionVectors = new List<Vector3>();

		for(int i = 0; i < positions.Length; i++){
			Vector3 vector = Vector3.zero;
			int neighbours = 0;

			//Debug.Log("FlockingManager::Cohesion ~ Calculating cohesion vector of object at position " + Vector3ToString(positions[i]));

			for(int j = 0; j < positions.Length; j++){
				if(i != j){
					Vector3 differenceVector = positions[j] - positions[i];

					if(Mathf.Abs(differenceVector.magnitude) < perceptiveDistance){
						neighbours++;

						//vector += gorbons[j].transform.position;
						vector += positions[j];//differenceVector;
					}
				}
			}

			//vector = (vector / neighbours) - gorbons[i].transform.position;
			//cohesionVectors.Add(vector.normalized);

			if(neighbours > 0) {
				vector = (vector / neighbours) - positions[i];
				vector = vector * Happiness(neighbours, flockSize);
				if(debug) Debug.DrawLine(positions[i], positions[i] + vector, Color.green);
			}

			cohesionVectors.Add(vector);
			//Debug.Log("FlockingManager::Cohesion ~ Calculated cohesion vector of " + Vector3ToString(cohesionVectors[i]));
		}

		return cohesionVectors.ToArray();
	}

	private static float Avoid(float x){
		return (1 / (Mathf.Sqrt(0.2f * Mathf.PI))) * Mathf.Exp(-Mathf.Pow(x, 2) / 0.2f);
	}
	private static Vector3[] Separation(Vector3[] positions, Vector3[] separatorPositions, float[] separatorScales, float perceptiveDistance, bool debug){
		List<Vector3> separationVectors = new List<Vector3>();

		for(int i = 0; i < positions.Length; i++){
			Vector3 vector = Vector3.zero;

			for(int j = 0; j < positions.Length; j++){
				if(i != j){
					Vector3 differenceVector = positions[j] - positions[i];

					if(Mathf.Abs(differenceVector.magnitude) < perceptiveDistance / 2){

						vector -= differenceVector;
					}
				}
			}

			for(int j = 0; j < separatorPositions.Length && j < separatorScales.Length; j++){
				Vector3 differenceVector = separatorPositions[j] - positions[i];

				if(debug) Debug.DrawLine(separatorPositions[j], separatorPositions[j] + Vector3.down * separatorScales[j], Color.red);

				if(Mathf.Abs(differenceVector.magnitude) < separatorScales[j]){

					//Debug.Log("FlockingManager::Separation ~ A flock member has entered the influence of a separator");

					Vector3 avoidanceVector = differenceVector * Avoid(Mathf.Abs(differenceVector.magnitude / separatorScales[j]));//Mathf.Pow(1 + (separatorScales[j] - Mathf.Abs(differenceVector.magnitude) / separatorScales[j]), 2);
					if(debug) Debug.DrawLine(positions[i], positions[i] - avoidanceVector, Color.magenta);
					vector -= avoidanceVector;
				}
			}

			if(debug) Debug.DrawLine(positions[i], positions[i] + vector, Color.red);
			separationVectors.Add(vector);
		}

		return separationVectors.ToArray();
	}

	private static Vector3[] Alignment(Vector3[] positions, Vector3[] headings, float perceptiveDistance, int flockSize, bool debug){
		List<Vector3> alignmentVectors = new List<Vector3>();

		for(int i = 0; i < positions.Length; i++){
			Vector3 vector = Vector3.zero;
			int neighbours = 0;

			for(int j = 0; j < positions.Length && j < headings.Length; j++){
				if(i != j){
					Vector3 differenceVector = positions[j] - positions[i];

					if(Mathf.Abs(differenceVector.magnitude) < perceptiveDistance){
						neighbours++;

						vector += headings[j];
					}
				}
			}

			//if(neighbours > 0) vector = vector / neighbours;
			if(debug) Debug.DrawLine(positions[i], positions[i] + vector.normalized * perceptiveDistance, Color.blue);
			//Debug.Log("FlockingManager::Alignment ~ Adding aligment vector " + Vector3ToString(vector));
			alignmentVectors.Add(vector);
		}

		return alignmentVectors.ToArray();
	}

	private static Vector3[] Attraction(IFlocker[] boids, Vector3[] positions, Vector3[] attractorPositions, float[] attractorScales, bool debug){
		List<Vector3> attractionVectors = new List<Vector3>();

		for(int i = 0; i < positions.Length; i++){
			Vector3 vector = Vector3.zero;
			List<Attractor> attractors = boids[i].GetAttractors();

			for(int j = 0; j < attractorPositions.Length && j < attractorScales.Length; j++){
				Vector3 differenceVector = attractorPositions[j] - positions[i];

				if(debug) Debug.DrawLine(attractorPositions[j], attractorPositions[j] + Vector3.back * attractorScales[j], Color.green);

				if(Mathf.Abs(differenceVector.magnitude) < attractorScales[j]){

					Vector3 attractionVector = differenceVector * (attractorScales[j] - Mathf.Abs(differenceVector.magnitude)) / attractorScales[j];
					if(debug) Debug.DrawLine(positions[i], positions[i] + attractionVector, Color.green);
					vector += attractionVector;
				}
			}

			for(int j = 0; j < attractors.Count; j++){
				Vector3 differenceVector = attractors[j].go.transform.position - positions[i];

				if(Mathf.Abs(differenceVector.magnitude) < attractors[j].radius){

					Vector3 attractionVector = differenceVector / Mathf.Log(differenceVector.magnitude);//2 * differenceVector;//differenceVector;//.normalized * Mathf.Log(attractors[j].radius); //differenceVector * (attractors[j].radius - Mathf.Abs(differenceVector.magnitude)) / attractors[j].radius;
					if(debug) Debug.DrawLine(positions[i], positions[i] + attractionVector, Color.cyan);
					vector += attractionVector;
				}
			}

			attractionVectors.Add(vector);
		}

		return attractionVectors.ToArray();
	}

	private static Vector3[] Bounding(Vector3[] positions, Vector3 boundingOrigin, float boundingScale, bool debug){
		List<Vector3> boundingVectors = new List<Vector3>();

		if(debug) Debug.DrawLine(boundingOrigin, boundingOrigin + Vector3.forward * boundingScale, Color.yellow);

		for(int i = 0; i < positions.Length; i++){
			Vector3 differenceVector = boundingOrigin - positions[i];
			Vector3 vector = (Mathf.Abs(differenceVector.magnitude) > boundingScale) ? differenceVector.normalized * (Mathf.Abs(differenceVector.magnitude) - boundingScale/ boundingScale) : Vector3.zero;
			if(debug) Debug.DrawLine(positions[i], positions[i] + vector, Color.yellow);
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

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

	public static Vector3[] Headings(Boid[] boids, Attractor[] attractors, Separator[] separators, Inhibitor[] inhibitors, Vector3 boundingOrigin, float boundingScale, int flockSize, bool debug, bool debugHeading, bool debugCohesion, bool debugSeparation, bool debugAlignment, bool debugAttraction, bool debugBounding, bool cohesion = true, bool alignment = true, bool separation = true){
		List<Vector3> newHeadings = new List<Vector3>();

		Vector3[] cohesionVectors = new Vector3[]{};
		if(cohesion) cohesionVectors = Cohesion(boids, flockSize, debug && debugCohesion);
		Vector3[] flockSeparationVectors = new Vector3[]{};
		if(separation) flockSeparationVectors = FlockSeparation(boids, flockSize, debug && debugSeparation);
		Vector3[] separationVectors = new Vector3[]{};
		separationVectors = Separation(boids, separators, debug && debugSeparation);
		Vector3[] alignmentVectors = new Vector3[]{};
		if(alignment) alignmentVectors = Alignment(boids, flockSize, debug && debugAlignment);
		Vector3[] attractionVectors = Attraction(boids, attractors, debug && debugAttraction);
		Vector3[] boundingVectors = Bounding(boids, boundingOrigin, boundingScale, debug && debugBounding);

		//if(debug) DrawPerceptiveDistance(boids);

		for(int i = 0; i < Mathf.Max(cohesionVectors.Length, flockSeparationVectors.Length, separationVectors.Length, alignmentVectors.Length, attractionVectors.Length, boundingVectors.Length); i++){
			//Debug.Log("FlockingManager::Headings ~ Creating heading from cohesion vector " + Vector3ToString(cohesionVectors[i]));
			// Vector3 vector = ((cohesionVectors.Length < i) ? cohesionVectors[i] : Vector3.zero) 
			// 				+ ((separationVectors.Length < i) ? separationVectors[i] : Vector3.zero) 
			// 				+ ((alignmentVectors.Length < i) ? alignmentVectors[i] : Vector3.zero) 
			// 				+ ((attractionVectors.Length < i) ? attractionVectors[i] : Vector3.zero) 
			// 				+ ((boundingVectors.Length < i) ? boundingVectors[i] : Vector3.zero);

			Vector3 vector = Vector3.zero;

			if(i < cohesionVectors.Length && boids[i].flocking){
				//if(debug && debugCohesion) Debug.Log("FlockingManager::Headings ~ Adding cohesion vector " + cohesionVectors[i].ToString() + " to boid " + boids[i].name);
				vector += cohesionVectors[i];
			}
			if(i < flockSeparationVectors.Length && boids[i].flocking){
				//if(debug && debugSeparation) Debug.Log("FlockingManager::Headings ~ Adding flocking separation vector " + flockSeparationVectors[i].ToString() + " to boid " + boids[i].name);
				vector += flockSeparationVectors[i];
			}
			if(i < separationVectors.Length) vector += separationVectors[i];
			if(i < alignmentVectors.Length && boids[i].flocking) vector += alignmentVectors[i];
			if(i < attractionVectors.Length) vector += attractionVectors[i];
			if(i < boundingVectors.Length) vector += boundingVectors[i];

			newHeadings.Add(vector);
		}

		return Inhibition(boids, inhibitors, newHeadings, debug);
	}

	public static Vector3[] Inhibition(Boid[] boids, Inhibitor[] inhibitors, List<Vector3> headings, bool debug){
		for(int i = 0; i < boids.Length; i++){
			Vector3 heading = headings[i];

			for(int j = 0; j < inhibitors.Length; j++){
				Vector3 boidPosition = boids[i].GetPosition();

				if(inhibitors[j].inStrip){
					Inhibitor previous = inhibitors[j].GetPrevious();
					Inhibitor next = inhibitors[j].GetNext();
					Inhibitor closest = null;

					if(previous != null && next != null) closest = ((previous.GetPosition() - boidPosition).sqrMagnitude < (next.GetPosition() - boidPosition).sqrMagnitude) ? previous : next;
					else{
						if(previous != null) closest = previous;
						else if(next != null) closest = next;
						else Debug.LogError("FlockingManager::Inhibition ~ Inhibitor node in strip has no neighbours");
					}

					// Find the point on the segment from this inhibitor to the closeset neighbour
					// where a vector normal to the segment intersects the boid's position
					// Explanation: Consider a segment defined by points a and b
					// A point on that segment is defined by p(t) = a + td, where d is their difference b - a
					// So the point where the distance to the boid is normal to the segment
					// d . (p(t) - B) = 0
					// d . (a + td - B) = 0
					// t = -((d . a) - (d . B)) / (d . d)
					// If t is between 0 and 1 then the segment intersects the plane at B normal to the segment

					Vector3 a = inhibitors[j].GetPosition();
					Vector3 b = closest.GetPosition();
					Vector3 d = b - a;

					float t = (-Vector3.Dot(d, a) + Vector3.Dot(d, boidPosition)) / d.sqrMagnitude;//-(Vector3.Dot(d, a) - boidPosition.magnitude) / d.sqrMagnitude;
					Vector3 p = a + t * d;

					if(0 <= t && t <= 1){
						if(debug) Debug.DrawLine(boidPosition, p, Color.black);
						Vector3 dp = p - boidPosition;

						if(dp.sqrMagnitude < inhibitors[j].radius * inhibitors[j].radius){
							
						}
					} 
				}
				else{
					Vector3 dif = inhibitors[j].GetPosition() - boidPosition;

					if(dif.sqrMagnitude < inhibitors[j].radius * inhibitors[j].radius){
						// If the heading is pointing towards the inhibitor
						if(Vector3.Dot(heading, dif.normalized) > 0){
							if(debug) Debug.DrawLine(boidPosition, boidPosition + heading, Color.black);

							// Project the heading onto the plane spanned by the difference vector and the difference of the difference vector and the heading
							heading = Vector3.Project(heading, (heading.normalized - dif.normalized).normalized);
						}
					}
				}
			}

			headings[i] = heading;
		}

		return headings.ToArray();
	}

	public static void DrawPerceptiveDistance(Boid[] boids){
		foreach(Boid boid in boids){
			Debug.DrawLine(boid.GetPosition(), boid.GetPosition() + (boid.transform.forward * boid.PerceptiveDistance()), Color.cyan);
		}
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
				//vector = vector.normalized * Mathf.Log(vector.magnitude * Happiness(neighbours, flockSize));
				//vector = vector * Happiness(neighbours, flockSize);
			}

			if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + vector, Color.green);
			cohesionVectors.Add(vector);
			//Debug.Log("FlockingManager::Cohesion ~ Calculated cohesion vector of " + Vector3ToString(cohesionVectors[i]));
		}

		return cohesionVectors.ToArray();
	}

	private static float Avoid(float x){
		return (1 / (Mathf.Sqrt(0.2f * Mathf.PI))) * Mathf.Exp(-Mathf.Pow(x, 2) / 0.2f);
	}

	private static Vector3[] FlockSeparation(Boid[] boids, int flockSize, bool debug){
		List<Vector3> separationVectors = new List<Vector3>();

		for(int i = 0; i < boids.Length; i++){
			Vector3 vector = Vector3.zero;
			int neighbours = 0;

			for(int j = 0; j < boids.Length; j++){
				if(i != j && boids[j].flocking){
					Vector3 differenceVector = boids[j].GetPosition() - boids[i].GetPosition();

					if(Mathf.Abs(differenceVector.magnitude) < boids[i].PerceptiveDistance()){
						float depth = boids[i].PerceptiveDistance() - differenceVector.magnitude;

						vector += differenceVector.normalized * depth * Separate(depth, boids[i].PerceptiveDistance());
						neighbours++;
						//vector -= Separate(differenceVector.magnitude, boids[i].PerceptiveDistance()) * differenceVector.normalized;
					}
				}
			}

			// if(neighbours > 0){
			// 	vector = vector * Happiness(neighbours, flockSize);
			// }

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
					float depth = separators[j].radius - differenceVector.magnitude;

					Vector3 avoidanceVector = differenceVector.normalized * depth * Separate(depth, separators[j].radius);//Avoid(Mathf.Abs(differenceVector.magnitude / separators[j].radius));//Mathf.Pow(1 + (separatorScales[j] - Mathf.Abs(differenceVector.magnitude) / separatorScales[j]), 2);
					
					if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + avoidanceVector, Color.magenta);
					vector += avoidanceVector; //differenceVector.normalized * depth;
				}
			}

			List<Separator> localSeparators = boids[i].GetSeparators();
			for(int j = 0; j < localSeparators.Count; j++){
				Vector3 differenceVector = localSeparators[j].GetPosition() - boids[i].GetPosition();

				if(debug) Debug.DrawLine(localSeparators[j].GetPosition(), localSeparators[j].GetPosition() + Vector3.down * localSeparators[j].radius, Color.red);

				if(Mathf.Abs(differenceVector.magnitude) < localSeparators[j].radius){
					float depth = localSeparators[j].radius - differenceVector.magnitude;

					Vector3 avoidanceVector = differenceVector.normalized * depth * Separate(depth, localSeparators[j].radius);
					//if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() - differenceVector.normalized * depth, Color.magenta);
					vector += avoidanceVector;//differenceVector.normalized * depth;
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
		
			//vector = vector.normalized * Mathf.Log(vector.magnitude);

			if(neighbours > 0) vector = vector / neighbours;
			if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + vector, Color.blue);
			//Debug.Log("FlockingManager::Alignment ~ Adding aligment vector " + Vector3ToString(vector));
			alignmentVectors.Add(vector);
		}

		return alignmentVectors.ToArray();
	}

	private static float Attract(float distance, float radius){
		//if(radius == float.PositiveInfinity) return 0.01f;
		return (distance) / radius;
	}

	private static float Separate(float distance, float radius){
		// ((radius - distance) / radius) + 1 - (distance / radius)
		// 
		//return Attract(distance, radius) + 1 - (distance / radius);//2 - distance / radius;
		float f = Attract(distance * distance, radius);
		return -(f * f);
	}

	private static Vector3[] Attraction(Boid[] boids, Attractor[] attractors, bool debug){
		List<Vector3> attractionVectors = new List<Vector3>();

		for(int i = 0; i < boids.Length; i++){
			Vector3 vector = Vector3.zero;
			List<Attractor> localAttractors = boids[i].GetAttractors();

			for(int j = 0; j < attractors.Length; j++){
				Vector3 differenceVector = attractors[j].GetPosition() - boids[i].GetPosition();

				if(debug) Debug.DrawLine(attractors[j].GetPosition(), attractors[j].GetPosition() + Vector3.back * attractors[j].radius, Color.black);

				if(Mathf.Abs(differenceVector.magnitude) < attractors[j].radius){

					Vector3 attractionVector = differenceVector * Attract(differenceVector.sqrMagnitude, attractors[j].radius);

					if(debug) Debug.DrawLine(boids[i].GetPosition(), boids[i].GetPosition() + attractionVector, Color.cyan);
					vector += attractionVector;
				}
			}

			for(int j = 0; j < localAttractors.Count; j++){
				Vector3 differenceVector = localAttractors[j].GetPosition() - boids[i].GetPosition();

				if(debug) Debug.DrawLine(localAttractors[j].GetPosition(), localAttractors[j].GetPosition() + Vector3.back * localAttractors[j].radius, Color.green);

				if(Mathf.Abs(differenceVector.magnitude) < localAttractors[j].radius){

					Vector3 attractionVector = differenceVector * Attract(differenceVector.magnitude, attractors[j].radius);//differenceVector / Mathf.Log(differenceVector.magnitude);//2 * differenceVector;//differenceVector;//.normalized * Mathf.Log(attractors[j].radius); //differenceVector * (attractors[j].radius - Mathf.Abs(differenceVector.magnitude)) / attractors[j].radius;
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
				vector = differenceVector * Attract(differenceVector.sqrMagnitude, boundingScale);
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

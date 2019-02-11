using UnityEngine;
using UnityEditor;

public class FlockingObstacle : MonoBehaviour {

	public SphereCollider collider;
	public bool hasAttractor;
	public Attractor attractor;
	public float attractorRadius;
	public bool hasSeparator;
	public Separator separator;
	public float separatorRadius;
	public bool hasInhibitor;
	public Inhibitor inhibitor;
	public float inhibitorRadius;
	public bool grounded;
	public ObstacleTypes type;
	public FlockingObstacle previous, next;
	[SerializeField] private float radius;

	public enum ObstacleTypes
	{
		Player,
		Energy,
		Water,
		Hazard,
		Obstacle,
		InhibitorStrip
	}

	#if UNITY_EDITOR
	[ExecuteInEditMode]
	void OnDrawGizmosSelected(){
		if(type == ObstacleTypes.InhibitorStrip){
			int numberOfPoints = 5;
			if(next != null){
				//inhibitorRadius = radius;
				Vector3 toNext = next.transform.position - transform.position;
				transform.rotation = Quaternion.LookRotation(toNext, transform.up);
				Vector3 previousPoint = Vector3.zero;
				Vector3 pointAroundCenter = transform.right * inhibitorRadius;

				
				float theta = 360 / numberOfPoints;

				Gizmos.color = Color.black;
				Gizmos.DrawLine(transform.position, next.transform.position);
				// collider.radius = inhibitorRadius;
				Gizmos.DrawWireSphere(transform.position, inhibitorRadius);
				Gizmos.DrawWireSphere(next.transform.position, inhibitorRadius);

				for(int i = 0; i < numberOfPoints + 1; i++){
					if(previousPoint != Vector3.zero){
						Gizmos.DrawLine(transform.position + previousPoint, transform.position + pointAroundCenter);
						Gizmos.DrawLine(next.transform.position + previousPoint, next.transform.position + pointAroundCenter);
					}

					Gizmos.DrawLine(transform.position + pointAroundCenter, next.transform.position + pointAroundCenter);

					previousPoint = pointAroundCenter;
					transform.rotation = transform.rotation * Quaternion.AngleAxis(theta, transform.forward);
					transform.rotation = Quaternion.LookRotation(toNext, transform.up);
					pointAroundCenter = transform.right * inhibitorRadius;
				}
			}
		}
		else{
			if(hasAttractor){
				Gizmos.color = Color.green;
				Gizmos.DrawWireSphere(transform.position, attractorRadius);
			}

			if(hasSeparator){
				Gizmos.color = Color.red;
				Gizmos.DrawWireSphere(transform.position, separatorRadius);
			}

			if(hasInhibitor){
				Gizmos.color = Color.black;
				Gizmos.DrawWireSphere(transform.position, inhibitorRadius);
			}
		}
	}

	#endif

	void Awake(){
		if(radius <= 0){
			Renderer[] renderers = gameObject.transform.parent.GetComponentsInChildren<Renderer>();
			
			if(renderers.Length > 0){
				Renderer largestRenderer = renderers[0];

				foreach(Renderer renderer in renderers){
					if(renderer.bounds.extents.magnitude > largestRenderer.bounds.extents.magnitude) largestRenderer = renderer;
				}
				
				radius = Vector3.ProjectOnPlane(largestRenderer.bounds.extents, transform.up).magnitude;
			}
		}

		switch(type){
			case ObstacleTypes.Player:
			case ObstacleTypes.Energy:
				hasAttractor = true;
				hasSeparator = true;
				hasInhibitor = false;
				break;
			case ObstacleTypes.Water:
				hasAttractor = false;
				hasSeparator = true;
				hasInhibitor = true;
				break;
			case ObstacleTypes.Hazard:
			case ObstacleTypes.Obstacle:
				hasAttractor = false;
				hasSeparator = true;
				hasInhibitor = false;
				break;
			case ObstacleTypes.InhibitorStrip:
				hasAttractor = false;
				hasSeparator = false;
				hasInhibitor = true;
				break;
			default:
				hasAttractor = false;
				hasSeparator = false;
				hasInhibitor = false;
				break;
		}		

		if(hasInhibitor){
			Inhibitor.Type inhibitorType = Inhibitor.Type.None;
			if(inhibitorRadius == 0.0f) inhibitorRadius = 0.75f * radius;
			bool inStrip = false;

			switch(type){
				case ObstacleTypes.Water:
					inhibitorType = Inhibitor.Type.Water;
					break;
				case ObstacleTypes.InhibitorStrip:
					inhibitorType = Inhibitor.Type.StripNode;
					inStrip = true;
					break;
				default:
					break;
			}

			if(inStrip) inhibitor = new Inhibitor(gameObject, inhibitorRadius, previous, next);
			else inhibitor = new Inhibitor(gameObject, inhibitorRadius, inhibitorType);
		}
		else inhibitor = null;

		if(hasSeparator){
			Separator.Type separatorType = Separator.Type.None;
			if(separatorRadius == 0.0f) separatorRadius = 1.5f * radius;

			switch(type){
				case ObstacleTypes.Player:
					separatorType = Separator.Type.Obstacle;
					separatorRadius = 20f;
					break;
				default:
					separatorType = Separator.Type.Obstacle;
					break;
			}

			separator = new Separator(gameObject, separatorRadius, separatorType);
		}
		else separator = null;

		if(hasAttractor){
			Attractor.Type attractorType = Attractor.Type.None;
			if(attractorRadius == 0.0f) attractorRadius = 2 * radius;

			switch(type){
				case ObstacleTypes.Player:
					attractorType = Attractor.Type.Player;
					attractorRadius = 4 * separatorRadius;
					break;
				case ObstacleTypes.Energy:
					attractorType = Attractor.Type.Energy;
					break;
				default:
					attractorType = Attractor.Type.None;
					break;
			}

			attractor = new Attractor(gameObject, attractorRadius, attractorType);
		}
		else attractor = null;

		collider.radius = Mathf.Max(separatorRadius, attractorRadius, inhibitorRadius);
	}

	// void OnTriggerEnter(Collider col){
	// 	//Debug.Log("FlockingObstacle " + gameObject.name + " collided with something");
	// }
}
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
	[SerializeField] private float radius;

	public enum ObstacleTypes
	{
		Player,
		Energy,
		Water,
		Hazard,
		Obstacle
	}

	[ExecuteInEditMode]
	void OnDrawGizmosSelected(){
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

	void Awake(){
		if(radius <= 0){
			Renderer[] renderers = gameObject.transform.parent.GetComponentsInChildren<Renderer>();
			
			if(renderers.Length > 0){
				Renderer largestRenderer = renderers[0];

				foreach(Renderer renderer in renderers){
					if(renderer.bounds.extents.magnitude > largestRenderer.bounds.extents.magnitude) largestRenderer = renderer;
				}
				
				radius = 2 * Vector3.ProjectOnPlane(largestRenderer.bounds.extents, transform.up).magnitude;
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
			default:
				hasAttractor = false;
				hasSeparator = false;
				hasInhibitor = false;
				break;
		}		

		if(hasInhibitor){
			Inhibitor.Type inhibitorType = Inhibitor.Type.None;
			if(inhibitorRadius == 0.0f) inhibitorRadius = 0.75f * radius;

			switch(type){
				case ObstacleTypes.Water:
					inhibitorType = Inhibitor.Type.Water;
					break;
				default:
					break;
			}

			inhibitor = new Inhibitor(gameObject, inhibitorRadius, inhibitorType);
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

public class ObstacleGizmoDrawer
{
    [DrawGizmo(GizmoType.InSelectionHierarchy | GizmoType.Active)]
	void Draw(FlockingObstacle obstacle, GizmoType gizmoType){
		if(obstacle.hasAttractor){
			Gizmos.color = Color.green;
        	Gizmos.DrawWireSphere(obstacle.transform.position, obstacle.attractorRadius);
		}

		if(obstacle.hasSeparator){
			Gizmos.color = Color.red;
			Gizmos.DrawWireSphere(obstacle.transform.position, obstacle.separatorRadius);
		}

		if(obstacle.hasInhibitor){
			Gizmos.color = Color.black;
			Gizmos.DrawWireSphere(obstacle.transform.position, obstacle.inhibitorRadius);
		}
	}
}
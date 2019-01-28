using UnityEngine;

public class FlockingObstacle : MonoBehaviour {

	public SphereCollider collider;
	public bool hasAttractor;
	public Attractor attractor;
	public bool hasSeparator;
	public Separator separator;
	public bool grounded;
	public ObstacleTypes type;
	[SerializeField] private float radius;

	public enum ObstacleTypes
	{
		Player,
		Energy,
		Hazard,
		Obstacle
	}

	void Awake(){
		if(radius <= 0){
			Renderer[] renderers = gameObject.transform.parent.GetComponentsInChildren<Renderer>();
			Renderer largestRenderer = renderers[0];

			foreach(Renderer renderer in renderers){
				if(renderer.bounds.extents.magnitude > largestRenderer.bounds.extents.magnitude) largestRenderer = renderer;
			}
			
			radius = 2 * largestRenderer.bounds.extents.magnitude;

			// if(type == ObstacleTypes.Player){
			// 	radius = Mathf.Max(radius, 50);
			// }
		}

		switch(type){
			case ObstacleTypes.Player:
			case ObstacleTypes.Energy:
				hasAttractor = true;
				hasSeparator = true;
				break;
			case ObstacleTypes.Hazard:
			case ObstacleTypes.Obstacle:
				hasAttractor = false;
				hasSeparator = true;
				break;
			default:
				hasAttractor = false;
				hasSeparator = false;
				break;
		}		

		float separatorRadius = 0.0f;
		float attractorRadius = 0.0f;

		if(hasSeparator){
			Separator.Type separatorType = Separator.Type.None;
			separatorRadius = radius;

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
			attractorRadius = 2 * radius;

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

		collider.radius = Mathf.Max(separatorRadius, attractorRadius);
	}

	void OnTriggerEnter(Collider col){
		//Debug.Log("FlockingObstacle " + gameObject.name + " collided with something");
	}
}

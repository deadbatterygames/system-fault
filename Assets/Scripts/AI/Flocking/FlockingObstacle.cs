using UnityEngine;

public class FlockingObstacle : MonoBehaviour {

	public SphereCollider collider;
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
			
			radius = largestRenderer.bounds.extents.magnitude;
			//radius = radius * radius;

			if(type == ObstacleTypes.Player){
				radius = Mathf.Max(radius, 15);
			}
		}

		collider.radius = radius;
		separator = new Separator(gameObject, radius, Separator.Type.Obstacle);
	}

	void OnTriggerEnter(Collider col){
		//Debug.Log("FlockingObstacle " + gameObject.name + " collided with something");
	}
}

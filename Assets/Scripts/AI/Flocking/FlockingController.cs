using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlockingController : MonoBehaviour {

	public static FlockingController controller;
	[SerializeField] private List<Flock> flocks; 

	// Use this for initialization
	void Awake () {
		controller = this;
		this.flocks = new List<Flock>();
		// Flock[] flocks = FindObjectsOfType<Flock>();

		// foreach(Flock flock in flocks){
		// 	this.flocks.Add(flock);
		// }
	}

	public static void AddFlock(Flock flock){
		controller.flocks.Add(flock);
	}
	
	public static void CreateAttractor(Attractor attractor){
		Debug.Log("FlockingController::CreateSeparator ~ Creating attractor for " + attractor.go.name);
		foreach(Flock flock in controller.flocks){
			flock.AddAttractor(attractor);
		}
	}

	public static void CreateSeparator(Separator separator){
		Debug.Log("FlockingController::CreateSeparator ~ Creating separator for " + separator.go.name);
		foreach(Flock flock in controller.flocks){
			flock.AddSeparator(separator);
		}
	}

	public static void DestroyAttractor(GameObject attractor, Attractor.Type type = Attractor.Type.None){
		foreach(Flock flock in controller.flocks){
			flock.RemoveAttractor(attractor, type);
		}
	}

	public static void DestroyAttractor(Attractor attractor){
		foreach(Flock flock in controller.flocks){
			flock.RemoveAttractor(attractor);
		}
	}
	
	public static void DestroySeparator(GameObject separator, Separator.Type type = Separator.Type.None){
		foreach(Flock flock in controller.flocks){
			flock.RemoveSeparator(separator, type);
		}
	}

	public static void DestroySeparator(Separator separator){
		foreach(Flock flock in controller.flocks){
			flock.RemoveSeparator(separator);
		}
	}
}

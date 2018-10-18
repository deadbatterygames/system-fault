using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingPad : MonoBehaviour {

	public List<GameObject> pads;
	public List<bool> padEmpty;
	void Awake(){
		padEmpty = new List<bool>();
		foreach(GameObject pad in pads){
			padEmpty.Add(true);
		}
	}
	public GameObject GetBody(){
		return transform.parent.gameObject;
	}
	private bool AnyEmpty(){
		foreach(bool pad in pads){
			if(pad) return true;
		}

		return false;
	}
	public GameObject GetLandingPad(){
		if(AnyEmpty()){
			int i = Random.Range(0, padEmpty.Count);

			while(!padEmpty[i]) i = Random.Range(0, padEmpty.Count);

			return pads[i];
		}

		return null;
	}
}

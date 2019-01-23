using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LandingPad : Flock {

	public List<GameObject> pads;
	public List<bool> padEmpty;
	[SerializeField] Planet planet;
	Queue<Xrom> landingXroms;
	float landingBarrierRadius;
	void Start(){
		base.InitializeFlock();
		padEmpty = new List<bool>();
		foreach(GameObject pad in pads){
			padEmpty.Add(true);
		}
		landingXroms = new Queue<Xrom>();

		planet = transform.parent.GetComponent<Planet>();
		gameObject.GetComponent<SphereCollider>().radius = planet.GetRadius() / 2; //(transform.parent.gameObject.transform.position - transform.position).magnitude / 4;
		boundingRadius = planet.GetRadius() / 2;
		landingBarrierRadius = planet.GetRadius() / 4;

		separators.Add(new Separator(gameObject, 10.0f));
	}
	public override void FixedUpdate(){
		base.FixedUpdate();

		if(landingXroms.Count > 0){
			Xrom xrom = landingXroms.Peek();
			if(xrom.HasLanded()){
				landingXroms.Dequeue();
			}
			else{
				if(xrom.HasLandingPermission()){

				}
				else{
					xrom.SetLandingPermission(true);
					xrom.RemoveSeparatorType(Separator.Type.LandingBarrier);
					xrom.flocking = false;
				}
			}
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
	public Attractor GetLandingPad(){
		if(AnyEmpty()){
			int i = Random.Range(0, padEmpty.Count);

			while(!padEmpty[i]) i = Random.Range(0, padEmpty.Count);

			padEmpty[i] = false;
			return new Attractor(pads[i], float.PositiveInfinity, gameObject, planet.gameObject);
		}

		return null;
	}
	public override float Speed(Boid boid){
		return planet.Speed(boid);
	}
	protected override void BoidTrigger(Boid boid){
		if(boid is Xrom){
			Xrom xrom = (Xrom) boid;
			if(xrom.Landing()){
				base.BoidTrigger(boid);
				Debug.Log("LandingPad::BoidTrigger ~ Informing Xrom of landing pad");

				boid.AddSeparator(new Separator(gameObject, landingBarrierRadius, Separator.Type.LandingBarrier));

				if(xrom.RequestLanding()){
					landingXroms.Enqueue(xrom);
				}
			}
		}
	}
}

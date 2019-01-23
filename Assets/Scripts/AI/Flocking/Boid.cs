using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour {

	protected Rigidbody rb;
	[SerializeField] protected float perceptiveDistance;
	protected List<Attractor> attractors;
	protected List<Separator> separators;
	[SerializeField] protected Flock flock;
	public bool flocking;
	public bool grounded;
	void Awake(){
		rb = transform.GetComponent<Rigidbody>();
		attractors = new List<Attractor>();
		separators = new List<Separator>();
		//flocking = true;
	}
	public virtual void Move(Vector3 heading, bool debug){}
	public virtual void Rotate(Vector3 rotation){}
	public void SetFlock(Flock flock){
		this.flock = flock;
	}
	public void RemoveFromFlock(){
		if(this.flock != null) this.flock.RemoveBoid(this);
	}
	public Flock GetFlock(){
		return flock;
	}
	public Vector3 GetPosition(){
		return transform.position;
	}
	public Vector3 GetRotation(){
		return transform.rotation.eulerAngles;
	}
	public Vector3 GetVelocity(){
		return rb.velocity;
	}
	public float PerceptiveDistance(){
		//Debug.Log(gameObject.name + " has perceptive distance " + perceptiveDistance.ToString());
		return perceptiveDistance;
	}
	public void SetPerceptiveDistance(float perceptiveDistance){
		this.perceptiveDistance = perceptiveDistance;
	}
	// public void Land(GameObject landingPad, GameObject landingBody){
	// 	AddAttractor(new Attractor(landingPad, float.PositiveInfinity, Attractor.Type.LandingPoint));
	// }
	public void AddAttractor(Attractor attractor){
		attractors.Add(attractor);
	}
	public void AddSeparator(Separator separator){
		separators.Add(separator);
	}
	public void RemoveSeparatorType(Separator.Type type){
		separators = separators.Where(x => x.type != type).ToList();
	}
	public List<Attractor> GetAttractors(){return attractors;}
	public List<Separator> GetSeparators(){return separators;}
}

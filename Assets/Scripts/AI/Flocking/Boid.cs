using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Boid : MonoBehaviour {

	protected Rigidbody rb;
	protected float perceptiveDistance = 50.0f;
	protected List<Attractor> attractors;
	protected List<Separator> separators;
	protected Flock flock;
	public bool flocking;
	void Awake(){
		rb = transform.GetComponent<Rigidbody>();
		attractors = new List<Attractor>();
		separators = new List<Separator>();
		flocking = true;
	}
	public virtual void Move(Vector3 heading, bool debug){}
	public virtual void Rotate(Vector3 rotation){}
	public void SetFlock(Flock flock){
		this.flock = flock;
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
		return perceptiveDistance;
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

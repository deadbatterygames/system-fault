using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IFlocker {

	void Move(Vector3 heading, bool debug);
	void Rotate(Vector3 rotation);

	Vector3 GetPosition();
	Vector3 GetRotation();
	Vector3 GetVelocity();
	void AddAttractor(Attractor attractor);
	void Land(GameObject landingPad, GameObject landingBody);
	bool GetLanding();
	GameObject GetLandingBody();
	Vector3 GetLandingPosition();
	List<Attractor> GetAttractors();
}

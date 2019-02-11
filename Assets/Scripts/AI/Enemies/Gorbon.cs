using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gorbon : Boid, IDamageable, IGroundable
{
    private float health = 100f;

    public float gorbonDrag = 0.5f;
    private float knockbackScale = 5f;
    private float maxForce = 0.5f;
    public void InitializeGorbon(){
		this.perceptiveDistance = 15f;

		GameManager.instance.AddGravityBody(GetComponent<Rigidbody>());
	}

    public override void Move(Vector3 heading, bool debug){
        if (grounded) {
            Vector3 input = Vector3.ClampMagnitude(Vector3.ProjectOnPlane(heading, flock.transform.up) * Overmind.instance.movementScale, 1f);
            Vector3 dv = Vector3.ClampMagnitude((input * flock.Speed(this) - rb.velocity), maxForce * flock.Speed(this));

            rb.AddForce(dv, ForceMode.VelocityChange);

            if (rb.velocity.sqrMagnitude < 0.2f) {
                rb.velocity = Vector3.zero;
                rb.angularDrag = gorbonDrag;
            }
            else rb.angularDrag = 0.0f;
        }
    }

    public override void Rotate(Vector3 rotation){
        rb.AddTorque(rotation, ForceMode.Impulse);
    }

    public void Damage(float damage, GameTypes.DamageType damageType, Vector3 force, Vector3 pointOfImpact, Vector3 directionOfImpact){
        Debug.Log("Gorbon::Damage ~ Dealing " + damage.ToString() + " points of " + damageType + " damage to " + gameObject.name);
		health -= damage;
        force = knockbackScale * force;
        Vector3 differenceToPointOfImpact = pointOfImpact - rb.position;
        Vector3 differenceOnUp = Vector3.ProjectOnPlane(differenceToPointOfImpact, flock.transform.up);
        Vector3 left = Vector3.Cross(flock.transform.up, directionOfImpact);
        float extensionInRight = -Vector3.Dot(differenceOnUp, left);

		rb.AddForce(force, ForceMode.Impulse);

        Rotate(flock.transform.up * force.magnitude * extensionInRight);

		if(health <= 0){
			DestroyGorbon();
		}
    }

    private void DestroyGorbon(){
        Debug.Log("Gorbon::DestroyGorbon ~ Oh, I'm destroyed (;___________;) ");
    }
}

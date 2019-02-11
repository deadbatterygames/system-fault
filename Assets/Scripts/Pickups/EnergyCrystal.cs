using UnityEngine;

//
// EnergyCrystal.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Crystals which shatter into collectable energy
//

public class EnergyCrystal : MonoBehaviour, IDamageable {

    [SerializeField] GameObject shatteredCrystal;
    [SerializeField] FlockingObstacle obstacle;
    float health = 30f;

    bool shattered;

    public void Damage(float amount, GameTypes.DamageType damageType, Vector3 damageForce, Vector3 pointOfImpact, Vector3 directionOfImpact) {
        health -= amount;
        if (health <= 0 && !shattered) Shatter(damageForce);
    }

    void Shatter(Vector3 shatterForce) {
        shattered = true;

        FlockingController.DestroySeparator(obstacle.separator);
        FlockingController.DestroyAttractor(obstacle.attractor);
        GameObject pieces = Instantiate(shatteredCrystal, transform.position, transform.rotation);
        pieces.transform.localScale = transform.localScale;

        Rigidbody[] shards = pieces.GetComponentsInChildren<Rigidbody>();
        foreach (Rigidbody shard in shards) shard.AddForce(shatterForce, ForceMode.Impulse);

        Destroy(gameObject);
    }
}

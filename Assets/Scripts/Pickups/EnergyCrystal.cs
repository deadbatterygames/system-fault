using UnityEngine;

//
// EnergyCrystal.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Crystals which shatter into collectable energy
//

public class EnergyCrystal : MonoBehaviour, IDamageable {

    [SerializeField] GameObject shatteredCrystal;
    float health = 30f;

    bool shattered;

    public void Damage(float amount, Vector3 damageForce) {
        health -= amount;
        if (health <= 0) Shatter(damageForce);
    }

    void Shatter(Vector3 shatterForce) {
        if (!shattered) {
            Debug.LogWarning("EnergyCrystal: Shatter magnitude " + shatterForce.magnitude);
            GameObject pieces = Instantiate(shatteredCrystal, transform.position, transform.rotation);
            pieces.transform.localScale = transform.localScale;

            Rigidbody[] shards = pieces.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody shard in shards) shard.AddForce(shatterForce, ForceMode.Impulse);

            Destroy(gameObject);

            shattered = true;
        }
    }
}

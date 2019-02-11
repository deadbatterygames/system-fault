using UnityEngine;

//
// ShieldPannel.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: A projected shield which can be overloaded by the same damage type
//

public class ShieldPanel : MonoBehaviour, IDamageable {

    const float MAX_HEALTH = 150f;
    const float CHARGE_RATE = 50f;
    const float SAME_TYPE_MULTIPLIER = 5f;

    [SerializeField] GameTypes.DamageType shieldType;

    MeshRenderer meshRenderer;

    float currentHealth = MAX_HEALTH;
    float maxAlpha;

    public void Start() {
        meshRenderer = GetComponent<MeshRenderer>();
        maxAlpha = meshRenderer.material.color.a;
    }

    public void Damage(float amount, GameTypes.DamageType damageType, Vector3 damageForce, Vector3 pointOfImpact, Vector3 directionOfImpact) {
        float finalDamage = amount;

        if (damageType == shieldType) finalDamage *= SAME_TYPE_MULTIPLIER;

        currentHealth -= finalDamage;

        if (currentHealth <= 0f) Destroy(gameObject);

        UpdateOpacity(currentHealth / MAX_HEALTH);
    }

    void Update() {
        if (currentHealth != MAX_HEALTH) {
            currentHealth += CHARGE_RATE * Time.deltaTime;
            if (currentHealth > MAX_HEALTH) currentHealth = MAX_HEALTH;

            UpdateOpacity(currentHealth / MAX_HEALTH);
        }
    }


    void UpdateOpacity(float shieldPercentage) {
        meshRenderer.material.color = new Color(meshRenderer.material.color.r, meshRenderer.material.color.g, meshRenderer.material.color.b, shieldPercentage * maxAlpha);
    }
}

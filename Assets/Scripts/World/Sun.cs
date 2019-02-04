using UnityEngine;

//
// Sun.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Kills anything that comes close
//

public class Sun : MonoBehaviour {
    [SerializeField] float damagePerSecond = 10f;

    private void OnTriggerStay(Collider other) {
        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable != null) damageable.Damage(damagePerSecond * Time.deltaTime, GameTypes.DamageType.Physical, Vector3.zero);
    }
}

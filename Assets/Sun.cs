using UnityEngine;

//
// Sun.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Kills anything that comes close
//

public class Sun : MonoBehaviour
{
    private void OnTriggerEnter(Collider other) {
        IDamageable damageable = other.GetComponent<IDamageable>();
        if (damageable != null) damageable.Damage(Mathf.Infinity, GameTypes.DamageType.Physical, Vector3.zero);
    }
}

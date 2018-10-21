using UnityEngine;

//
// TreePlanter.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Planter object for PlanterEditor script
//

public class Planter : MonoBehaviour {
    public GameObject[] objectPrefabs;
    public float minimumScale = 1f;
    public float maximumScale = 1f;
    [Range(0f, 10f)] public float density = 5f;
    public LayerMask densityLayer;
}

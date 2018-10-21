using UnityEngine;

//
// PlayerData.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Holds information that should persist if the player is destroyed
//

public class PlayerData : MonoBehaviour {

    public static PlayerData instance = null;

    public GameObject dematerializedShipPrefab;
    public float shipTeleportTime = 2f;
    public float fallTolerance = 50f;

    [HideInInspector] public bool teleporting;

    public bool hasMatterManipulator;
    public bool hasMultiCannon;

    void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }
}

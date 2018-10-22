using UnityEngine;
using System.Collections.Generic;

//
// PlayerData.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Holds information that should persist if the player is destroyed
//

public class PlayerData : MonoBehaviour {

    public static PlayerData instance = null;

    [SerializeField] GameObject[] bulletPrefabs;

    public GameObject dematerializedShipPrefab;
    public float shipTeleportTime = 2f;
    public float shipDamageTolerance = 30f;
    public float fallTolerance = 50f;

    public bool hasMatterManipulator;
    public bool hasMultiCannon;

    [SerializeField] int maxBullets = 10;
    int[] bulletIndicies = new int[4];
    List<GameObject> pulseBullets;

    [HideInInspector] public bool alive;
    [HideInInspector] public bool teleporting;

    void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        pulseBullets = new List<GameObject>();
    }

    public GameObject GetBullet(GameTypes.DamageType bulletType) {
        switch (bulletType) {
            case GameTypes.DamageType.Pulse:
                if (pulseBullets.Count >= maxBullets) {
                    // Recycle bullet
                    int index = CycleBulletIndex(bulletType);
                    pulseBullets[index].GetComponent<Bullet>().RecycleBullet();
                    return pulseBullets[index];
                } else {
                    // Make new bullet
                    GameObject newBullet = Instantiate(bulletPrefabs[0]);
                    pulseBullets.Add(newBullet);
                    return newBullet;
                }
            default:
                Debug.LogError("PlayerData: Invalid bullet type requested");
                return Instantiate(bulletPrefabs[0]);
        }
    }

    int CycleBulletIndex(GameTypes.DamageType bulletType) {
        if (bulletIndicies[(int)bulletType] >= maxBullets - 1) bulletIndicies[(int)bulletType] = 0;
        else bulletIndicies[(int)bulletType]++;
        return bulletIndicies[(int)bulletType];
    }

    public void ClearBulletLists() {
        pulseBullets.Clear();

        bulletIndicies = new int[4];
    }
}

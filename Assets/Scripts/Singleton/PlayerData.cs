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

    [Header("Ship")]
    public GameObject dematerializedShipPrefab;
    public float shipTeleportTime = 3f;
    public float shipDamageTolerance = 30f;

    [Header("Input")]
    [Range(0.1f, 5f)] public float mouseForceSensitivity = 0.5f;
    [Range(0.1f, 5f)] public float lookSensitivity = 0.5f;

    [Header("Bullets")]
    public GameObject[] bulletPrefabs;
    [SerializeField] int maxBullets = 10;
    int[] bulletIndicies = new int[4];
    List<GameObject> yellowBullets = new List<GameObject>();
    List<GameObject> blueBullets = new List<GameObject>();
    List<GameObject> redBullets = new List<GameObject>();

    [HideInInspector] public bool alive;
    [HideInInspector] public bool teleportingShip;
    [HideInInspector] public bool hasMatterManipulator;
    [HideInInspector] public bool hasMulticannon;
    [HideInInspector] public bool blueUnlocked;
    [HideInInspector] public bool redUnlocked;

    void Awake() {
        if (instance == null) instance = this;
        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public bool IsTypeUnlocked(GameTypes.DamageType type) {
        switch (type) {
            case GameTypes.DamageType.Yellow:
                return true;
            case GameTypes.DamageType.Blue:
                return blueUnlocked;
            case GameTypes.DamageType.Red:
                return redUnlocked;
            default:
                return false;
        }
    }

    public GameObject GetBullet(GameTypes.DamageType bulletType) {
        int index = CycleBulletIndex(bulletType);

        switch (bulletType) {
            case GameTypes.DamageType.Yellow:
                if (yellowBullets.Count >= maxBullets) return yellowBullets[index];
                else {
                    // Make new bullet
                    GameObject newBullet = Instantiate(bulletPrefabs[0]);
                    yellowBullets.Add(newBullet);
                    return newBullet;
                }
            case GameTypes.DamageType.Blue:
                if (blueBullets.Count >= maxBullets) return blueBullets[index];
                else {
                    // Make new bullet
                    GameObject newBullet = Instantiate(bulletPrefabs[1]);
                    blueBullets.Add(newBullet);
                    GameManager.instance.AddGravityBody(newBullet.GetComponent<Rigidbody>());
                    return newBullet;
                }
            case GameTypes.DamageType.Red:
                if (redBullets.Count >= maxBullets) return redBullets[index];
                else {
                    // Make new bullet
                    GameObject newBullet = Instantiate(bulletPrefabs[2]);
                    redBullets.Add(newBullet);
                    return newBullet;
                }
            default:
                Debug.LogError("PlayerData: Invalid bullet type requested");
                return Instantiate(bulletPrefabs[0]);
        }
    }

    int CycleBulletIndex(GameTypes.DamageType bulletType) {
        int typeIndex = (int)bulletType;
        if (bulletIndicies[typeIndex] >= maxBullets - 1) bulletIndicies[typeIndex] = 0;
        else bulletIndicies[typeIndex]++;
        return bulletIndicies[typeIndex];
    }

    public void ClearBulletLists() {
        yellowBullets.Clear();
        blueBullets.Clear();
        redBullets.Clear();

        bulletIndicies = new int[4];
    }
}

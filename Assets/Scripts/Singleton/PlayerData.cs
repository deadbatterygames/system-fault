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
    public Material reservedSlotMaterial;

    [Header("Input")]
    [Range(0.1f, 5f)] public float mouseForceSensitivity = 0.5f;
    [Range(0.1f, 5f)] public float lookSensitivity = 0.5f;

    [Header("Bullets")]
    public GameObject[] bulletPrefabs;
    [SerializeField] int maxPlayerBullets = 10;
    [SerializeField] int maxXromBullets = 50;

    int[] bulletIndicies = new int[4];
    List<GameObject> yellowBullets = new List<GameObject>();
    List<GameObject> blueBullets = new List<GameObject>();
    List<GameObject> redBullets = new List<GameObject>();
    List<GameObject> xromBullets = new List<GameObject>();

    [Header("Particles")]
    public GameObject explosionPrefab;
    public GameObject laserParticles;

    [Header("Unlocks")]
    public bool hasMatterManipulator;
    public bool hasMulticannon;
    public bool blueUnlocked;
    public bool redUnlocked;

    [HideInInspector] public bool alive;
    [HideInInspector] public bool teleportingShip;

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
        int index = CycleBulletIndex(bulletType, maxPlayerBullets);

        switch (bulletType) {
            case GameTypes.DamageType.Yellow:
                if (yellowBullets.Count >= maxPlayerBullets) return yellowBullets[index];
                else {
                    GameObject newBullet = Instantiate(bulletPrefabs[0]);
                    yellowBullets.Add(newBullet);
                    return newBullet;
                }
            case GameTypes.DamageType.Blue:
                if (blueBullets.Count >= maxPlayerBullets) return blueBullets[index];
                else {
                    GameObject newBullet = Instantiate(bulletPrefabs[1]);
                    blueBullets.Add(newBullet);
                    GameManager.instance.AddGravityBody(newBullet.GetComponent<Rigidbody>()); // Blue bullets are affected by gravity
                    return newBullet;
                }
            case GameTypes.DamageType.Red:
                if (redBullets.Count >= maxPlayerBullets) return redBullets[index];
                else {
                    GameObject newBullet = Instantiate(bulletPrefabs[2]);
                    redBullets.Add(newBullet);
                    return newBullet;
                }
            case GameTypes.DamageType.Physical:
                index = CycleBulletIndex(bulletType, maxXromBullets); // Xrom bullets have a separate pool

                if (xromBullets.Count >= maxXromBullets) return xromBullets[index];
                else {
                    GameObject newBullet = Instantiate(bulletPrefabs[3]);
                    xromBullets.Add(newBullet);
                    return newBullet;
                }
            default:
                Debug.LogError("PlayerData: Invalid bullet type requested");
                return Instantiate(bulletPrefabs[0]);
        }
    }

    int CycleBulletIndex(GameTypes.DamageType bulletType, int maxBullets) {
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

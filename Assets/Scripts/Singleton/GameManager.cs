﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//
// GameManager.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Handles changes in the scene, such as AI clustering and gravity
//

public class GameManager : MonoBehaviour {

    public static GameManager instance = null;

    [HideInInspector] public List<Rigidbody> gravityBodies = new List<Rigidbody>();
    public const float GRAVITY_CONSTANT = 100000f;

    Transform playerSpawn;

    List<GameObject> earthInterestPoints = new List<GameObject>();
    List<GameObject> moonInterestPoints = new List<GameObject>();
    List<GameObject> gasInterestPoints = new List<GameObject>();
    List<GameObject> playerShipSpawns = new List<GameObject>();

    GravityWell currentWell = null;
    List<GravityWell> gravityWells = new List<GravityWell>();

    [Header("Prefabs")]
    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject playerShipPrefab;
    [SerializeField] GameObject startingZone;

    [Space]
    [SerializeField] GameObject crystalPrefab;
    [SerializeField] GameObject blueprintPrefab;

    [Header("Testing")]
    [SerializeField] GameTypes.SpawnLocation spawnLocation;
    [SerializeField] bool testMode;
    [SerializeField][Range(0,2)] int equipmentTier;

    public const float MAX_SHIP_SPEED = 300f;

    [HideInInspector] public Ship ship;
    PlayerCamera playerCam;


    void Awake() {
        if (instance == null) instance = this;

        else if (instance != this) Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    void Start() {
        if (!playerPrefab) Debug.LogError("GameManager: No player prefab set in inspector");

        playerCam = FindObjectOfType<PlayerCamera>();
        if (!playerCam) Debug.LogError("GameManager: No PlayerCam exists in the scene");

        // Interest Points
        moonInterestPoints.AddRange(GameObject.FindGameObjectsWithTag("IPMoon"));
        earthInterestPoints.AddRange(GameObject.FindGameObjectsWithTag("IPEarth"));
        gasInterestPoints.AddRange(GameObject.FindGameObjectsWithTag("IPGas"));
        playerShipSpawns.AddRange(GameObject.FindGameObjectsWithTag("PlayerShipSpawn"));

        if (moonInterestPoints.Count < PartPrinterData.MODULE_TYPES + 2) Debug.LogError("GameManager: Not enough Moon interest points");
        if (earthInterestPoints.Count < PartPrinterData.MODULE_TYPES + 2) Debug.LogError("GameManager: Not enough Earth interest points");
        if (gasInterestPoints.Count < PartPrinterData.MODULE_TYPES + 2) Debug.LogError("GameManager: Not enough Gas interest points");

        ShuffleInterestPoints();

        switch (spawnLocation) {
            case GameTypes.SpawnLocation.Moon: playerSpawn = moonInterestPoints[0].transform; break;
            case GameTypes.SpawnLocation.Earth: playerSpawn = earthInterestPoints[0].transform; break;
            case GameTypes.SpawnLocation.Gas: playerSpawn = gasInterestPoints[0].transform; break;
        }

        // Blueprints
        SpawnBlueprints(moonInterestPoints, 1);
        SpawnBlueprints(earthInterestPoints, 2);
        SpawnBlueprints(gasInterestPoints, 3);
        
        // Energy
        SpawnEnergy(earthInterestPoints);
        SpawnEnergy(moonInterestPoints);
        SpawnEnergy(gasInterestPoints);

        // Gravity
        gravityBodies.AddRange(FindObjectsOfType<Rigidbody>());
        gravityWells.AddRange(FindObjectsOfType<GravityWell>());
        currentWell = FindClosestWell();
        ChangeGravityWell(currentWell);

        // Starting Spawn
        SpawnPlayer(playerSpawn, Vector3.zero);
        if (testMode) {
            GameData.instance.hasMatterManipulator = true;
            GameData.instance.hasMulticannon = true;
            GameData.instance.blueUnlocked = true;
            GameData.instance.redUnlocked = true;
            PartPrinterData.instance.UnlockAllModules();

            GiveEquipment(PartPrinterData.instance.modulePrefabs[0 * PartPrinterData.MODULE_TIERS + equipmentTier]);
            GiveEquipment(PartPrinterData.instance.modulePrefabs[1 * PartPrinterData.MODULE_TIERS + equipmentTier], 5f);
            GiveEquipment(PartPrinterData.instance.modulePrefabs[2 * PartPrinterData.MODULE_TIERS + equipmentTier], 10f);
            GiveEquipment(PartPrinterData.instance.modulePrefabs[3 * PartPrinterData.MODULE_TIERS + equipmentTier], 15f);
            GiveEquipment(PartPrinterData.instance.modulePrefabs[4 * PartPrinterData.MODULE_TIERS + equipmentTier], 20f);
            GiveEquipment(PartPrinterData.instance.modulePrefabs[5 * PartPrinterData.MODULE_TIERS + equipmentTier], 25f);
            GiveEquipment(PartPrinterData.instance.modulePrefabs[5 * PartPrinterData.MODULE_TIERS + equipmentTier], 30f);
        } else {
            GameData.instance.hasMatterManipulator = false;
            GameData.instance.hasMulticannon = false;
            GameData.instance.blueUnlocked = false;
            GameData.instance.redUnlocked = false;
            PartPrinterData.instance.LockAllModules();
            GiveEquipment(startingZone);
            GiveEquipment(PartPrinterData.instance.modulePrefabs[0], 5f);
        }

        // Ship
        SpawnShip(playerShipSpawns[Random.Range(1, playerShipSpawns.Count)].transform);
        if (testMode) {
            ship.transform.position = playerSpawn.position + playerSpawn.forward * 20f + playerSpawn.up * 20f;
            ship.transform.rotation = playerSpawn.rotation;
        }

        // Camera
        PlayerCamera.instance.MoveCamToPlayer();

        // HUD
        PlayerHUD.instance.ClearHUD();
        PlayerHUD.instance.ToggleAllHelp();

        Time.timeScale = 1f;
    }

    public void ShuffleInterestPoints() {
        earthInterestPoints = ShuffleList(earthInterestPoints);
        moonInterestPoints = ShuffleList(moonInterestPoints);
        gasInterestPoints = ShuffleList(gasInterestPoints);
    }

    public List<GameObject> ShuffleList(List<GameObject> list) {
        for (int i = 0; i < list.Count; i++) {
            int rnd = Random.Range(i, list.Count);

            GameObject temp = list[i];
            list[i] = list[rnd];
            list[rnd] = temp;
        }

        return list;
    }

    public void SpawnBlueprints(List<GameObject> ipList, int tier) {
        for (int i = 1; i < PartPrinterData.MODULE_TYPES + 1; i++) {
            Blueprint bp = Instantiate(blueprintPrefab, ipList[i].transform.position +
                ipList[i].transform.up * 1.75f,
                ipList[i].transform.rotation,
                ipList[i].transform.parent).GetComponent<Blueprint>();

            if (i == 1 && tier != 3) bp.SetBlueprintType((GameTypes.ModuleType)i-1, tier+1);
            else bp.SetBlueprintType((GameTypes.ModuleType)i-1, tier);
        }
    }

    public void SpawnEnergy(List<GameObject> ipList) {
        for (int i = PartPrinterData.MODULE_TYPES + 1; i < ipList.Count; i++) {
            Instantiate(crystalPrefab, ipList[i].transform.position, ipList[i].transform.rotation, ipList[i].transform.parent);
        }
    }

    public void SpawnShip(Transform spawnPoint) {
        if (FindObjectOfType<Ship>() == null) {
            ship = Instantiate(playerShipPrefab, spawnPoint.position + spawnPoint.up, spawnPoint.rotation).GetComponent<Ship>();
            AddGravityBody(ship.GetComponent<Rigidbody>());
        }
    }

    public void SpawnPlayer(Transform spawnPoint, Vector3 spawnVelocity) {
        if (FindObjectOfType<Player>() == null) {
            GameObject player = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
            Rigidbody playerRB = player.GetComponent<Rigidbody>();
            AddGravityBody(playerRB);
            playerRB.velocity = spawnVelocity;
            PlayerControl.instance.TakeControl(playerRB.GetComponent<IControllable>());
        }
    }

    public void GiveEquipment(GameObject equipment, float spawnHeight = 0f) {
        Rigidbody rb = Instantiate(equipment, playerSpawn.position + playerSpawn.up * spawnHeight + playerSpawn.forward * 5f, playerSpawn.rotation, null).GetComponent<Rigidbody>();
        if (rb) AddGravityBody(rb);
    }

    public void DespawnPlayer() {
        Player player = FindObjectOfType<Player>();
        if (player) {
            Rigidbody playerRB = player.GetComponent<Rigidbody>();
            RemoveGravityBody(playerRB);
            FlockingController.DestroyAttractor(player.obstacle.attractor);
            FlockingController.DestroySeparator(player.obstacle.separator);
            Destroy(player.gameObject);
        }
    }

    public void AddGravityBody(Rigidbody rb) {
        gravityBodies.Add(rb);
    }

    public void RemoveGravityBody(Rigidbody rb) {
        gravityBodies.Remove(rb);
    }

    void FixedUpdate() {
        if (PlayerControl.instance.InControl() && FindClosestWell() != currentWell) ChangeGravityWell(FindClosestWell());
    }

    GravityWell FindClosestWell() {
        float minimumDistanceSqr = float.MaxValue;
        GravityWell closestWell = null;

        foreach (GravityWell well in gravityWells) {
            if (well) {
                float distanceSqr = (playerCam.transform.position - well.transform.position).sqrMagnitude;
                if (distanceSqr < minimumDistanceSqr) {
                    minimumDistanceSqr = distanceSqr;
                    closestWell = well;
                }
            }
        }

        return closestWell;
    }

    void ChangeGravityWell(GravityWell newWell) {
        Debug.Log("GameManager: Gravity well changed");
        foreach (GravityWell well in gravityWells) {
            if (well) {
                if (well != newWell) well.enabled = false;
                else {
                    well.enabled = true;
                    currentWell = well;
                }
            }
        }
    }

    public void ConnectAllShipModules() {
        FindObjectOfType<WeaponSlot>().matterManipulator.ConnectAllModules();
    }

    public IEnumerator PlayerDeath() {
        Time.timeScale = 0.25f;
        PlayerHUD.instance.ClearHUD();
        PlayerHUD.instance.ToggleShipRadar(false);
        PlayerHUD.instance.SetInfoPrompt("DEAD");

        yield return new WaitForSecondsRealtime(3f);

        earthInterestPoints.Clear();
        moonInterestPoints.Clear();
        gasInterestPoints.Clear();
        playerShipSpawns.Clear();

        gravityBodies.Clear();
        gravityWells.Clear();

        GameData.instance.ClearBulletLists();
        GameData.instance.hasMatterManipulator = false;
        GameData.instance.hasMulticannon = false;

        AsyncOperation reloadScene = SceneManager.LoadSceneAsync("Roguelike", LoadSceneMode.Single);
        while (!reloadScene.isDone) {
            PlayerHUD.instance.SetInfoPrompt("Loading...");
            Debug.LogWarning("GameManager: Loading...");
            yield return null;
        }

        Start();
    }

    public string GetModuleTypeString(GameTypes.ModuleType moduleType) {
        switch (moduleType) {
            case GameTypes.ModuleType.EnergyPack: return "Energy Pack";
            case GameTypes.ModuleType.Boosters: return "Boosters";
            case GameTypes.ModuleType.Thrusters: return "Thrusters";
            case GameTypes.ModuleType.QuantumDrive: return "Quantum Drive";
            case GameTypes.ModuleType.LaserCannon: return "Laser Cannon";
            case GameTypes.ModuleType.MissileRack: return "Missile Rack";
            default: return "Unknown module type";
        }
    }

    public bool IsInTestMode() {
        return testMode;
    }

    public void SetTestMode(bool set) {
        testMode = set;
    }
}

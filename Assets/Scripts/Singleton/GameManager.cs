using System.Collections;
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

    GravityWell currentWell = null;
    List<GravityWell> gravityWells = new List<GravityWell>();

    [SerializeField] GameObject playerPrefab;
    [SerializeField] GameObject xromPrefab;

    public const float MAX_PLAYER_SPEED = 300f;

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

        playerSpawn = GameObject.Find("PlayerSpawn").transform;
        if (!playerSpawn) Debug.LogError("GameManager: No PlayerSpawn exists in the scene");

        // Gravity
        gravityBodies.AddRange(FindObjectsOfType<Rigidbody>());
        gravityWells.AddRange(FindObjectsOfType<GravityWell>());
        currentWell = FindClosestWell();
        ChangeGravityWell(currentWell);

        // Player Spawn
        SpawnPlayer(playerSpawn, Vector3.zero);
        playerSpawn.gameObject.SetActive(false);
        PlayerCamera.instance.MoveCamToPlayer();

        // Xrom Spawns
        SpawnXroms();

        Time.timeScale = 1f;
    }

    public void SpawnPlayer(Transform spawnPoint, Vector3 spawnVelocity) {
        if (FindObjectOfType<Player>() == null) {
            Rigidbody playerRB = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation).GetComponent<Rigidbody>();
            AddGravityBody(playerRB);
            playerRB.velocity = spawnVelocity;
        }
    }

    public void SpawnXroms() {
        foreach (GameObject spawn in GameObject.FindGameObjectsWithTag("XromSpawn")) {
            AddGravityBody(Instantiate(xromPrefab, spawn.transform.position, spawn.transform.rotation).GetComponent<Rigidbody>());
            Destroy(spawn.GetComponentInChildren<MeshRenderer>());
        }
    }

    public void DespawnPlayer() {
        Player player = FindObjectOfType<Player>();
        if (player) {
            RemoveGravityBody(player.GetComponent<Rigidbody>());
            Destroy(player.gameObject);
        }
    }

    public void AddGravityBody(Rigidbody rb) {
        gravityBodies.Add(rb);
    }

    public void RemoveGravityBody(Rigidbody rb) {
        gravityBodies.Remove(rb);
    }

    // TODO: Remove
    public void Update() {
        if (Input.GetKeyDown(KeyCode.P)) {
            Player p = FindObjectOfType<Player>();
            if (p) p.Damage(21.276f, Vector3.zero);
        }
    }

    void FixedUpdate() {
        if (FindClosestWell() != currentWell) {
            ChangeGravityWell(FindClosestWell());
        }
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

    void ChangeGravityWell(GravityWell /*Gabe*/ newWell) {
        Debug.Log("GameManager: Gravity well changed");
        foreach (GravityWell well in gravityWells) {
            if (well != newWell) well.enabled = false;
            else {
                well.enabled = true;
                currentWell = well;
            }
        }
    }

    public IEnumerator ReloadScene() {
        yield return new WaitForSecondsRealtime(3f);

        gravityBodies.Clear();
        gravityWells.Clear();

        AsyncOperation reloadScene = SceneManager.LoadSceneAsync("TestScene");
        while (!reloadScene.isDone) {
            Debug.LogWarning("GameManager: Loading...");
            yield return null;
        }

        Resources.UnloadUnusedAssets();
        PlayerHUD.instance.ResetHUD();

        Start();
    }
}

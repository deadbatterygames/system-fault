using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//
// Shield.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Spawns shields
//

public class Shield : MonoBehaviour
{
    [SerializeField] float spawnTime = 1f;
    [SerializeField] GameObject[] panels;

    bool active = false;

    // Start is called before the first frame update
    void Start() {
        ActivateShield();
    }

    public void ActivateShield() {
        if (!active) StartCoroutine("SpawnPanels");
    }

    IEnumerator SpawnPanels() {
        active = true;

        foreach (GameObject panel in panels) {
            panel.SetActive(true);
            yield return new WaitForSeconds(spawnTime / panels.Length);
        }
    }
}

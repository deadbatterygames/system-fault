using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Overmind : MonoBehaviour
{
    [SerializeField] private GameObject xromFlockPrefab;
    private GameObject[] POI;
    [SerializeField] private bool flockingTest;
    // Start is called before the first frame update
    void Start()
    {
        int n = 5;
        int created = 0;

        POI = GameObject.FindGameObjectsWithTag("IPEarth");

        for(int i = 1; i < POI.Length; i++){
            float r = Random.Range(0.0f, 1.0f);
            if(r > (n / 5) / POI.Length && created < n){
                CreateFlock(true, false, POI[i].transform.position, 5);
                created += 5;
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static void AddAttractor(Attractor attractor){
        XromFlock[] xromFlocks = GameObject.FindObjectsOfType<XromFlock>();

        foreach(XromFlock flock in xromFlocks){
            flock.AddAttractor(attractor);
        }
    }

    private void CreateFlock(bool grounded, bool flying, Vector3 position, int boids){
        GameObject flockGO = GameObject.Instantiate(xromFlockPrefab, position, Quaternion.identity);
        XromFlock flock = flockGO.GetComponent<XromFlock>();

        

        flock.InitializeFlock(grounded, flying);

        for(int i = 0; i < boids; i++){
            flock.CreateXrom(grounded, flockingTest);
        }

        FlockingController.AddFlock(flock);
    }
}

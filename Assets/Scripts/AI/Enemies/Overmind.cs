using UnityEngine;

public class Overmind : MonoBehaviour
{
    public float movementScale;
    [SerializeField] private GameObject xromFlockPrefab;
    private GameObject[] POI;
    [SerializeField] private bool flockingTest;
    public static Overmind instance;

    void Start() {
        if(!instance) instance = this;
        else Destroy(this);
        
        int n = 5;
        int created = 0;

        Debug.Log("Overmind::Start ~ Making " + n + " boys");

        if(n >= 5){
            CreateFlock(true, true, GameObject.FindGameObjectWithTag("Player").transform.position, 5);
            created += 5;
        }

        POI = GameObject.FindGameObjectsWithTag("IPEarth");

        for(int i = 1; i < POI.Length; i++){
            float r = Random.Range(0.0f, 1.0f);
            if(r > (n / 5) / POI.Length && created < n){
                CreateFlock(true, false, POI[i].transform.position, 5);
                created += 5;
            }
        }
        
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

using UnityEngine;
using UnityEditor;

//
// TreeEditor.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Plants objects on the face of a plantable surface and orients them
//

[CustomEditor(typeof(Planter))]
public class ScenePlanter : Editor {

    Planter planter;
    GameObject lastPlanted;

    void OnEnable() {
        planter = (Planter)target;
        lastPlanted = null;
    }

    void OnSceneGUI() {
        Event e = Event.current;
        if (e.keyCode == KeyCode.L && e.type == EventType.KeyDown) Plant();
        if (e.keyCode == KeyCode.U && e.type == EventType.KeyDown) Undo();
        if (e.keyCode == KeyCode.R && e.type == EventType.KeyDown) RemoveLast();
    }

    void Undo(){
        if(lastPlanted){
            Destroy(lastPlanted);
        }
    }

    void RemoveLast(){
        lastPlanted = null;
    }

    void ConnectInhibitorStrip(FlockingObstacle previous, FlockingObstacle next){
        if(previous != null && next != null && previous.type == FlockingObstacle.ObstacleTypes.InhibitorStrip && next.type == FlockingObstacle.ObstacleTypes.InhibitorStrip){
            previous.next = next;
            next.previous = previous;
        }
    }

    void Plant() {
        RaycastHit hit;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.CompareTag("PlanetSurface")) {
                if (Physics.OverlapSphere(hit.point, 50f / planter.density, planter.densityLayer).Length == 0) {
                    GameObject thing = PrefabUtility.InstantiatePrefab(planter.objectPrefabs[Random.Range(0, planter.objectPrefabs.Length)]) as GameObject;

                    if(lastPlanted) ConnectInhibitorStrip(lastPlanted.GetComponent<FlockingObstacle>(), thing.GetComponent<FlockingObstacle>());

                    float scale = Random.Range(planter.minimumScale, planter.maximumScale);
                    thing.transform.position = hit.point;
                    thing.transform.parent = planter.transform;
                    thing.transform.localScale = new Vector3(scale, scale, scale);

                    lastPlanted = thing;
                }
            } else if (!hit.collider.CompareTag("Tree")) {
                Debug.LogWarning("ScenePlanter: Not a planet surface");
            }
        }
    }
}

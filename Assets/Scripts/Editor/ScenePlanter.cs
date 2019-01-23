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

    void OnEnable() {
        planter = (Planter)target;
    }

    void OnSceneGUI() {
        Event e = Event.current;
        if (e.keyCode == KeyCode.L && e.type == EventType.KeyDown) Plant();
    }

    void Plant() {
        RaycastHit hit;
        Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            if (hit.collider.CompareTag("PlanetSurface")) {
                if (Physics.OverlapSphere(hit.point, 50f / planter.density, planter.densityLayer).Length == 0) {
                    GameObject thing = PrefabUtility.InstantiatePrefab(planter.objectPrefabs[Random.Range(0, planter.objectPrefabs.Length)]) as GameObject;

                    float scale = Random.Range(planter.minimumScale, planter.maximumScale);
                    thing.transform.position = hit.point;
                    thing.transform.parent = planter.transform;
                    thing.transform.localScale = new Vector3(scale, scale, scale);
                }
            } else if (!hit.collider.CompareTag("Tree")) {
                Debug.LogWarning("ScenePlanter: Not a planet surface");
            }
        }
    }
}

using UnityEngine;

// This will eventually be controlled within the weapon

public class TempLightScript : MonoBehaviour {

    Light tempLight;

    void Start() {
        tempLight = GetComponent<Light>();
    }

    void Update () {
        if (Input.GetButtonDown("Light")) {
            if (tempLight.enabled) tempLight.enabled = false;
            else tempLight.enabled = true;
        }
	}
}

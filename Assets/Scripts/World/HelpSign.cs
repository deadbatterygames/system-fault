using UnityEngine;

//
// HelpSign.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Displays helpful information to the player
//

[RequireComponent(typeof(Canvas))]

public class HelpSign : MonoBehaviour
{
    Canvas canvas;
    Vector3 relativeUp;

    bool showCanvas;
    const float MAX_DISTANCE = 50f;

    void Awake() {
        relativeUp = transform.up;
        canvas = GetComponent<Canvas>();

        PlayerHUD.instance.AddHelpSign(this);
        ToggleCanvas(PlayerHUD.instance.showHelp);
    }

    void Update() {
        if ((PlayerCamera.instance.transform.position - transform.position).sqrMagnitude < MAX_DISTANCE * MAX_DISTANCE) {
            transform.LookAt(PlayerCamera.instance.transform, relativeUp);
            canvas.enabled = showCanvas;
        } else canvas.enabled = false;
    }

    public void ToggleCanvas(bool toggle) {
        canvas.enabled = toggle;
        showCanvas = toggle;
    }
}

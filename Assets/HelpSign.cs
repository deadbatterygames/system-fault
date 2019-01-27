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
    [HideInInspector] public Canvas canvas;
    Vector3 relativeUp;

    void Awake() {
        canvas = GetComponent<Canvas>();
        PlayerHUD.instance.AddHelpSign(this);
    }

    void Start() {
        if (!PlayerHUD.instance.showHelp) canvas.enabled = false;

        relativeUp = transform.up;
    }

    void Update() {
        transform.LookAt(PlayerCamera.instance.transform, relativeUp);
    }
}

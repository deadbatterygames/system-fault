using UnityEngine;

//
// XromLegs.cs
//
// Author: Eric Thompson (Dead Battery Games)
// Purpose: Moves Xrom legs
//

public class XromLegs : MonoBehaviour {

    [SerializeField] float animationSpeed = 20f;

    [SerializeField] Transform[] leftLegTransforms;
    [SerializeField] Transform[] rightLegTransforms;

    [SerializeField] Transform[] legUpTransforms;
    [SerializeField] Transform[] legForwardTransforms;
    [SerializeField] Transform[] legBackTransforms;

    Quaternion[] legIdleRotations;
    Quaternion[] legUpRotations;
    Quaternion[] legForwardRotations;
    Quaternion[] legBackRotations;

    enum LegPosition {
        Idle,
        Up,
        Forward,
        Back
    };

    LegPosition leftLegPosition = LegPosition.Idle;

    XromWalker walker;

    void Start () {
        walker = GetComponentInParent<XromWalker>();

        legIdleRotations = new Quaternion[leftLegTransforms.Length];
        legUpRotations = new Quaternion[leftLegTransforms.Length];
        legForwardRotations = new Quaternion[leftLegTransforms.Length];
        legBackRotations = new Quaternion[leftLegTransforms.Length];
        for (int i = 0; i < leftLegTransforms.Length; i++) {
            legIdleRotations[i] = leftLegTransforms[i].localRotation;
            legUpRotations[i] = legUpTransforms[i].localRotation;
            legForwardRotations[i] = legForwardTransforms[i].localRotation;
            legBackRotations[i] = legBackTransforms[i].localRotation;
        }
    }
	
	void Update () {
        if (walker && walker.IsXromActive()) Walk();
	}

    void Walk() {
        bool leftLegComplete;
        bool rightLegComplete;

        switch (leftLegPosition) {
            case LegPosition.Idle:
                leftLegComplete = MoveLeg(leftLegTransforms, legIdleRotations);
                rightLegComplete = MoveLeg(rightLegTransforms, legUpRotations);
                if (leftLegComplete && rightLegComplete) {
                    leftLegPosition = LegPosition.Back;
                    //rightLegPosition = LegPosition.Forward;
                }
                break;
            case LegPosition.Back:
                leftLegComplete = MoveLeg(leftLegTransforms, legBackRotations);
                rightLegComplete = MoveLeg(rightLegTransforms, legForwardRotations);
                if (leftLegComplete && rightLegComplete) {
                    leftLegPosition = LegPosition.Up;
                    //rightLegPosition = LegPosition.Idle;
                }
                break;
            case LegPosition.Up:
                leftLegComplete = MoveLeg(leftLegTransforms, legUpRotations);
                rightLegComplete = MoveLeg(rightLegTransforms, legIdleRotations);
                if (leftLegComplete && rightLegComplete) {
                    leftLegPosition = LegPosition.Forward;
                    //rightLegPosition = LegPosition.Back;
                }
                break;
            case LegPosition.Forward:
                leftLegComplete = MoveLeg(leftLegTransforms, legForwardRotations);
                rightLegComplete = MoveLeg(rightLegTransforms, legBackRotations);
                if (leftLegComplete && rightLegComplete) {
                    leftLegPosition = LegPosition.Idle;
                    //rightLegPosition = LegPosition.Up;
                }
                break;
        }
    }

    bool MoveLeg(Transform[] leg, Quaternion[] desiredRotations) {
        bool complete = true;

        for (int i = 0; i < leg.Length; i++) {
            leg[i].transform.localRotation = Quaternion.Lerp(leg[i].transform.localRotation, desiredRotations[i], animationSpeed * Time.deltaTime);

            if (Quaternion.Angle(leg[i].transform.localRotation, desiredRotations[i]) > 1f) complete = false;
        }

        return complete;
    }
}

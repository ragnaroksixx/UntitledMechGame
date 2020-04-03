using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;
public class FlightStickController : MonoBehaviour
{
    public Flightstick leftJoysticks, rightJoysticks;
    public MechController mech;
    float maxRetRotX = 20, maxRetRotY = 10;
    public Transform reticleROtator;
    public enum MechAction
    {
        positive,
        negative,
        none
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        float magMove = 0;
        float magRot = 0;
        MechAction moveAction = GetMoveAction(out magMove);
        MechAction rotAction = GetRotAction(out magRot);

        switch (moveAction)
        {
            case MechAction.positive:
                mech.Move(1, magMove);
                break;
            case MechAction.negative:
                mech.Move(-1, magMove);
                break;
            case MechAction.none:
                break;
            default:
                break;
        }
        switch (rotAction)
        {
            case MechAction.positive:
                mech.Steer(1, 1);
                break;
            case MechAction.negative:
                mech.Steer(-1, 1);
                break;
            case MechAction.none:
                mech.Steer(0, 0);
                break;
            default:
                break;
        }
        float x = 0;
        if (leftJoysticks.BeingHeld)
            x += InputBridge.instance.LeftThumbstickAxis.x;
        if (rightJoysticks.BeingHeld)
            x += InputBridge.instance.RightThumbstickAxis.x;

        float y = 0;
        if (leftJoysticks.BeingHeld)
            y += InputBridge.instance.LeftThumbstickAxis.y;
        if (rightJoysticks.BeingHeld)
            y += InputBridge.instance.RightThumbstickAxis.y;

        x /= 2;
        y /= 2;
        reticleROtator.transform.localEulerAngles = new Vector3(-y * maxRetRotY, x * maxRetRotX, 0);
    }

    public MechAction GetMoveAction(out float mag)
    {
        float left = leftJoysticks.LeverPercentageX;
        float right = rightJoysticks.LeverPercentageX;
        float total = left + right;
        float forwardVal = 30;
        float backValue = -30;
        MechAction action = MechAction.none;
        mag = 0;
        bool isForward = total > forwardVal;
        bool isBack = total < backValue;
        if (isForward && !isBack)
        {
            action = MechAction.positive;
            mag = total / 100.0f;
        }
        else if (isBack && !isForward)
        {
            action = MechAction.negative;
            mag = total / 100.0f;
        }
        return action;
    }
    public MechAction GetRotAction(out float mag)
    {
        float left = leftJoysticks.LeverPercentageZ;
        float right = rightJoysticks.LeverPercentageZ;
        float total = left + right;
        float clockRot = 50;
        float counterclockROt = -50;
        MechAction action = MechAction.none;
        mag = 0;
        bool clockwise = total > clockRot;
        bool counterclockwise = total < counterclockROt;
        if (clockwise && !counterclockwise)
        {
            action = MechAction.positive;
            mag = total / 100.0f;
        }
        else if (counterclockwise && !clockwise)
        {
            mag = total / 100.0f;
            action = MechAction.negative;
        }
        return action;
    }

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;
public class FlightStickController : MonoBehaviour
{
    public Flightstick leftJoysticks, rightJoysticks;
    public MechController mech;
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
    }

    /* public MechAction GetMoveAction()
     {
         float left = leftJoysticks.LeverPercentageX;
         float right = rightJoysticks.LeverPercentageX;
         float forwardVal = 70;
         float backValue = 30;
         MechAction action = MechAction.none;
         if (left > forwardVal && right > forwardVal)
             action = MechAction.positive;
         else if (left < backValue && right < backValue)
             action = MechAction.negative;
         return action;
     }
     public MechAction GetRotAction()
     {
         float left = leftJoysticks.LeverPercentageZ;
         float right = rightJoysticks.LeverPercentageZ;
         float leftPosVal = 70;
         float rightPosVal=30;
         MechAction action = MechAction.none;
         if (left < leftPosVal && right < rightPosVal)
             action = MechAction.positive;
         else if (left > leftPosVal && right > rightPosVal)
             action = MechAction.negative;
         return action;
     }*/
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

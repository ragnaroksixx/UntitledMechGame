using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BNG;
using UnityEngine.UI;

public class Flightstick : Grabbable
{
    public Transform graphic;
    Transform controller = null;
    Quaternion ogQuat;
    Vector3 ogEuler;
    Vector3 x;
    Vector3 y;
    /// <summary>
    /// Minimum angle the Level can be rotated
    /// </summary>
    public float MinDegrees = -45f;

    /// <summary>
    /// Maximum angle the Level can be rotated
    /// </summary>
    public float MaxDegrees = 45f;

    /// <summary>
    /// Current Percentage of joystick on X axis (left / right)
    /// </summary>
    public float LeverPercentageX = 0;

    /// <summary>
    /// Current Percentage of joystick on Z axis (forward / back)
    /// </summary>
    public float LeverPercentageZ = 0;
    /// <summary>
    /// Event called when Joystick value is changed
    /// </summary>
    public FloatFloatEvent onJoystickChange;

    // Keep track of Joystick Rotation
    Vector3 currentRotation;
    float angleX;
    float angleZ;
    public Transform baseRef;
    public override void Awake()
    {
        base.Awake();
        ogQuat = graphic.localRotation;
        ogEuler = graphic.localEulerAngles;

    }
    private void Update()
    {
        if (controller)
        {
            graphic.up = controller.up;

            x = baseRef.forward;
            y = baseRef.right;
            float xCom = (Vector3.Dot(graphic.up, x) ) ;
            float yCom = (Vector3.Dot(graphic.up, y) ) ;

            LeverPercentageX = xCom * 100;
            LeverPercentageZ = yCom * 100;

            // Lever value changed event
            OnJoystickChange(LeverPercentageX, LeverPercentageZ);
            //transform.up = graphic.up;

        }
    }
    // Callback for lever percentage change
    public virtual void OnJoystickChange(float leverX, float leverY)
    {
        if (onJoystickChange != null)
        {
            onJoystickChange.Invoke(leverX, leverY);
        }
    }
    public override void GrabItem(Grabber grabbedBy)
    {
        controller = grabbedBy.transform;
        base.GrabItem(grabbedBy);
    }

    public override void DropItem(Grabber droppedBy)
    {
        base.DropItem(droppedBy);
        resetStuff();
    }
    public override void DropItem(Grabber droppedBy, bool resetVelocity, bool resetParent)
    {
        base.DropItem(droppedBy, resetVelocity, resetParent);
        resetStuff();
    }
    void resetStuff()
    {
        controller = null;
        graphic.localRotation = ogQuat;
        LeverPercentageX = 0;
        LeverPercentageZ = 0;
    }
}

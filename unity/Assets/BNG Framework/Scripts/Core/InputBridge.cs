using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    public enum ControllerHand {
        Left,
        Right,
        None
    }

    public enum HandControl {
        LeftGrip,
        RightGrip,
        LeftTrigger,
        RightTrigger,
        None
    }

    public enum GrabButton {
        Grip,
        Trigger        
    }

    /// <summary>
    /// A proxy for handling input from Oculus. 
    /// </summary>
    public class InputBridge : MonoBehaviour {

        /// <summary>
        /// How far Left Grip is Held down. Values : 0 - 1 (Fully Open / Closed)
        /// </summary>
        public float LeftGrip = 0;

        /// <summary>
        /// Left Grip was pressed down this drame, but not last
        /// </summary>
        public bool LeftGripDown = false;

        /// <summary>
        /// How far Right Grip is Held down. Values : 0 - 1 (Fully Open / Closed)
        /// </summary>
        public float RightGrip = 0;

        /// <summary>
        /// Right Grip was pressed down this drame, but not last
        /// </summary>
        public bool RightGripDown = false;

        public float LeftTrigger = 0;
        public bool LeftTriggerNear = false;
        public bool LeftTriggerDown = false;
        public float RightTrigger = 0;
        public bool RightTriggerDown = false;
        public bool RightTriggerNear = false;

        public bool LeftThumbNear = false;
        public bool RightThumbNear = false;

        // Return true during frame
        public bool LeftThumbstickDown = false;
        public bool RightThumbstickDown = false;
        
        // Oculus Touch Controllers
        public bool AButton = false;
        public bool AButtonDown = false;
        public bool BButton = false;
        public bool BButtonDown = false;
        public bool XButton = false;
        public bool XButtonDown = false;
        public bool YButton = false;
        public bool YButtonDown = false;

        public bool StartButton = false;
        public bool StartButtonDown = false;
        public bool BackButton = false;
        public bool BackButtonDown = false;

        public Vector2 LeftThumbstickAxis;
        public Vector2 RightThumbstickAxis;

        // Update is called once per frame
        void Update() {
#if SDK_OCULUS
            // 
#endif
            LeftGrip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.LTouch);
            LeftGripDown = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.LTouch);

            RightGrip = OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger, OVRInput.Controller.RTouch);
            RightGripDown = OVRInput.GetDown(OVRInput.Button.PrimaryHandTrigger, OVRInput.Controller.RTouch);

            LeftTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.LTouch);
            LeftTriggerDown = OVRInput.GetDown(OVRInput.Button.PrimaryIndexTrigger);
            LeftTriggerNear = OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, OVRInput.Controller.LTouch);

            LeftThumbNear = OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, OVRInput.Controller.LTouch);
            RightThumbNear = OVRInput.Get(OVRInput.NearTouch.PrimaryThumbButtons, OVRInput.Controller.RTouch);

            RightTrigger = OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, OVRInput.Controller.RTouch);
            RightTriggerDown = OVRInput.GetDown(OVRInput.Button.SecondaryIndexTrigger);
            RightTriggerNear = OVRInput.Get(OVRInput.NearTouch.PrimaryIndexTrigger, OVRInput.Controller.RTouch);

            AButton = OVRInput.Get(OVRInput.Button.One, OVRInput.Controller.RTouch);
            AButtonDown = OVRInput.GetDown(OVRInput.Button.One, OVRInput.Controller.RTouch);            

            BButton = OVRInput.Get(OVRInput.Button.Two);
            BButtonDown = OVRInput.GetDown(OVRInput.Button.Two);
            XButton = OVRInput.Get(OVRInput.Button.Three);
            XButtonDown = OVRInput.GetDown(OVRInput.Button.Three);
            YButton = OVRInput.Get(OVRInput.Button.Four);            
            YButtonDown = OVRInput.GetDown(OVRInput.Button.Four);            

            StartButton = OVRInput.Get(OVRInput.Button.Start);
            StartButtonDown = OVRInput.GetDown(OVRInput.Button.Start);

            BackButton = OVRInput.Get(OVRInput.Button.Back);
            BackButtonDown = OVRInput.GetDown(OVRInput.Button.Back);

            LeftThumbstickAxis = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);
            RightThumbstickAxis = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick);

            LeftThumbstickDown = OVRInput.GetDown(OVRInput.Button.PrimaryThumbstick);
            RightThumbstickDown = OVRInput.GetDown(OVRInput.Button.SecondaryThumbstick);
        }

        // Start Vibration on controller
        public void VibrateController(float frequency, float amplitude, float duration, ControllerHand hand) {
            StartCoroutine(Vibrate(frequency, amplitude, duration, hand));
        }

        IEnumerator Vibrate(float frequency, float amplitude, float duration, ControllerHand hand) {
            // Start vibration
            if (hand == ControllerHand.Right) {
                OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.RTouch);
            }
            else if (hand == ControllerHand.Left) {
                OVRInput.SetControllerVibration(frequency, amplitude, OVRInput.Controller.LTouch);
            }

            yield return new WaitForSeconds(duration);

            // Stop vibration
            if (hand == ControllerHand.Right) {
                OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.RTouch);
            }
            else if (hand == ControllerHand.Left) {
                OVRInput.SetControllerVibration(0, 0, OVRInput.Controller.LTouch);
            }
        }
    }
}


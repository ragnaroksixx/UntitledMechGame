using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BNG {

    /// <summary>
    /// Contains various functionality used in the Demo Scene such as  Switching Hands and Locomotion
    /// </summary>
    public class DemoScript : MonoBehaviour {

        /// <summary>
        /// Child index of the hand model to use if nothing stored in playerprefs and LoadHandSelectionFromPrefs true
        /// </summary>
        public int DefaultHandsModel = 1;

        /// <summary>
        /// If true, hand model will be saved and loaded from player prefs
        /// </summary>
        public bool LoadHandSelectionFromPrefs = true;

        /// <summary>
        /// Default locomotion to use if nothing stored in playerprefs. 0 = Teleport. 1 = SmoothLocomotion
        /// </summary>
        public float DefaultLocomotion = 0;

        /// <summary>
        /// If true, locomotion type will be saved and loaded from player prefs
        /// </summary>
        public bool LoadLocomotionFromPrefs = true;

        /// <summary>
        /// Grabber attached to Left Controller
        /// </summary>
        public Grabber LeftGrabber;

        /// <summary>
        /// Grabber attached to Right Controller
        /// </summary>
        public Grabber RightGrabber;

        /// <summary>
        /// This transform holds all of the hand models. Can be used to enabled / disabled various hand options
        /// </summary>
        public Transform LeftHandGFXHolder;

        /// <summary>
        /// This transform holds all of the hand models. Can be used to enabled / disabled various hand options
        /// </summary>
        public Transform RightHandGFXHolder;
        private int _selectedHandGFX = 0;

        /// <summary>
        /// GameObject used to show Debug Info in-game
        /// </summary>
        public GameObject DebugMenu;
        public Text LabelToUpdate;

        /// <summary>
        /// Used in demo scene
        /// </summary>
        public Text JoystickText;

        InputBridge input;
        BNGPlayerController player;
        PlayerTeleport teleport;

        /// <summary>
        /// Used for demo scene IK Hands / Body
        /// </summary>
        public CharacterIK IKBody;

        /// <summary>
        /// Used in demo scene to spawn ammo clips
        /// </summary>
        public GameObject AmmoObject;

        /// <summary>
        /// This is the start point of a line for UI purposes. We may want to move this around if we change models or controllers.        
        /// </summary>
        UIPointer uiPoint;

        /// <summary>
        /// Holds all of the grabbable objects in the scene
        /// </summary>
        public Transform ItemsHolder;

        Dictionary<Grabbable, PosRot> _initalGrabbables;

        
        // Start is called before the first frame update
        void Start() {
            input = GetComponent<InputBridge>();
            player = GetComponent<BNGPlayerController>();
            teleport = GetComponent<PlayerTeleport>();
            uiPoint = GetComponentInChildren<UIPointer>();

            // Load new Hands. Default to White hands (3rd child child)
            if (LoadHandSelectionFromPrefs) {
                ChangeHandsModel(PlayerPrefs.GetInt("HandSelection", DefaultHandsModel), false);
            }

            // Load Locomotion Preference
            if (LoadLocomotionFromPrefs) {
                ChangeLocomotion(PlayerPrefs.GetInt("LocomotionSelection", 0), false);
            }

            VRUtils.Instance.Log("Output text here by using VRUtils.Log(\"Message Here\");");
            VRUtils.Instance.Log("Click the Menu button to toggle this menu.");

            // Set up initial grabbables so we can reset them later
            if(ItemsHolder) {
                _initalGrabbables = new Dictionary<Grabbable, PosRot>();
                var allGrabs = ItemsHolder.GetComponentsInChildren<Grabbable>();
                foreach(var grab in allGrabs) {
                    _initalGrabbables.Add(grab, new PosRot() { Position = grab.transform.position, Rotation = grab.transform.rotation });
                }
            }
        }

        // Some example controls useful for testing
        void Update() {

            // Toggle Locomotion by pressing left thumbstick down
            if(input.LeftThumbstickDown) {
                ChangeLocomotion(player.SelectedLocomotion == LocomotionType.SmoothLocomotion ? 0 : 1, LoadLocomotionFromPrefs);
            }

            teleport.enabled = player.SelectedLocomotion == LocomotionType.Teleport;

            // Cycle through hand models with Right Thumbstick
            if (input.RightThumbstickDown || Input.GetKeyDown(KeyCode.N)) {
                ChangeHandsModel(_selectedHandGFX + 1, LoadLocomotionFromPrefs);
            }

            // Toggle Debug Menu by pressing menu button
            if (input.StartButtonDown) {
                DebugMenu.SetActive(!DebugMenu.activeSelf);
            }
        }

        public void ChangeHandsModel(int childIndex, bool save = false) {

            // Deactivate Old
            LeftHandGFXHolder.GetChild(_selectedHandGFX).gameObject.SetActive(false);
            RightHandGFXHolder.GetChild(_selectedHandGFX).gameObject.SetActive(false);

            // Loop back to beginning if we went over
            _selectedHandGFX = childIndex;
            if (_selectedHandGFX > LeftHandGFXHolder.childCount - 1) {
                _selectedHandGFX = 0;
            }

            // Activate New
            GameObject leftHand = LeftHandGFXHolder.GetChild(_selectedHandGFX).gameObject;
            GameObject rightHand = RightHandGFXHolder.GetChild(_selectedHandGFX).gameObject;

            leftHand.SetActive(true);
            rightHand.SetActive(true);

            // Update any animators
            HandController leftControl = LeftHandGFXHolder.parent.GetComponent<HandController>();
            HandController rightControl = RightHandGFXHolder.parent.GetComponent<HandController>();
            if(leftControl && rightControl) {
                leftControl.HandAnimator = leftHand.GetComponentInChildren<Animator>();
                rightControl.HandAnimator = rightHand.GetComponentInChildren<Animator>();
            }

            // Enable / Disable IK Character. For demo purposes only
            if(IKBody != null) {
                IKBody.gameObject.SetActive(leftHand.transform.name.Contains("IK"));
            }

            // Change UI Pointer position depending on if we're using Oculus Hands or Oculus Controller Model
            // This is for the demo. Typically this would be fixed to a bone or transform
            // Oculus Touch Controller is positioned near the front
            if(_selectedHandGFX == 0 && uiPoint != null) {
                uiPoint.PointerObject.localPosition = new Vector3(0, 0, 0.0462f);
                uiPoint.PointerObject.localEulerAngles = new Vector3(0, -4.5f, 0);
            }
            // Hand Model
            else if(_selectedHandGFX != 0 && uiPoint != null) {
                uiPoint.PointerObject.localPosition = new Vector3(0.045f, 0.07f, 0.12f);
                uiPoint.PointerObject.localEulerAngles = new Vector3(-9.125f, 4.65f, 0);
            }

            if (save) {
                PlayerPrefs.SetInt("HandSelection", _selectedHandGFX);
            }
        }

        public void ChangeLocomotion(int locomotionType, bool save) {
            if(locomotionType == 0) {
                player.ChangeLocomotionType(LocomotionType.Teleport);
            }
            else if(locomotionType == 1) {
                player.ChangeLocomotionType(LocomotionType.SmoothLocomotion);
            }

            if (save) {
                PlayerPrefs.SetInt("LocomotionSelection", locomotionType);
            }
        }

        public void UpdateSliderText(float sliderValue) {
            if (LabelToUpdate != null) {
                LabelToUpdate.text = (int)sliderValue + "%";
            }
        }

        public void UpdateJoystickText(float leverX, float leverY) {
            if (JoystickText != null) {
                JoystickText.text = "X : " + (int)leverX + "\nY: " + (int)leverY;
            }
        }

        public void ResetGrabbables() {
            foreach (var kvp in _initalGrabbables) {

                // Only reset high level grabbables
                if(kvp.Key != null && kvp.Key.transform.parent == ItemsHolder) {
                    kvp.Key.transform.position = kvp.Value.Position;
                    kvp.Key.transform.rotation = kvp.Value.Rotation;
                }
            }
        }

        List<Grabbable> demoClips;
        public void GrabAmmo(Grabber grabber) {

            if(demoClips == null) {
                demoClips = new List<Grabbable>();
            }

            if(demoClips.Count > 0 && demoClips[0] == null) {
                demoClips.RemoveAt(0);
            }

            if(AmmoObject != null) {

                // Make room for new clip. This ensures the demo doesn't ge bogged down
                if(demoClips.Count > 4 && demoClips[0] != null && demoClips[0].transform.parent == null) {
                    GameObject.Destroy(demoClips[0].gameObject);
                }

                GameObject ammo = Instantiate(AmmoObject, grabber.transform.position, grabber.transform.rotation) as GameObject;
                Grabbable g = ammo.GetComponent<Grabbable>();

                // Disable rings for performance
                GrabbableRingHelper grh = ammo.GetComponentInChildren<GrabbableRingHelper>();
                Destroy(grh);
                RingHelper r = ammo.GetComponentInChildren<RingHelper>();
                Destroy(r.gameObject);

                // Offset to hand
                ammo.transform.parent = grabber.transform;
                ammo.transform.localPosition = -g.GrabPositionOffset;
                ammo.transform.parent = null;

                if(g != null) {
                    demoClips.Add(g);
                }

                grabber.GrabGrabbable(g);
            }
        }
    }

    public class PosRot {
        public Vector3 Position;
        public Quaternion Rotation;
    }
}


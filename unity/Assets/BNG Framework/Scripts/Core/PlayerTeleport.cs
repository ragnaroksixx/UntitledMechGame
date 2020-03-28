using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    public enum TeleportControls {
        Thumbstick,
        BButton
    }

    /// <summary>
    /// A basic Teleport script that uses a parabolic arc to determine teleport location
    /// </summary>
    public class PlayerTeleport : MonoBehaviour {

        public Color ValidTeleport = Color.green;
        public Color InvalidTeleport = Color.red;

        public Grabber LeftHandGrabber;
        public Grabber RightHandGrabber;

        public Transform TeleportBeginTransform;
        public Transform TeleportDestination;
        public GameObject TeleportMarker;
        public Transform DirectionIndicator;

        public float MaxRange = 20f;
        public int SegmentCount = 100; // More segments means a smoother line, at the cost of performance
        public float simulationVelocity = 500f;
        public float segmentScale = 0.5f;
        public LayerMask CollisionLayers;
        public LayerMask ValidLayers;

        public TeleportControls ControlType = TeleportControls.Thumbstick;
        public bool AllowTeleportRotation = true;
        private bool _reachThumbThreshold = false;
        public float MaxSlope = 60f;

        CharacterController controller;
        BNGPlayerController playerController;
        InputBridge input;
        Transform cameraRig;

        bool aimingTeleport = false;
        bool validTeleport = false;
        bool teleportationEnabled = true;
        public LineRenderer TeleportLine;

        void Start() {
            input = GetComponent<InputBridge>();
            playerController = GetComponent<BNGPlayerController>();
            controller = GetComponentInChildren<CharacterController>();
            cameraRig = GetComponentInChildren<OVRCameraRig>().transform;

            // Make sure teleport line is a root object
            if (TeleportLine != null) {
                TeleportLine.transform.parent = null;
            }

            if (CollisionLayers == 0) {
                Debug.Log("Teleport layer not set. Setting Default.");
                CollisionLayers = 1;
            }
        }

        void LateUpdate() {

            // Are we pressing button to check for teleport?
            aimingTeleport = keyDownForTeleport();

            if (aimingTeleport) {
                playerController.LastTeleportTime = Time.time;
                updateTeleport();
            }
            // released key, finish teleport or just hide graphics
            else if (keyUpFromTeleport()) {
                if (validTeleport) {
                    tryTeleport();
                }
                else {
                    hideTeleport();
                }
            }
        }        

        void FixedUpdate() {
            if (aimingTeleport) {
                calculateParabola();
            }
        }
        public void EnableTeleportation() {
            teleportationEnabled = true;
        }

        public void DisableTeleportation() {
            teleportationEnabled = false;
            validTeleport = false;
            aimingTeleport = false;

            hideTeleport();
        }

        // gameobject we're actually pointing at (may be useful for highlighting a target, etc.)
        Collider _hitObject;
        private Vector3 _hitVector;
        float _hitAngle;

        void calculateParabola() {

            validTeleport = false;
            bool isDestination = false;

            Vector3[] segments = new Vector3[SegmentCount];

            segments[0] = TeleportBeginTransform.position;
            // Initial velocity
            Vector3 segVelocity = TeleportBeginTransform.forward * simulationVelocity * Time.deltaTime;

            _hitObject = null;

            for (int i = 1; i < SegmentCount; i++) {

                // Hit something, so assign all future segments to this segment
                if (_hitObject != null) {
                    segments[i] = _hitVector;
                    continue;
                }

                // Time it takes to traverse one segment of length segScale (careful if velocity is zero)
                float segTime = (segVelocity.sqrMagnitude != 0) ? segmentScale / segVelocity.magnitude : 0;

                // Add velocity from gravity for this segment's timestep
                segVelocity = segVelocity + Physics.gravity * segTime;

                // Check to see if we're going to hit a physics object
                RaycastHit hit;
                if (Physics.Raycast(segments[i - 1], segVelocity, out hit, segmentScale, CollisionLayers)) {

                    // remember who we hit
                    _hitObject = hit.collider;
                    // set next position to the position where we hit the physics object
                    segments[i] = segments[i - 1] + segVelocity.normalized * hit.distance;
                    // correct ending velocity, since we didn't actually travel an entire segment
                    segVelocity = segVelocity - Physics.gravity * (segmentScale - hit.distance) / segVelocity.magnitude;
                    // flip the velocity to simulate a bounce
                    segVelocity = Vector3.Reflect(segVelocity, hit.normal);

                    _hitAngle = Vector3.Angle(transform.up, hit.normal);

                    // Align marker to hit normal
                    TeleportMarker.transform.position = segments[i]; // hit.point;
                    TeleportMarker.transform.rotation = Quaternion.FromToRotation(TeleportMarker.transform.up, hit.normal) * TeleportMarker.transform.rotation;

                    // Snap to Teleport Destination
                    TeleportDestination td = _hitObject.GetComponent<TeleportDestination>();
                    if(td != null) {
                        isDestination = true;
                        TeleportMarker.transform.position = td.transform.position;
                        TeleportMarker.transform.rotation = td.transform.rotation;
                    }

                    _hitVector = segments[i];

                }
                // If our raycast hit no objects, then set the next position to the last one plus v*t
                else {
                    segments[i] = segments[i - 1] + segVelocity * segTime;
                }
            }

            validTeleport = _hitObject != null;

            // Make sure teleport location is valid
            // Destination Targets ignore checks
            if(validTeleport && !isDestination) {

                // Angle too steep
                if (_hitAngle > MaxSlope) {
                    validTeleport = false;
                }

                // Hit a grabbable object
                if (_hitObject.GetComponent<Grabbable>() != null) {
                    validTeleport = false;
                }

                // Hit a restricted zone
                if (_hitObject.GetComponent<InvalidTeleportArea>() != null) {
                    validTeleport = false;
                }

                // Something in the way via raycast
                if (!teleportClear() ) {
                    validTeleport = false;
                }
            }

            // Render the positions as a line
            TeleportLine.positionCount = SegmentCount;
            for (int i = 0; i < SegmentCount; i++) {
                TeleportLine.SetPosition(i, segments[i]);
            }
        }

        // Clear of obstacles
        bool teleportClear() {

            // Something in the way via overlap sphere. Uses player capsule radius.
            Collider[] hitColliders = Physics.OverlapSphere(TeleportDestination.position, controller.radius, CollisionLayers, QueryTriggerInteraction.Ignore);
            if (hitColliders.Length > 0) {
                return false;
            }

            // Something in the way via Raycast up from teleport spot
            // Raycast from the ground up to the height of character controller
            RaycastHit hit;
            if (Physics.Raycast(TeleportMarker.transform.position, TeleportMarker.transform.up, out hit, controller.height, CollisionLayers, QueryTriggerInteraction.Ignore)) {
                return false;
            }

            // Invalid Layer
            bool validLayer = ValidLayers == (ValidLayers | (1 << _hitObject.gameObject.layer));
            return validLayer;
        }

        void hideTeleport() {
            TeleportLine.enabled = false;

            if(TeleportMarker.activeSelf) {
                TeleportMarker.SetActive(false);
            }
        }

        // Raycast, update graphics
        void updateTeleport() {

            if(validTeleport) {
                // Ensure line is enabled if we have a valid connection
                TeleportLine.enabled = true;

                // Rotate based on controller thumbstick
                rotateMarker();

                // Make sure constraint isn't active
                playerController.LastTeleportTime = Time.time;
            }

            // Show / Hide Teleport Marker
            TeleportMarker.SetActive(validTeleport);

            // Set proper line color
            Color startColor = validTeleport ? ValidTeleport : InvalidTeleport;
            Color endColor = validTeleport ? ValidTeleport : InvalidTeleport;
            startColor.a = 1;
            endColor.a = 0;
            TeleportLine.startColor = startColor;
            TeleportLine.endColor = endColor;
        }

        void rotateMarker() {

            if(AllowTeleportRotation) {

                if(DirectionIndicator != null && !DirectionIndicator.gameObject.activeSelf) {
                    DirectionIndicator.gameObject.SetActive(true);
                }

                Vector3 controllerDirection = new Vector3(input.LeftThumbstickAxis.x, 0.0f, input.LeftThumbstickAxis.y);

                //get controller pointing direction in world space
                controllerDirection = controller.transform.TransformDirection(controllerDirection);

                //get marker forward in local space
                Vector3 forward = TeleportMarker.transform.forward; // TeleportMarker.transform.InverseTransformDirection(TeleportMarker.transform.forward);

                //find the angle diference betwen the controller pointing direction and marker current forward
                float angle = Vector2.SignedAngle(new Vector2(controllerDirection.x, controllerDirection.z), new Vector2(forward.x, forward.z));

                //rotate marker in local space to match controller pointing direction
                TeleportMarker.transform.Rotate(Vector3.up, angle, Space.Self);
            }
            // Rotation Disabled
            else {
                if (DirectionIndicator != null && DirectionIndicator.gameObject.activeSelf) {
                    DirectionIndicator.gameObject.SetActive(false);
                }
            }
        }

        IEnumerator doTeleport() {

            OVRPlayerController o = GetComponentInChildren<OVRPlayerController>();            
            o.enabled = false;
            controller.enabled = false;
            playerController.LastTeleportTime = Time.time;

            Vector3 destination = TeleportDestination.position;
            bool overrideRotation = false;
            // Override if we're looking at a teleport destination
            var dest = _hitObject.GetComponent<TeleportDestination>();
            if (dest != null) {
                destination = dest.DestinationTransform.position;
                overrideRotation = dest.ForcePlayerRotation;

                if(overrideRotation) {
                    controller.transform.rotation = dest.DestinationTransform.rotation;
                }
            }

            // Calculate teleport offset as character may have been resized
            float yOffset = 1 + cameraRig.localPosition.y;

            // Apply Teleport before offset is applied
            controller.transform.position = destination;
            
            // Aopply offset
            controller.transform.localPosition -= new Vector3(0, yOffset, 0);

            // Rotate player to TeleportMarker Rotation
            if (AllowTeleportRotation && !overrideRotation) {
                controller.transform.rotation = Quaternion.identity;
                controller.transform.localEulerAngles = new Vector3(controller.transform.localEulerAngles.x, TeleportMarker.transform.localEulerAngles.y, controller.transform.localEulerAngles.z);
                controller.transform.rotation = TeleportMarker.transform.rotation;

                // Force our character to remain upright
                controller.transform.eulerAngles = new Vector3(0, controller.transform.eulerAngles.y, 0);
            }
           
            yield return new WaitForEndOfFrame();
            o.enabled = true;
            controller.enabled = true;
        }

        void tryTeleport() {

            if (validTeleport) {
                Debug.Log("Teleport to" + TeleportDestination.position);
                StartCoroutine(doTeleport());
            }

            // We teleported, so update this value for next raycast
            validTeleport = false;
            aimingTeleport = false;

            hideTeleport();
        }

        // Are we pressing proper to key to initiate teleport?
        bool keyDownForTeleport() {

            // Make sure we can use teleport
            if(!teleportationEnabled) {
                return false;
            }

            // Press stick in any direction to initiate teleport
            if (ControlType == TeleportControls.Thumbstick) {
                if(Math.Abs(input.LeftThumbstickAxis.x) >= 0.75 || Math.Abs(input.LeftThumbstickAxis.y) >= 0.75) {
                    _reachThumbThreshold = true;
                    return true;
                }
                // In dead zone
                else if (_reachThumbThreshold && (Math.Abs(input.LeftThumbstickAxis.x) > 0.25 || Math.Abs(input.LeftThumbstickAxis.y) > 0.25)) {
                    return true;
                }

            }
            else if(ControlType == TeleportControls.BButton) {
                return input.BButton;
            }

            return false;
        }

        bool keyUpFromTeleport() {

            // Stick has returned back past input position
            if (ControlType == TeleportControls.Thumbstick) {
                if(Math.Abs(input.LeftThumbstickAxis.x) <= 0.25 && Math.Abs(input.LeftThumbstickAxis.y) <= 0.25) {
                    // Reset threshold
                    _reachThumbThreshold = false;
                    return true;
                }

                return false;
            }
            // Or no longer holding down button
            else if (ControlType == TeleportControls.BButton) {
                return !input.BButton;
            }

            return true;
        }

        public bool IsAiming() {
            return aimingTeleport;
        }

        void OnDrawGizmosSelected() {
            if (controller != null && TeleportDestination.gameObject.activeSelf) {
                Color gizColor = Color.red;
                gizColor.a = 0.9f;
                Gizmos.color = gizColor;
                Gizmos.DrawSphere(TeleportDestination.position, controller.radius);
            }
        }
    }
}


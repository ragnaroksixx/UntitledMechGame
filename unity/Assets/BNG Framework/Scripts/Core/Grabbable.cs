using OVRTouchSample;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BNG {

    public enum GrabType {
        Snap,
        Precise
    }

    public enum HoldType {
        HoldDown,
        Toggle
    }

    public enum GrabPhysics {
        PhysicsJoint,
        Kinematic,
        None
    }

    public enum OtherGrabBehavior {
        None,
        SwapHands,
        DualGrab
    }

    /// <summary>
    /// An object that can be picked up by a Grabber
    /// </summary>
    public class Grabbable : MonoBehaviour {

        [HideInInspector]
        public bool BeingHeld = false;
        public bool IsGrabbable
        {
            get
            {
                return isGrabbable();
            }
        }
        List<Grabber> validGrabbers;

        /// <summary>
        /// Can the object be picked up from far away. Must be within RemoteGrabber Trigger
        /// </summary>
        public bool RemoteGrabbable = false;

        // Max Distance Object can be Remote Grabbed. Not applicable if RemoteGrabbable is false
        public float RemoteGrabDistance = 2f;

        /// <summary>
        /// The grabber that is currently holding us. Null if not being held
        /// </summary>
        protected List<Grabber> heldByGrabbers;

        /// <summary>
        /// Our root collider. May need to be disabled in some cases.
        /// </summary>
        Collider col;
        Rigidbody rigid;

        /// <summary>
        /// Save whether or not the RigidBody was kinematic on Start.
        /// </summary>
        bool wasKinematic;
        bool usedGravity;

        /// <summary>
        /// Is the object being pulled towards the Grabber
        /// </summary>
        bool remoteGrabbing;

        /// <summary>
        /// Configure which button is used to initiate the grab
        /// </summary>
        public GrabButton GrabButton = GrabButton.Grip;

        /// <summary>
        /// Should the Grab Button be held down, or can it be toggle on and off
        /// </summary>
        public HoldType Grabtype = HoldType.HoldDown;

        /// <summary>
        /// Kinematic Physics locks the object in place on the hand / grabber. PhysicsJoint allows collisions with the environment.
        /// </summary>
        public GrabPhysics GrabPhysics = GrabPhysics.PhysicsJoint;

        /// <summary>
        /// Snap to a location or grab anywhere on the object
        /// </summary>
        public GrabType GrabMechanic = GrabType.Snap;

        /// <summary>
        /// How fast to Lerp the object to the hand
        /// </summary>
        public float GrabSpeed = 7.5f;

        /// <summary>
        /// Multiply controller's velocity times this when throwing
        /// </summary>
        public float ThrowForceMultiplier = 2f; 
        
        /// <summary>
        /// Multiply controller's angular velocity times this when throwing
        /// </summary>
        public float ThrowForceMultiplierAngular = 1.5f; // Multiply Angular Velocity times this

        /// <summary>
        /// Drop the item if we get this far from the Grabber's Center (in meters)
        /// </summary>
        public float BreakDistance = 1f;

        /// <summary>
        /// Enabling this will hide the Transform specified in the Grabber's HandGraphics property
        /// </summary>
        public bool HideHandGraphics = false;

        /// <summary>
        ///  Parent this object to the hands for better stability.
        ///  Not recommended for child grabbers
        /// </summary>
        public bool ParentToHands = true;

        /// <summary>
        /// If true, the hand model will be attached to the grabbed object
        /// </summary>
        public bool ParentHandModel = false;

        /// <summary>
        /// Animator ID of the Hand Pose to use
        /// </summary>
        public HandPoseId CustomHandPose = HandPoseId.Default;

        /// <summary>
        /// What to do if another grabber grabs this while equipped
        /// </summary>
        public OtherGrabBehavior SecondaryGrabBehavior = OtherGrabBehavior.None;
        /// <summary>
        /// The Grabbable can only be grabbed if this grabbable is being held.
        /// Example : If you only want a weapon part to be grabbable if the weapon itself is being held.
        /// </summary>
        public Grabbable OtherGrabbableMustBeGrabbed = null;
        /// <summary>
        /// How much Spring Force to apply to the joint when something comes in contact with the grabbable
        /// A higher Spring Force will make the Grabbable more rigid
        /// </summary>
        public float CollisionSpring = 3000;

        /// <summary>
        /// How much Slerp Force to apply to the joint when something is in contact with the grabbable
        /// A higher Slerp value 
        /// </summary>
        public float CollisionSlerp = 500;

        // Time when the Grab started.
        protected float grabTime;

        /// <summary>
        /// Set to false to disable dropping. If true, will be permanently attached to whatever grabs this.
        /// </summary>
        public bool CanBeDropped = true;

        /// <summary>
        /// Can this object be snapped to snap zones? Set to false if you never want this to be snappable. Further filtering can be done on the SnapZones
        /// </summary>
        public bool CanBeSnappedToSnapZone = true;

        /// <summary>
        /// Time in seconds (Time.time) when we last dropped this item
        /// </summary>
        public float LastDropTime;

        // Total distance between the Grabber and Grabbable.
        float journeyLength;

        private float _originalScale;
        public float OriginalScale {
            get {
                return _originalScale;
            }
        }

        // Keep track of objects that are colliding with us
        List<Collider> collisions;

        /// <summary>
        /// If GrabMechanic is set to Snap, offset local position by this amount
        /// </summary>
        public Vector3 GrabPositionOffset = Vector3.zero;
        private Vector3 _grabPositionOffset = Vector3.zero;

        /// <summary>
        /// If GrabMechanic is set to Snap, offset local rotation by this amount
        /// </summary>
        public Vector3 GrabRotationOffset = Vector3.zero;
        public bool MirrorOffsetForOtherHand = true;

        ConfigurableJoint connectedJoint;

        private Transform _grabTransform;
        
        protected Transform grabTransform
        {
            get
            {
                if(_grabTransform != null) {
                    return _grabTransform;
                }

                _grabTransform = new GameObject().transform;
                _grabTransform.parent = this.transform;
                _grabTransform.name = "Grab Transform";
                _grabTransform.localPosition = Vector3.zero;
                _grabTransform.hideFlags = HideFlags.HideInHierarchy;

                return _grabTransform;
            }
        }
        Transform originalParent;

        protected InputBridge input;
        protected GrabbableEvents events;
        bool didParentHands = false;

        // Start is called before the first frame update
        void Awake() {
            col = GetComponent<Collider>();
            rigid = GetComponent<Rigidbody>();

            if(GameObject.FindGameObjectWithTag("Player")) {
                input = GameObject.FindGameObjectWithTag("Player").GetComponent<InputBridge>();
            }
            
            events = GetComponent<GrabbableEvents>();
            collisions = new List<Collider>();

            // Try parent if no rigid found here
            if (rigid == null && transform.parent != null) {
                rigid = transform.parent.GetComponent<Rigidbody>();
            }

            wasKinematic = rigid != null && rigid.isKinematic;
            usedGravity = rigid != null && rigid.useGravity;
            
            UpdateOriginalParent();

            validGrabbers = new List<Grabber>();

            _grabPositionOffset = GrabPositionOffset;

            _originalScale = transform.localScale.x;
        }

        void Update() {

            if(remoteGrabbing) {
                transform.position = Vector3.Lerp(transform.position, grabTransform.position, Time.deltaTime * GrabSpeed);
                transform.rotation = Quaternion.Lerp(transform.rotation, grabTransform.rotation, Time.deltaTime * GrabSpeed);

                // reached destination, snap to final transform position
                if (Vector3.Distance(transform.position, grabTransform.position) < 0.01) {
                    transform.position = grabTransform.position;
                    transform.rotation = grabTransform.rotation;

                    if(flyingTo != null) {
                        flyingTo.GrabGrabbable(this);
                    }
                }
            }
        }

        void updateJoints(Grabber g) {
            

            if (GrabPhysics == GrabPhysics.PhysicsJoint) {
                // Nothing touching it so we can stick to hand rigidly
                if (collisions.Count == 0) {
                    // Lock Angular, XYZ Motion
                    // Make joint very rigid
                    connectedJoint.rotationDriveMode = RotationDriveMode.XYAndZ;

                    connectedJoint.xMotion = ConfigurableJointMotion.Locked;
                    connectedJoint.yMotion = ConfigurableJointMotion.Locked;
                    connectedJoint.zMotion = ConfigurableJointMotion.Locked;
                    connectedJoint.angularXMotion = ConfigurableJointMotion.Limited;
                    connectedJoint.angularYMotion = ConfigurableJointMotion.Limited;
                    connectedJoint.angularZMotion = ConfigurableJointMotion.Limited;

                    //SoftJointLimitSpring sp = connectedJoint.linearLimitSpring;
                    //sp.spring = 0;
                    //sp.damper = 5;

                    // Stiff if we're holding it
                    JointDrive xDrive = connectedJoint.xDrive;
                    xDrive.positionSpring = 9001;

                    JointDrive slerpDrive = connectedJoint.slerpDrive;
                    slerpDrive.positionSpring = 1000;

                    // Set parent to us to keep movement smoothed
                    if (ParentToHands && transform.parent != g.transform) {
                        transform.parent = g.transform;
                    }
                }
                else {
                    // Make Springy
                    connectedJoint.rotationDriveMode = RotationDriveMode.Slerp;
                    connectedJoint.xMotion = ConfigurableJointMotion.Free;
                    connectedJoint.yMotion = ConfigurableJointMotion.Free;
                    connectedJoint.zMotion = ConfigurableJointMotion.Free;
                    connectedJoint.angularXMotion = ConfigurableJointMotion.Free;
                    connectedJoint.angularYMotion = ConfigurableJointMotion.Free;
                    connectedJoint.angularZMotion = ConfigurableJointMotion.Free;

                    //SoftJointLimitSpring sp = connectedJoint.linearLimitSpring;
                    //sp.spring = 5000;
                    //sp.damper = 5;

                    JointDrive xDrive = connectedJoint.xDrive;
                    xDrive.positionSpring = CollisionSpring;

                    JointDrive slerpDrive = connectedJoint.slerpDrive;
                    slerpDrive.positionSpring = CollisionSlerp;

                    // No parent if we are in contact with something. Player movement should be independent
                    if (ParentToHands) {
                        transform.parent = null;
                    }
                }
            }

            // Snap / Lerp to center or precise position on object
            if (GrabPhysics == GrabPhysics.Kinematic) {
                // Distance moved equals elapsed time times speed.
                float distCovered = (Time.time - grabTime) * GrabSpeed;

                // Fraction of journey completed equals current distance divided by total distance.
                float fractionOfJourney = distCovered / journeyLength;

                if (GrabMechanic == GrabType.Snap) {
                    // Set our position as a fraction of the distance between the markers.
                    transform.position = Vector3.Lerp(transform.position, grabTransform.position, fractionOfJourney);
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, grabTransform.localRotation, Time.deltaTime * 10);
                }
                else if (GrabMechanic == GrabType.Precise) {
                    transform.position = grabTransform.position;
                    transform.eulerAngles = grabTransform.eulerAngles;
                }
            }

            if (ParentHandModel && !didParentHands) {
                float dist = Vector3.Distance(transform.position, grabTransform.position);
                // Ok to parent if we are very close or used precise grab type
                if (dist < 0.002f || GrabMechanic == GrabType.Precise) {
                    parentHandGraphics(g);
                }
            }
        }

        void FixedUpdate() {

            if(BeingHeld) {

                // Something happened to our Grabber. Drop the item
                if(heldByGrabbers == null) {
                    DropItem(null, true, true);
                    return;
                }

                // Make sure all collisions are valid
                filterCollisions();

                // Should we drop the item if it's too far away?
                foreach (var g in heldByGrabbers) {
                    if (Vector3.Distance(transform.position, g.transform.position) > BreakDistance) {
                        DropItem(g, true, true);
                        break;
                    }
                }

                foreach (var g in heldByGrabbers) {

                    // Make the joint stiff or springy, update position
                    updateJoints(g);

                    // Fire off any relevant events
                    callEvents(g);
                }                                
            }
        }
        
        void callEvents(Grabber g) {
            if (events) {
                ControllerHand hand = g.HandSide;

                // Right Hand Controls
                if (hand == ControllerHand.Right) {
                    events.OnGrip(input.RightGrip);
                    events.OnTrigger(input.RightTrigger);

                    if (input.RightTriggerDown) {
                        events.OnTriggerDown();
                    }

                    if (input.AButton) {
                        events.OnButton1();
                    }
                    if (input.AButtonDown) {
                        events.OnButton1Down();
                    }
                    if (input.BButton) {
                        events.OnButton2();
                    }
                    if (input.BButtonDown) {
                        events.OnButton2Down();
                    }
                }

                // Left Hand Controls
                if (hand == ControllerHand.Left) {
                    events.OnGrip(input.LeftGrip);
                    events.OnTrigger(input.LeftTrigger);

                    if (input.LeftTriggerDown) {
                        events.OnTriggerDown();
                    }

                    if (input.XButton) {
                        events.OnButton1();
                    }
                    if (input.XButtonDown) {
                        events.OnButton1Down();
                    }
                    if (input.YButton) {
                        events.OnButton2();
                    }
                    if (input.YButtonDown) {
                        events.OnButton2Down();
                    }
                }
            }
        }
        
        public void AddValidGrabber(Grabber grabber) {

            if(validGrabbers == null) {
                validGrabbers = new List<Grabber>();
            }

            if(!validGrabbers.Contains(grabber)) {
                validGrabbers.Add(grabber);
            }
        }

        public void RemoveValidGrabber(Grabber grabber) {
            if (validGrabbers != null && validGrabbers.Contains(grabber)) {
                validGrabbers.Remove(grabber);
            }
        }

        /// <summary>
        /// Is Object able to be grabbed right now. Must have a valid grabber or remotegrabber
        /// </summary>
        /// <returns></returns>
        bool isGrabbable() {
            return IsValidGrabbable() && validGrabbers.Count > 0;
        }

        /// <summary>
        /// Is this object able to be grabbed. Does not check for valid Grabbers, only if it isn't being held, is active, etc.
        /// </summary>
        /// <returns></returns>
        public bool IsValidGrabbable() {

            // Not valid if not active
            if (!isActiveAndEnabled) {
                return false;
            }

            // Not valid if being held and the object has no secondary grab behavior
            if(BeingHeld == true && SecondaryGrabBehavior == OtherGrabBehavior.None) {
                return false;
            }

            // Make sure grabbed conditions are met
            if (OtherGrabbableMustBeGrabbed != null && !OtherGrabbableMustBeGrabbed.BeingHeld) {
                return false;
            }

            return true;
        }

        void setupConfigJoint(Grabber g) {
            connectedJoint = g.GetComponent<ConfigurableJoint>();
            connectedJoint.autoConfigureConnectedAnchor = false;
            connectedJoint.connectedBody = rigid;

            float anchorOffsetVal = (1 / g.transform.localScale.x) * -1;
            connectedJoint.anchor = Vector3.zero;

            connectedJoint.connectedAnchor = _grabPositionOffset;
        }

        void removeConfigJoint() {
            if(connectedJoint != null) {
                connectedJoint.anchor = Vector3.zero;
                connectedJoint.connectedBody = null;
            }
        }

        void addGrabber(Grabber g){
            if(heldByGrabbers == null)
            {
                heldByGrabbers = new List<Grabber>();
            }

            if(!heldByGrabbers.Contains(g)) {
                heldByGrabbers.Add(g);
            }
        }

        void removeGrabber(Grabber g) {
            if (heldByGrabbers == null) {
                heldByGrabbers = new List<Grabber>();
            }
            else if (heldByGrabbers.Contains(g)) {
                heldByGrabbers.Remove(g);
            }
        }

        public virtual void GrabItem(Grabber grabbedBy) {

            // Make sure we release this item
            if(BeingHeld && SecondaryGrabBehavior != OtherGrabBehavior.DualGrab) {
                DropItem(false, true);
            }

            // Make sure all values are reset first
            ResetGrabbing();

            // Officially being held
            BeingHeld = true;
            grabTime = Time.time;

            // Update held by properties
            addGrabber(grabbedBy);
            grabTransform.parent = grabbedBy.transform;
            _grabPositionOffset = GrabPositionOffset;

            // Set position offset used for placing in Hand
            // Reverse X axis if on other hand
            if(MirrorOffsetForOtherHand && grabbedBy.HandSide == ControllerHand.Left) {
                float newX = _grabPositionOffset.x * -1;
                _grabPositionOffset = new Vector3(newX, _grabPositionOffset.y, _grabPositionOffset.z);
            }

            grabbedBy.transform.localEulerAngles = GrabRotationOffset;                        

            // Hide the hand graphics if necessary
            if (HideHandGraphics) {
                grabbedBy.HideHandGraphics();
            }

            // Use center of grabber if snapping
            if (GrabMechanic == GrabType.Snap) {
                grabTransform.localEulerAngles = Vector3.zero;
                grabTransform.localPosition = Vector3.zero - _grabPositionOffset;
            }
            // Precision hold can use position of what we're grabbing
            else if (GrabMechanic == GrabType.Precise) {
                grabTransform.position = transform.position;
                grabTransform.rotation = transform.rotation;
            }

            // Set up the connected joint
            if (GrabPhysics == GrabPhysics.PhysicsJoint && GrabMechanic == GrabType.Precise) {
                connectedJoint = grabbedBy.GetComponent<ConfigurableJoint>();
                connectedJoint.connectedBody = rigid;
                // Just let the autoconfigure handle the calculations for us
                connectedJoint.autoConfigureConnectedAnchor = true;
            }

            // Set up the connected joint for snapping
            if (GrabPhysics == GrabPhysics.PhysicsJoint && GrabMechanic == GrabType.Snap) {

                // Need to Fix Rotation on Snap Physics when close by
                transform.rotation = grabTransform.rotation;

                // Setup joint
                setupConfigJoint(grabbedBy);
            }

            if (GrabPhysics == GrabPhysics.Kinematic) {

                if (ParentToHands) {
                    transform.parent = grabbedBy.transform;
                }

                if (rigid != null) {
                    rigid.isKinematic = true;
                }
            }            

            // Let events know we were grabbed
            GrabbableEvents[] ge = GetComponents<GrabbableEvents>();
            for (int x = 0; x < ge.Length; x++) {
                ge[x].OnGrab(grabbedBy);
            }
            
            journeyLength = Vector3.Distance(transform.position, grabbedBy.transform.position);
        }

        void parentHandGraphics(Grabber g) {
            if (g.HandsGraphics != null) {
                g.HandsGraphics.transform.parent = transform;
                didParentHands = true;
            }
        }

        Grabber flyingTo;
        public virtual void GrabRemoteItem(Grabber grabbedBy) {
            flyingTo = grabbedBy;
            grabTransform.parent = grabbedBy.transform;
            grabTransform.localEulerAngles = Vector3.zero;
            grabTransform.localPosition = Vector3.zero - GrabPositionOffset;

            grabTransform.transform.localEulerAngles = GrabRotationOffset;

            if (rigid) {
                rigid.collisionDetectionMode = CollisionDetectionMode.Discrete;
                rigid.isKinematic = true;
            }

            remoteGrabbing = true;
        }

        public void ResetGrabbing() {
            if (rigid) {
                rigid.isKinematic = wasKinematic;
            }

            flyingTo = null;

            remoteGrabbing = false;

            collisions = new List<Collider>();
        }

        public virtual void DropItem(Grabber droppedBy, bool resetVelocity, bool resetParent) {

            // Nothing holding us
            if(heldByGrabbers == null) {
                BeingHeld = false;
                return;
            }

            if (resetParent) {
                ResetParent();
            }

            //disconnect all joints and set the connected object to null
            removeConfigJoint();

            //  If something called drop on this item we want to make sure the parent knows about it
            // Reset's Grabber position, grabbable state, etc.
            if (droppedBy) {
                droppedBy.DidDrop();
            }

            // Release item and apply physics force to it
            if (rigid != null) {
                rigid.isKinematic = wasKinematic;
                rigid.useGravity = usedGravity;

                // Make sure velocity is passed on
                GameObject playSpace = GameObject.Find("TrackingSpace");
                Quaternion playSpaceRotation = Quaternion.identity;
                if (playSpace) {
                    playSpaceRotation = playSpace.transform.rotation;
                }

                if (resetVelocity && droppedBy) {
                    Vector3 velocity = playSpaceRotation * droppedBy.GetGrabberAveragedVelocity() + droppedBy.GetComponent<Rigidbody>().velocity;
                    Vector3 angularVelocity = playSpaceRotation * droppedBy.GetGrabberAveragedAngularVelocity() + droppedBy.GetComponent<Rigidbody>().angularVelocity;

                    StartCoroutine(Release(velocity, angularVelocity));
                }
            }

            if (events) {
                events.OnRelease();
            }

            removeGrabber(droppedBy);
            BeingHeld = false;
            didParentHands = false;
            LastDropTime = Time.time;
        }

        public virtual void DropItem(Grabber droppedBy) {
            DropItem(droppedBy, true, true);
        }

        public virtual void DropItem(bool resetVelocity, bool resetParent) {
            DropItem(GetPrimaryGrabber(), resetVelocity, resetParent);
        }

        public void ResetScale() {
            transform.localScale = new Vector3(OriginalScale, OriginalScale, OriginalScale);
        }

        public void ResetParent() {
            transform.parent = originalParent;
        }

        public void UpdateOriginalParent() {
            originalParent = transform.parent;
        }

        public ControllerHand GetControllerHand(Grabber g) {
            if(g != null) {
                return g.HandSide;
            }

            return ControllerHand.None;
        }
        
        /// <summary>
        /// Returns the Grabber that first grabbed this item. Return null if not being held.
        /// </summary>
        /// <returns></returns>
        public Grabber GetPrimaryGrabber() {
            if(heldByGrabbers != null) {
                return heldByGrabbers.FirstOrDefault();
            }

            return null;
        }

        /// <summary>
        /// Get the closest valid grabber. 
        /// </summary>
        /// <returns>Returns null if no valid Grabbers in range</returns>
        public Grabber GetClosestGrabber() {

            Grabber closestGrabber = null;
            float lastDistance = 9999;

            if (validGrabbers != null) {
               
                foreach(var g in validGrabbers) {
                    if(g != null) {
                        float dist = Vector3.Distance(transform.position, g.transform.position);
                        if(dist < lastDistance) {
                            closestGrabber = g;
                        }
                    }
                }
            }

            return closestGrabber;
        }

        IEnumerator Release (Vector3 velocity, Vector3 angularVelocity) {

            yield return new WaitForFixedUpdate();

            rigid.velocity = velocity * ThrowForceMultiplier;
            rigid.angularVelocity = angularVelocity * -1;
        }

        public virtual bool IsValidCollision(Collision collision) {

            // Ignore Projectiles from grabbable collision
            // This way our grabbable stays rigid when projectils come in contact
            string transformName = collision.transform.name;
            if (transformName.Contains("Projectile") || transformName.Contains("Bullet") || transformName.Contains("Clip")) {
                return false;
            }

            // Ignore Character Joints as these cause jittery issues
            if (transformName.Contains("Joint")) {
                return false;
            }

            // Ignore Character Controllers
            CharacterController cc = collision.gameObject.GetComponent<CharacterController>();
            if (cc) {
                Physics.IgnoreCollision(col, cc,  true);
                return false;
            }

            return true;
        }

        void filterCollisions() {
            foreach (var c in collisions) {
                if (c == null || !c.enabled) {
                    collisions.Remove(c);
                    break;
                }
            }
        }

        private void OnCollisionEnter(Collision collision) {
            // Keep track of how many objects we are colliding with
            if(BeingHeld && IsValidCollision(collision) && !collisions.Contains(collision.collider)) {
                collisions.Add(collision.collider);
            }
        }

        private void OnCollisionExit(Collision collision) {
            if (BeingHeld && IsValidCollision(collision) && collisions.Contains(collision.collider)) {
                collisions.Remove(collision.collider);
            }
        }

        void OnDrawGizmosSelected() {
            
            // Show Grip Point
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(transform.TransformPoint(GrabPositionOffset), 0.025f);
        }

        void OnDestroy() {
            // DropItem(false, false);
        }
    }
}


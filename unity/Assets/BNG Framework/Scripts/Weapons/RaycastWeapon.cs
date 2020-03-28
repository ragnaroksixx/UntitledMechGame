using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// An example weapon script. A more configurable Raycast weapon will be available later.
    /// </summary>
    public class RaycastWeapon : GrabbableEvents {

        /// <summary>
        /// How far we can shoot in meters
        /// </summary>
        public float MaxRange = 25f;

        /// <summary>
        /// How much damage to apply to "Damageable" on contact
        /// </summary>
        public float Damage = 25f;

        /// <summary>
        /// Semi requires user to press trigger repeatedly, Auto to hold down
        /// </summary>
        public FiringType FiringMethod = FiringType.Semi;

        /// <summary>
        /// How does the user reload once the Clip is Empty
        /// </summary>
        public ReloadType ReloadMethod = ReloadType.InfiniteAmmo;

        /// <summary>
        /// Ex : 0.2 = 5 Shots per second
        /// </summary>
        public float FiringRate = 0.2f;
        float lastShotTime;

        /// <summary>
        /// Maximum amount of internal ammo this weapon can hold. Does not account for attached clips.  For example, a shotgun has internal ammo
        /// </summary>
        public float MaxInternalAmmo = 10;

        /// <summary>
        /// Set true to automatically chamber a new round on fire. False to require charging. Example : Bolt-Action Rifle does not auto chamber.  
        /// </summary>
        public bool AutoChamberRounds = true;

        /// <summary>
        /// Does it matter if rounds are chambered or not. Does the user have to charge weapon as soon as ammo is inserted
        /// </summary>
        public bool MustChamberRounds = false;

        /// <summary>
        /// How much force to apply to the tip of the barrel
        /// </summary>
        public Vector3 RecoilForce = Vector3.zero;
        Rigidbody weaponRigid;

        public LayerMask ValidLayers;

        /// <summary>
        /// Transform of trigger to animate rotation of
        /// </summary>
        public Transform TriggerTransform;

        /// <summary>
        /// Move this back on fire
        /// </summary>
        public Transform SlideTransform;

        /// <summary>
        /// Where our raycast or projectile will spawn from
        /// </summary>
        public Transform MuzzlePointTransform;

        /// <summary>
        /// Where to eject a bullet casing (optional)
        /// </summary>
        public Transform EjectPointTransform;

        /// <summary>
        /// Transform of Chambered Bullet. Hide this when no bullet is chambered
        /// </summary>
        public Transform ChamberedBullet;

        /// <summary>
        /// Make this active on fire. Randomize scale / rotation
        /// </summary>
        public GameObject MuzzleFlashObject;

        /// <summary>
        /// Eject this at EjectPointTransform (optional)
        /// </summary>
        public GameObject BulletCasingPrefab;

        /// <summary>
        /// If time is slowed this object will be instantiated instead of using a raycast
        /// </summary>
        public GameObject ProjectilePrefab;

        /// <summary>
        /// Hit Effects spawned at point of impact
        /// </summary>
        public GameObject HitFXPrefab;

        /// <summary>
        /// Play this sound on shoot
        /// </summary>
        public AudioClip GunShotSound;

        /// <summary>
        /// Play this sound if no ammo and user presses trigger
        /// </summary>
        public AudioClip EmptySound;

        /// <summary>
        /// How far back to move the slide on fire
        /// </summary>
        public float SlideDistance = -0.028f;

        /// <summary>
        /// Is there currently a bullet chambered and ready to be fired
        /// </summary>
        public bool BulletInChamber = false;

        /// <summary>
        /// Is there currently a bullet chambered and that must be ejected
        /// </summary>
        public bool EmptyBulletInChamber = false;

        /// <summary>
        /// Should the slide be forced back if we shoot the last bullet
        /// </summary>
        public bool ForceSlideBackOnLastShot = true;

        /// <summary>
        /// (Optional) Look at this grabbable if being held with secondary hand
        /// </summary>
        public Grabbable SecondHandGrabbable;

        /// <summary>
        /// Is the slide / receiver forced back due to last shot
        /// </summary>
        bool slideForcedBack = false;

        WeaponSlide ws;

        private float originalTriggerX;
        private bool readyToShoot = true;

        void Start() {
            weaponRigid = GetComponent<Rigidbody>();

            if(TriggerTransform) {
                originalTriggerX = TriggerTransform.localEulerAngles.x;
            }

            if (MuzzleFlashObject) {
                MuzzleFlashObject.SetActive(false);
            }

            ws = GetComponentInChildren<WeaponSlide>();

            updateChamberedBullet();
        }

        public override void OnTrigger(float triggerValue) {

            // Update trigger graphics
            if(TriggerTransform) {
                float maxTriggerValue = originalTriggerX + 20f; // -10, 10
                TriggerTransform.localEulerAngles = new Vector3(originalTriggerX + (triggerValue * maxTriggerValue), 0, 0);
            }

            if (triggerValue <= 0.5) {
                readyToShoot = true;
            }

            // Fire gun if possible
            if (readyToShoot && triggerValue >= 0.75f) {
                Shoot();

                // Immediately ready to keep firing if 
                readyToShoot = FiringMethod == FiringType.Automatic;
            }

            updateChamberedBullet();

            checkSecondaryWeaponLook();

            base.OnTrigger(triggerValue);
        }

        void LateUpdate() {
            checkSecondaryWeaponLook();
        }

        Vector3 initialSecondaryOffset;
        float yOffset = 0;
        void checkSecondaryWeaponLook() {
            if(SecondHandGrabbable != null && SecondHandGrabbable.BeingHeld) {

                if(thisGrabber != null) {
                    Vector3 angle = thisGrabber.transform.localEulerAngles;
                    var secondGrabber = SecondHandGrabbable.GetPrimaryGrabber();

                    if (initialSecondaryOffset == Vector3.zero) {
                        float yOffset = SecondHandGrabbable.transform.position.y - secondGrabber.transform.position.y;
                        initialSecondaryOffset = SecondHandGrabbable.transform.position - secondGrabber.transform.position;
                        Debug.Log(yOffset);
                    }
                    
                    //Vector3 trans = new Vector3(0, yOffset, 0);
                    //thisGrabber.transform.LookAt(secondGrabber.transform.position + secondGrabber.transform.TransformVector(trans), thisGrabber.transform.forward);

                    Quaternion rot = Quaternion.LookRotation(secondGrabber.transform.position - thisGrabber.transform.position);

                    thisGrabber.transform.rotation = Quaternion.Slerp(thisGrabber.transform.rotation, rot, Time.deltaTime * 35);

                    // Only use these coords
                    thisGrabber.transform.localEulerAngles = new Vector3(thisGrabber.transform.localEulerAngles.x, thisGrabber.transform.localEulerAngles.y, angle.z);
                }
            }
            else if(SecondHandGrabbable != null) {
                // Reset Vector
                if (thisGrabber != null) {
                    thisGrabber.transform.localRotation = Quaternion.Slerp(thisGrabber.transform.localRotation, Quaternion.identity, Time.deltaTime * 20);
                }

                initialSecondaryOffset = Vector3.zero;
            }
        }

        // Snap slide back in to place Button 2 (A / X)
        public override void OnButton1Down() {
           
            if(ws != null) {                
                ws.UnlockBack();
            }

            base.OnButton1Down();
        }

        // Eject clips when press Button 1 (B / Y)
        public override void OnButton2Down() {

            MagazineSlide ms = GetComponentInChildren<MagazineSlide>();
            if (ms != null) {
                ms.EjectMagazine();
            }

            base.OnButton2Down();
        }

        public float ShotForce = 10f;
        public void Shoot() {

            // Has enough time passed between shots
            float shotInterval = Time.timeScale < 1 ? 0.3f : FiringRate;
            if (Time.time - lastShotTime < shotInterval) {
                return;
            }

            // Need to Chamber round into weapon
            if(!BulletInChamber && MustChamberRounds) {
                VRUtils.Instance.PlaySpatialClipAt(EmptySound, transform.position, 1f, 0.5f);
                return;
            }

            // Need to release slide
            if(ws != null && ws.LockedBack) {
                VRUtils.Instance.PlaySpatialClipAt(EmptySound, transform.position, 1f, 0.5f);
                return;
            }

            // Create our own spatial clip
            VRUtils.Instance.PlaySpatialClipAt(GunShotSound, transform.position, 1f);

            // Haptics
            if (thisGrabber != null) {
                input.VibrateController(0.1f, 0.2f, 0.1f, thisGrabber.HandSide);
            }

            // Use projectile if Time has been slowed
            bool useProjectile = Time.timeScale < 1;
            if (useProjectile) {
                GameObject projectile = Instantiate(ProjectilePrefab, MuzzlePointTransform.position, MuzzlePointTransform.rotation) as GameObject;
                Rigidbody projectileRigid = projectile.GetComponent<Rigidbody>();
                projectileRigid.AddForce(MuzzlePointTransform.forward * ShotForce, ForceMode.VelocityChange);
                Projectile proj = projectile.GetComponent<Projectile>();
                // Convert back to raycast if Time reverts
                if (proj) {
                    proj.MarkAsRaycastBullet();
                }

                // Make sure we clean up this projectile
                Destroy(projectile, 20);
            }
            else {
                // Raycast to hit
                RaycastHit hit;
                if (Physics.Raycast(MuzzlePointTransform.position, MuzzlePointTransform.forward, out hit, MaxRange, ValidLayers, QueryTriggerInteraction.Ignore)) {
                    // Particle FX on impact
                    Quaternion decalRotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
                    GameObject impact = Instantiate(HitFXPrefab, hit.point, decalRotation) as GameObject;

                    BulletHole hole = impact.GetComponent<BulletHole>();
                    if (hole) {
                        hole.TryAttachTo(hit.collider);
                    }

                    // push object if rigidbody
                    Rigidbody hitRigid = hit.collider.attachedRigidbody;
                    if (hitRigid != null) {
                        float bulletForce = 1000;
                        hitRigid.AddForceAtPosition(bulletForce * MuzzlePointTransform.forward, hit.point);
                    }

                    // Damage if possible
                    Damageable d = hit.collider.GetComponent<Damageable>();
                    if (d) {
                        d.DealDamage(Damage);
                    }
                }
            }

            // Apply recoil
            if (weaponRigid != null && RecoilForce != Vector3.zero) {
                weaponRigid.AddForceAtPosition(RecoilForce, MuzzlePointTransform.position, ForceMode.VelocityChange);
            }

            // We just fired this bullet
            BulletInChamber = false;

            // Try to load a new bullet into chamber         
            if (AutoChamberRounds) {
                chamberRound();
            }
            else {
                EmptyBulletInChamber = true;
            }

            // Unable to chamber bullet, force slide back
            if(!BulletInChamber) {
                // Do we need to force back the receiver?
                slideForcedBack = ForceSlideBackOnLastShot;

                if (slideForcedBack && ws != null) {
                    ws.LockBack();
                }
            }

            // Store our last shot time to be used for rate of fire
            lastShotTime = Time.time;

            // Stop previous routine
            if (shotRoutine != null) {
                StopCoroutine(shotRoutine);

            }

            if (AutoChamberRounds) {
                shotRoutine = animateSlideAndEject();
                StartCoroutine(shotRoutine);
            }
            else {
                shotRoutine = doMuzzleFlash();
                StartCoroutine(shotRoutine);
            }
            
        }

        /// <summary>
        /// Something attached ammo to us
        /// </summary>
        public void OnAttachedAmmo() {

            // May have ammo loaded
            updateChamberedBullet();
        }

        // Ammo was detached from the weapon
        public void OnDetachedAmmo() {
            // May have ammo loaded
            updateChamberedBullet();
        }

        public int GetBulletCount() {
            if (ReloadMethod == ReloadType.InfiniteAmmo) {
                return 9999;
            }

            return GetComponentsInChildren<Bullet>(false).Length;
        }

        void removeBullet() {

            // Don't remove bullet here
            if (ReloadMethod == ReloadType.InfiniteAmmo) {
                return;
            }

            Bullet firstB = GetComponentInChildren<Bullet>(false);

            // Deactivate gameobject as this bullet has been consumed
            if(firstB != null) {
                Destroy(firstB.gameObject);
                //firstB.gameObject.SetActive(false);
            }

            // Whenever we remove a bullet is a good time to check the chamber
            updateChamberedBullet();
        }

        void updateChamberedBullet() {
            if (ChamberedBullet != null) {
                ChamberedBullet.gameObject.SetActive(BulletInChamber || EmptyBulletInChamber);
            }
        }

        void chamberRound() {

            int currentBulletCount = GetBulletCount();

            if(currentBulletCount > 0) {
                // Remove the first bullet we find in the clip                
                removeBullet();

                // That bullet is now in chamber
                BulletInChamber = true;
            }
            // Unable to chamber a bullet
            else {
                BulletInChamber = false;
            }

            
        }

        IEnumerator shotRoutine;
        public float slideSpeed = 1;
        public float minSlideDistance = 0.001f;

        // Randomly scale / rotate to make them seem different
        void randomizeMuzzleFlashScaleRotation() {
            MuzzleFlashObject.transform.localScale = Vector3.one * Random.Range(0.75f, 1.5f);
            MuzzleFlashObject.transform.localEulerAngles = new Vector3(0, 0, Random.Range(0, 90f));
        }       

        public void OnWeaponCharged(bool allowCasingEject) {

            // Already bullet in chamber, eject it
            if (BulletInChamber && allowCasingEject) {                
                ejectCasing();
            }
            else if (EmptyBulletInChamber && allowCasingEject) {
                ejectCasing();
                EmptyBulletInChamber = false;
            }

            chamberRound();

            // Slide is no longer forced back if weapon was just charged
            slideForcedBack = false;
        }

        void ejectCasing() {
            GameObject shell = Instantiate(BulletCasingPrefab, EjectPointTransform.position, EjectPointTransform.rotation) as GameObject;
            Rigidbody rb = shell.GetComponent<Rigidbody>();

            if (rb) {
                rb.AddRelativeForce(Vector3.right * 3, ForceMode.VelocityChange);
            }

            // Clean up shells
            GameObject.Destroy(shell, 5);
        }

        IEnumerator doMuzzleFlash() {
            MuzzleFlashObject.SetActive(true);
            yield return new WaitForEndOfFrame();

            randomizeMuzzleFlashScaleRotation();
            yield return new WaitForEndOfFrame();

            MuzzleFlashObject.SetActive(false);
        }

        // Animate the slide back, eject casing, pull slide back
        IEnumerator animateSlideAndEject() {

            // Start Muzzle Flash
            MuzzleFlashObject.SetActive(true);

            int frames = 0;
            bool slideEndReached = false;
            Vector3 slideDestination = new Vector3(0, 0, SlideDistance);

            if(SlideTransform) {
                while (!slideEndReached) {


                    SlideTransform.localPosition = Vector3.MoveTowards(SlideTransform.localPosition, slideDestination, Time.deltaTime * slideSpeed);
                    float distance = Vector3.Distance(SlideTransform.localPosition, slideDestination);

                    if (distance <= minSlideDistance) {
                        slideEndReached = true;
                    }

                    frames++;

                    // Go ahead and update muzzleflash in sync with slide
                    if (frames < 2) {
                        randomizeMuzzleFlashScaleRotation();
                    }
                    else {
                        slideEndReached = true;
                        MuzzleFlashObject.SetActive(false);
                    }

                    yield return new WaitForEndOfFrame();
                }
            }
            else {
                yield return new WaitForEndOfFrame();
                randomizeMuzzleFlashScaleRotation();
                yield return new WaitForEndOfFrame();
                
                MuzzleFlashObject.SetActive(false);
                slideEndReached = true;
            }
            
            // Set Slide Position
            if(SlideTransform) {
                SlideTransform.localPosition = slideDestination;
            }
            

            yield return new WaitForEndOfFrame();

            // Eject Shell
            ejectCasing();

            // Pause for shell to eject before returning slide
            yield return new WaitForEndOfFrame();


            if(!slideForcedBack && SlideTransform != null) {
                // Slide back to original position
                frames = 0;
                bool slideBeginningReached = false;
                while (!slideBeginningReached) {

                    SlideTransform.localPosition = Vector3.MoveTowards(SlideTransform.localPosition, Vector3.zero, Time.deltaTime * slideSpeed);
                    float distance = Vector3.Distance(SlideTransform.localPosition, Vector3.zero);

                    if (distance <= minSlideDistance) {
                        slideBeginningReached = true;
                    }

                    if (frames > 2) {
                        slideBeginningReached = true;
                    }

                    yield return new WaitForEndOfFrame();
                }
            }
        }
    }

    public enum FiringType {
        Semi,
        Automatic
    }

    public enum ReloadType {
        InfiniteAmmo,
        ManualClip
    }
}


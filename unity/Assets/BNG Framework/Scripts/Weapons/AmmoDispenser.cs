using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {


    /// <summary>
    /// This is an example of how to spawn ammo depending on the weapon that is equipped in the opposite hand
    /// </summary>
    public class AmmoDispenser : MonoBehaviour {

        /// <summary>
        /// Used to determine if holding a weapon
        /// </summary>
        public Grabber LeftGrabber;

        /// <summary>
        /// Used to determine if holding a weapon
        /// </summary>
        public Grabber RightGrabber;

        /// <summary>
        /// Disable this if weapon not equipped
        /// </summary>
        public GameObject AmmoDispenserObject;

        /// <summary>
        /// Instantiate this if pistol equipped
        /// </summary>
        public GameObject PistolClip;

        /// <summary>
        /// Instantiate this if shotgun equipped
        /// </summary>
        public GameObject ShotgunShell;

        // Update is called once per frame
        void Update() {
            bool weaponEquipped = false;

            if (grabberHasWeapon(LeftGrabber) || grabberHasWeapon(RightGrabber)) {
                weaponEquipped = true;
            }

            // Only show if we have something equipped
            if(AmmoDispenserObject.activeSelf != weaponEquipped) {
                AmmoDispenserObject.SetActive(weaponEquipped);
            }
        }

        bool grabberHasWeapon(Grabber g) {

            // Holding shotgun or pistol
            if(g != null && g.HeldGrabbable != null && (g.HeldGrabbable.transform.name.Contains("Shotgun") || g.HeldGrabbable.transform.name.Contains("Pistol"))) {
                return true;
            }

            return false;
        }

        GameObject getAmmo() {

            if (LeftGrabber != null && LeftGrabber.HeldGrabbable != null && LeftGrabber.HeldGrabbable.transform.name.Contains("Shotgun")) {
                return ShotgunShell;
            }
            else if (RightGrabber != null && RightGrabber.HeldGrabbable != null && RightGrabber.HeldGrabbable.transform.name.Contains("Shotgun")) {
                return ShotgunShell;
            }

            // Default to Pistol
            return PistolClip;
        }

        public void GrabAmmo(Grabber grabber) {

            GameObject ammo = Instantiate(getAmmo(), grabber.transform.position, grabber.transform.rotation) as GameObject;
            Grabbable g = ammo.GetComponent<Grabbable>();

            // Disable rings for performance
            GrabbableRingHelper grh = ammo.GetComponentInChildren<GrabbableRingHelper>();
            if (grh) {
                Destroy(grh);
                RingHelper r = ammo.GetComponentInChildren<RingHelper>();
                Destroy(r.gameObject);
            }


            // Offset to hand
            ammo.transform.parent = grabber.transform;
            ammo.transform.localPosition = -g.GrabPositionOffset;
            ammo.transform.parent = null;

            grabber.GrabGrabbable(g);
        }
    }
}


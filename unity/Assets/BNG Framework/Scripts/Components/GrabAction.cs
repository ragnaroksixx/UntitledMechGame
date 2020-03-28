using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BNG {

    /// <summary>
    /// Call a user-defined event on Grab
    /// </summary>
    public class GrabAction : GrabbableEvents {

        public GrabberEvent OnGrabEvent;

        Grabbable g;
        float lastGrabTime = 0;
        float minTimeBetweenGrabs = 0.2f; // In Seconds

        public override void OnGrab(Grabber grabber) {

            if(g == null) {
                g = GetComponent<Grabbable>();
            }

            // Never hold this item
            g.DropItem(grabber, false, false);

            // Don't grab this if we are currently grabbing / remote grabbing a different item
            if(grabber.RemoteGrabbingItem || grabber.HoldingItem) {
                return;
            }
            
            // Call the event
            if (OnGrabEvent != null) {

                // Don't want to repeatedly do grabs if this is a hold item
                if(Time.time - lastGrabTime >= minTimeBetweenGrabs) {
                    OnGrabEvent.Invoke(grabber);
                    lastGrabTime = Time.time;
                }
            }
        }
    }
}


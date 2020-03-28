using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {
    public class RotateWithHMD : MonoBehaviour {

        /// <summary>
        /// The Eye / HMD to maintain rotation with
        /// </summary>
        public Transform HMDTransform;

        /// <summary>
        /// The Character Capsule to  rotate along with
        /// </summary>
        public CharacterController Character;

        /// <summary>
        /// Offset to apply in local space to the hmdTransform
        /// </summary>
        public Vector3 Offset = new Vector3(0, -0.25f, 0);


        // Update is called once per frame
        void Update() {
            updateBodyPosition();
        }

        void updateBodyPosition() {
            if (HMDTransform != null) {
                transform.position = HMDTransform.position;

                // Move position relative to Character Controller
                if (Character != null) {
                    transform.rotation = Quaternion.Lerp(transform.rotation, Character.transform.rotation, Time.deltaTime * 5f);
                    transform.localPosition -= Character.transform.TransformVector(Offset);
                }
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {

    /// <summary>
    /// This collider will Damage a Damageable object on impact
    /// </summary>
    public class DamageCollider : MonoBehaviour {

        /// <summary>
        /// How much damage to apply to the Damageable object
        /// </summary>
        public float Damage = 25f;

        /// <summary>
        /// Used to determine velocity of this collider
        /// </summary>
        Rigidbody damageRigidbody;

        /// <summary>
        /// Minimum Amount of force necessary to do damage. Expressed as relativeVelocity.magnitude
        /// </summary>
        public float MinForce = 0.1f;

        /// <summary>
        /// Our previous frame's last relative velocity value
        /// </summary>
        public float LastRelativeVelocity = 0;

        // How much impulse force was applied last onCollision enter
        public float LastDamageForce = 0;

        private void Start() {
            damageRigidbody = GetComponent<Rigidbody>();
        }        

        private void OnCollisionEnter(Collision collision) {

            LastDamageForce = collision.impulse.magnitude;
            LastRelativeVelocity = collision.relativeVelocity.magnitude;

            if (LastDamageForce >= MinForce) {
                Damageable d = collision.gameObject.GetComponent<Damageable>();
                if (d) {
                    d.DealDamage(Damage);
                }
            }
        }
    }
}


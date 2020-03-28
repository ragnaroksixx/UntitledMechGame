using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BNG {
    /// <summary>
    /// A basic damage implementation. Call a function on death. Allow for respawning.
    /// </summary>
    public class Damageable : MonoBehaviour {

        public float Health = 100;
        private float _startingHealth;

        public List<GameObject> ActivateGameObjectsOnDeath;
        public List<GameObject> DeactivateGameObjectsOnDeath;
        public List<Collider> DeactivateCollidersOnDeath;

        /// <summary>
        /// Destroy this object on Death? False if need to respawn.
        /// </summary>
        public bool DestroyOnDeath = true;

        /// <summary>
        /// How long to wait before destroying this objects
        /// </summary>
        public float DestroyDelay = 0f;

        /// <summary>
        /// If true the object will be reactivated according to RespawnTime
        /// </summary>
        public bool Respawn = false;

        /// <summary>
        /// If Respawn true, this gameObject will reactivate after RespawnTime. In seconds.
        /// </summary>
        public float RespawnTime = 10f;

        /// <summary>
        /// Remove any decals that were parented to this object on death. Useful for clearing unused decals.
        /// </summary>
        public bool RemoveBulletHolesOnDeath = true;

        bool destroyed = false;

        private void Start() {
            _startingHealth = Health;
        }

        public void DealDamage(float damageAmount) {

            if (destroyed) {
                return;
            }

            Health -= damageAmount;
            if (Health < 0) {
                DestroyThis();
            }
        }

        public void DestroyThis() {
            Health = 0;
            destroyed = true;

            // Activate
            foreach (var go in ActivateGameObjectsOnDeath) {
                go.SetActive(true);
            }

            // Deactivate
            foreach (var go in DeactivateGameObjectsOnDeath) {
                go.SetActive(false);
            }
            foreach (var col in DeactivateCollidersOnDeath) {
                col.enabled = false;
            }

            if (DestroyOnDeath) {
                Destroy(this.gameObject, DestroyDelay);
            }
            else if (Respawn) {
                StartCoroutine(RespawnRoutine(RespawnTime));
            }

            if (RemoveBulletHolesOnDeath) {
                BulletHole[] holes = GetComponentsInChildren<BulletHole>();
                foreach (var hole in holes) {
                    GameObject.Destroy(hole.gameObject);
                }

                Transform decal = transform.Find("Decal");
                if (decal) {
                    GameObject.Destroy(decal.gameObject);
                }
            }
        }

        IEnumerator RespawnRoutine(float seconds) {

            yield return new WaitForSeconds(seconds);

            Health = _startingHealth;
            destroyed = false;

            // Deactivate
            foreach (var go in ActivateGameObjectsOnDeath) {
                go.SetActive(false);
            }

            // Re-Activate
            foreach (var go in DeactivateGameObjectsOnDeath) {
                go.SetActive(true);
            }
            foreach (var col in DeactivateCollidersOnDeath) {
                col.enabled = true;
            }
        }
    }
}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Projectile prefab;
    public Transform shootpoint;
    public float mag;
    public float shootRate;
    public Rigidbody player;
    public virtual void Shoot()
    {
        Projectile instance = GameObject.Instantiate(prefab, shootpoint.position, shootpoint.rotation);
        float magUse = player.velocity.magnitude + mag;
        instance.Shoot(shootpoint.forward, magUse);
    }
}

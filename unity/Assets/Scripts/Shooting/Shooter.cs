using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Projectile prefab;
    public Transform[] shootpoints;
    protected float mag= 10000;
    public float shootRate;
    public Rigidbody player;
    public bool isSingleSot = false;
    public int numShots = 1;
    protected float lifeSpan = 1;
    public virtual void Shoot()
    {
        for (int i = 0; i < numShots; i++)
        {
            Transform shootpoint = shootpoints[i];
            Projectile instance = GameObject.Instantiate(prefab, shootpoint.position, shootpoint.rotation);
            float magUse = player.velocity.magnitude + mag;
            instance.Shoot(shootpoint.forward, magUse, lifeSpan);
        }
    }
}

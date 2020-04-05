using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject onContactExplosion;
    bool onecheck;

    public void Shoot(Vector3 dir, float speed, float lifeSpan)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(dir * speed);
        Destroy(this.gameObject, lifeSpan);
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (onecheck) return;
        onecheck = true;
        Shootable[] themshots = collision.gameObject.GetComponentsInChildren<Shootable>();
        foreach (Shootable item in themshots)
        {
            item.OnShot(this);
        }
        Destroy(this.gameObject);
    }
    private void OnDestroy()
    {
        GameObject.Instantiate(onContactExplosion.gameObject, transform.position, transform.rotation);
    }

}

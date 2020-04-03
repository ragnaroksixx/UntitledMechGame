using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public GameObject onContactExplosion;
    bool onecheck;
    float lifespan = 1f;
    private void Awake()
    {
        Destroy(this.gameObject, lifespan);
    }
    public void Shoot(Vector3 dir, float speed)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.AddForce(dir * speed);
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

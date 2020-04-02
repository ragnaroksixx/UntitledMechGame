using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PlayerShooter : Shooter
{
    public bool isRight;
    float timeToNextShoot;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            StartShoot();
        }
        else if (Input.GetKey(KeyCode.Mouse0))
        {
            ShootUpdate();
        }
    }
    void StartShoot()
    {
        Shoot();
    }
    void ShootUpdate()
    {
        if (Time.time > timeToNextShoot)
            Shoot();
    }
    public override void Shoot()
    {
        base.Shoot();
        timeToNextShoot = Time.time + shootRate;
    }
}


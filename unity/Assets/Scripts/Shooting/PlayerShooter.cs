using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PlayerShooter : Shooter
{
    public bool isRight;
    float timeToNextShoot;
    KeyCode pcKey;
    public Transform reticle;
    public Flightstick stick;
    public float maxRot = 60;
    private void Update()
    {
        shootpoint.LookAt(reticle);

        pcKey = !isRight ? KeyCode.Mouse0 : KeyCode.Mouse1;
        bool isDown = Input.GetKeyDown(pcKey) || (isRight ? InputBridge.instance.RightTriggerDown : InputBridge.instance.LeftTriggerDown);
        bool isHeld = Input.GetKey(pcKey) || (isRight ? InputBridge.instance.RightTrigger > 0.5f : InputBridge.instance.LeftTrigger > 0.5f);
        if (!stick.BeingHeld && !(isDown || isHeld)) return;
        if (isDown)
        {
            StartShoot();
        }
        else if (isHeld)
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


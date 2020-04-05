using BNG;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShooter : Shooter
{
    public bool isRight;
    float timeToNextShoot;
    KeyCode pcKey;
    public Transform reticle;
    public Flightstick stick;
    public float maxRot = 60;
    public AudioSource audioSource;
    public AudioClip[] clips;
    public enum GunType
    {
        NONE = 0,
        PISTOL = 1,
        SHOTGUN = 2,
        MACHINEGUN = 3
    }
    GunType gunType;
    public List<GameObject> guns;
    private void Awake()
    {
        SetGun(GunType.NONE, true);
    }
    private void Update()
    {
        shootpoints[0].LookAt(reticle);

        pcKey = !isRight ? KeyCode.Mouse0 : KeyCode.Mouse1;
        bool isDown = Input.GetKeyDown(pcKey) || (isRight ? InputBridge.instance.RightTriggerDown : InputBridge.instance.LeftTriggerDown);
        bool isHeld = false;
        if (!isSingleSot)
            isHeld = Input.GetKey(pcKey) || (isRight ? InputBridge.instance.RightTrigger > 0.5f : InputBridge.instance.LeftTrigger > 0.5f);
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
        if (gunType == GunType.NONE) return;
        base.Shoot();
        timeToNextShoot = Time.time + shootRate;
        audioSource.Play();
    }
    public void SetGun(GunType guno, bool force = false)
    {
        if (gunType == guno && !force) return;
        gunType = guno;
        for (int i = 0; i < guns.Count; i++)
        {
            guns[i].SetActive(i == (int)guno);
        }
        audioSource.clip = clips[(int)guno];
        isSingleSot = guno != GunType.MACHINEGUN;
        switch (guno)
        {
            case GunType.NONE:
                break;
            case GunType.PISTOL:
                numShots = 1;
                lifeSpan = 0.25f;
                break;
            case GunType.SHOTGUN:
                numShots = 5;
                lifeSpan = 0.5f;
                break;
            case GunType.MACHINEGUN:
                numShots = 1;
                lifeSpan = 1;
                break;
            default:
                break;
        }
    }

    public void OnScoreUpdate(int score)
    {
        if (!isRight)
        {
            if (score < 200)
                SetGun(GunType.PISTOL);
            else if (score < 600)
                SetGun(GunType.SHOTGUN);
            else
                SetGun(GunType.MACHINEGUN);
        }
        else
        {
            if (score < 100)
                SetGun(GunType.NONE);
            else if (score < 300)
                SetGun(GunType.PISTOL);
            else if (score < 900)
                SetGun(GunType.SHOTGUN);
            else
                SetGun(GunType.MACHINEGUN);
        }
    }
}


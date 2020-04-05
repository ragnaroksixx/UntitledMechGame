using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class MechController : MonoBehaviour
{
    public float normalSpeed = 15;
    public float boostSpeed = 30;
    public float rotationSpeed = 15;

    public Transform kartNormal;
    public Rigidbody sphere;

    float speed, currentSpeed;
    float rotate, currentRotate;

    public float gravity = 10f;
    public LayerMask layerMask;

    public static MechController player;

    int starthealth = 2;
    int maxhealth = 4;
    public int health;

    public Image hpImage;
    public List<Sprite> hpImages;
    public Light hpLight;
    private void Awake()
    {
        player = this;
        health = starthealth;
        UpdateHpUI();
    }
    void Update()
    {
        //Follow Collider
        transform.position = sphere.transform.position;

        //Accelerate
        if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.Space))
            Move(1, 2);
        else if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.Space))
            Move(1, 1);
        else if (Input.GetKey(KeyCode.S))
            Move(-1, 1);

        //Steer
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            int dir = 1;
            if (Input.GetKey(KeyCode.A))
                dir = -1;
            //float amount = Mathf.Abs((Input.GetAxis("Horizontal")));
            Steer(dir, 1);
        }
        float damp = 12;
        if (speed < currentSpeed)
            damp = 6;
        currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * damp);
        speed = 0;
        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);
        rotate = 0;


    }
    public void Hit()
    {
        health--;
        float flashTime = 1;
        int numFlashes = 5;
        float intensity = 4;
        if (health <= 0)
            intensity = 2;
        hpLight.DOKill();
        DOTween.Sequence()
            .Append(hpLight.DOIntensity(intensity, flashTime / (float)numFlashes))
            .Append(hpLight.DOIntensity(0, flashTime / (float)numFlashes))
            .SetLoops(numFlashes);
        if (health <= 0)
        {
            LevelManager.instance.EndGame();
        }
        UpdateHpUI();
    }
    public void OnGameStart()
    {
        health = starthealth;
        UpdateHpUI();
    }
    public void GainHP(int i = 1)
    {
        health += i;
        if (health > maxhealth)
            health = maxhealth;
        UpdateHpUI();
    }
    void UpdateHpUI()
    {
        hpImage.sprite = hpImages[health];
    }
    private void FixedUpdate()
    {
        //Forward Acceleration
        sphere.AddForce(transform.forward * currentSpeed, ForceMode.Acceleration);

        //Gravity
        sphere.AddForce(Vector3.down * gravity, ForceMode.Acceleration);

        //Steering
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, new Vector3(0, transform.eulerAngles.y + currentRotate, 0), Time.deltaTime * 5f);

        RaycastHit hitOn;
        RaycastHit hitNear;

        Physics.Raycast(transform.position + (transform.up * .1f), Vector3.down, out hitOn, 1.1f, layerMask);
        Physics.Raycast(transform.position + (transform.up * .1f), Vector3.down, out hitNear, 2.0f, layerMask);

        //Normal Rotation
        kartNormal.up = Vector3.Lerp(kartNormal.up, hitNear.normal, Time.deltaTime * 8.0f);
        kartNormal.Rotate(0, transform.eulerAngles.y, 0);
    }

    public void Steer(int direction, float amount)
    {
        if (!LevelManager.instance.isPlaying)
        {
            rotate = 0;
            return;
        }
        rotate = (rotationSpeed * direction) * amount;
    }
    public void Move(int direction, float amount)
    {
        if (!LevelManager.instance.isPlaying)
        {
            speed = 0;
            return;
        }
        speed = normalSpeed * amount * amount * direction;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + transform.up, transform.position - (transform.up * 2));
    }
}



using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class MechController : MonoBehaviour
{

    public enum MovementTier
    {
        STOP = 0,
        NORMAL,
        BOOST
    }
    MovementTier moveTier;
    public float normalSpeed = 15;
    public float boostSpeed = 30;
    public float rotationSpeed = 15;
    private void Awake()
    {
    }
    public void SetTier(MovementTier tier)
    {
        moveTier = tier;
        switch (moveTier)
        {
            case MovementTier.STOP:
                speed = 0;
                break;
            case MovementTier.NORMAL:
                speed = normalSpeed;
                break;
            case MovementTier.BOOST:
                speed = boostSpeed;
                break;
            default:
                break;
        }
    }
    public Transform kartNormal;
    public Rigidbody sphere;

    float speed, currentSpeed;
    float rotate, currentRotate;

    public float gravity = 10f;
    public LayerMask layerMask;

    void Update()
    {
        //Follow Collider
        transform.position = sphere.transform.position;

        //Accelerate
        if (Input.GetKeyDown(KeyCode.Mouse0))
            SetTier(MovementTier.NORMAL);
        else if (Input.GetKeyDown(KeyCode.Mouse1))
            SetTier(MovementTier.STOP);
        else if (Input.GetKeyDown(KeyCode.Mouse2))
            SetTier(MovementTier.BOOST);

        //Steer
        if (Input.GetAxis("Horizontal") != 0)
        {
            int dir = Input.GetAxis("Horizontal") > 0 ? 1 : -1;
            float amount = Mathf.Abs((Input.GetAxis("Horizontal")));
            Steer(dir, amount);
        }

        currentSpeed = Mathf.SmoothStep(currentSpeed, speed, Time.deltaTime * 12f);
        currentRotate = Mathf.Lerp(currentRotate, rotate, Time.deltaTime * 4f);


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
        rotate = (rotationSpeed * direction) * amount;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position + transform.up, transform.position - (transform.up * 2));
    }
}



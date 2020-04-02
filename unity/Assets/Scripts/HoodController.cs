using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class HoodController : MonoBehaviour
{
    public List<GameObject> objectsToHide;
    public Transform hood;
    public Transform closeRot, openRot;
    public float closeSpeed;
    bool isOpen;
    public LayerMask visible, nonVisible;
    Coroutine c = null;
    private void Awake()
    {
    }
    private IEnumerator Start()
    {
        yield return new WaitForSeconds(2);
        Close();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            Toggle();
        }
    }
    public void Open()
    {
        isOpen = true;
        LayerMask mask = isOpen ? visible : nonVisible;
        int layer = (int)Mathf.Log(mask.value, 2);
        foreach (GameObject item in objectsToHide)
        {
            item.layer = layer;
        }
        c = null;
        OnMove();
    }
    public void Close()
    {
        isOpen = false;
        OnMove();
    }
    public void Toggle()
    {
        if (isOpen) Close();
        else Open();
    }
    void OnMove()
    {
        bool wasRunning = false;
        if (c != null)
        {
            StopCoroutine(c);
            wasRunning = true;
        }
        Quaternion rot = isOpen ? openRot.localRotation : closeRot.localRotation;
        hood.DOKill();
        hood.DOLocalRotateQuaternion(rot, closeSpeed)
            .OnComplete(() => OnMoveComplete())
            .SetDelay(isOpen && !wasRunning ? .2f : 0);
    }
    void OnMoveComplete()
    {
        if (c != null)
            StopCoroutine(c);
        c = StartCoroutine(SetLayers());
    }
    public IEnumerator SetLayers()
    {
        yield return new WaitForSeconds(1);
        LayerMask mask = isOpen ? visible : nonVisible;
        int layer = (int)Mathf.Log(mask.value, 2);
        foreach (GameObject item in objectsToHide)
        {
            item.layer = layer;
        }
        c = null;
    }
}


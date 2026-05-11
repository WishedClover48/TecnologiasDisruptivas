using System;
using UnityEngine;

public class PhoneTutorial : MonoBehaviour, ITutorial
{
    [SerializeField] private Camera cam;

    private void Awake()
    {
        if (cam == null) cam = Camera.main;
    }
    public void Activate()
    {
        gameObject.SetActive(true);
    }
    private void OnTriggerExit(Collider other)
    {
        Deactivate();
    }
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
    private void Update()
    {
        var rotation = cam.transform.rotation;
        transform.LookAt(transform.localPosition + rotation * Vector3.forward, rotation * Vector3.up);
    }
}

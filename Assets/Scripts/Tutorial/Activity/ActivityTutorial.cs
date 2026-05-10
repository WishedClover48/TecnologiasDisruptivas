using System;
using UnityEngine;

public class ActivityTutorial : MonoBehaviour
{
    [SerializeField] private Canvas canvas;
    private void Awake()
    {
        canvas.enabled = false;
    }

    [ContextMenu("Active")]
    public void Activate()
    {
        canvas.enabled = true;
    }

    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
}

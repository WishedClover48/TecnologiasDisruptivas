using System;
using UnityEngine;

public class ActivityTutorial : MonoBehaviour, ITutorial
{
    [SerializeField] private Canvas canvas;
    [SerializeField] private Activity activity;
    
    private void Awake()
    {
        canvas.enabled = false;

        activity.OnActivity += Deactivate;
    }
    
    public void Activate()
    {
        gameObject.SetActive(true);
        canvas.enabled = true;
    }

    public void Deactivate()
    {
        activity.OnActivity -= Deactivate;
        gameObject.SetActive(false);
    }
}

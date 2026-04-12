using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Events/Void")]
public class GlobalEventSO_Void : ScriptableObject
{
    public event Action OnEventRaised;

    public void RaiseEvent()
    {
        OnEventRaised?.Invoke();
    }
}
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI_PhoneHistory : MonoBehaviour
{
    [Serializable] private class InternalUIPhoneHistory
    {
        public TMP_Text name;
        public TMP_Text time;
        public TMP_Text money;
        public TMP_Text stress;
        public TMP_Text health;
    }
    
    [SerializeField] private InternalUIPhoneHistory @internal;
    
    private int index;
    private List<ActionData_SO> history;

    private void Start()
    {
        gameObject.SetActive(false);
        history = new List<ActionData_SO>();
        index = 0;
    }

    public void AddToHistory(ActionData_SO action)
    {
        history.Add(action);
        Show(action);
        index = history.Count;
        
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
    }

    public void ShowPrevious()
    {
        if ( (index - 1) < 0 ) index = 0;
        
        Show(history[index]);
    }
    public void ShowNext()
    {
        if ( (index + 1) > history.Count ) index = history.Count;
        
        Show(history[index]);
    }

    private void Show(ActionData_SO lastAction)
    {
        @internal.name.text = lastAction.name;
        @internal.time.text = lastAction.TimeCost.ToString();
        @internal.health.text = lastAction.Health.ToString();
        @internal.stress.text = lastAction.Stress.ToString();
        @internal.money.text = lastAction.MoneyCost.ToString();
    }
}

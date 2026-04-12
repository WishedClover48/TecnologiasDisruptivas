using System;
using DefaultNamespace;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_Phone : MonoBehaviour
{
    [SerializeField] private GlobalEventSO_Int onDayEnd;
    [SerializeField] private GlobalEventSO<ActionData_SO> onActivityApplied;
    [Space]
    [SerializeField] private Internal_UI_Phone @internal;
    
    private PlayerManager playerManager;
    private DayTimeManager dayTimeManager;

    private void Start()
    {
        playerManager = PlayerManager.Instance;
        dayTimeManager = DayTimeManager.Instance;

        onActivityApplied.OnEventRaised += HandleActivityApplied;
        onDayEnd.OnEventRaised += HandleDayEnd;
        
        UpdateTime();
        UpdateStats();
    }

    private void HandleDayEnd(int value)
    {
        UpdateDay(value);
    }
    private void HandleActivityApplied(ActionData_SO action)
    {
        UpdateTime();
        UpdateStats();
    }

    private void UpdateDay(int value)
    {
        @internal.day.text = value.ToString();
    }
    private void UpdateTime()
    {
        @internal.time.text = dayTimeManager.RemainingHours.ToString();
    }
    private void UpdateStats()
    {
        @internal.health.text = playerManager.Health.ToString();
        @internal.money.text = playerManager.Money.ToString();
        @internal.stress.text = playerManager.Stress.ToString();
    }


    [Serializable] private class Internal_UI_Phone
    {
        public TMP_Text day;
        public TMP_Text time;
        public TMP_Text money;
        public TMP_Text stress;
        public TMP_Text health; 
    }
}

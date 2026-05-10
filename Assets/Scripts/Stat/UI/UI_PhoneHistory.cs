using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays the history of applied activities on the phone screen.
/// Wire the five TMP labels via Inspector or PhoneUIBuilder.
/// </summary>
public class UI_PhoneHistory : MonoBehaviour
{
    [Header("Activity Display")]
    [SerializeField] public TMP_Text activityName;

    [Header("Stat Chips")]
    [SerializeField] public TMP_Text chipTime;
    [SerializeField] public TMP_Text chipHealth;
    [SerializeField] public TMP_Text chipStress;
    [SerializeField] public TMP_Text chipMoney;

    private int                 index;
    private List<ActionData_SO> history;

    private void Start()
    {
        gameObject.SetActive(false);
        history = new List<ActionData_SO>();
        index   = 0;
    }

    public void AddToHistory(ActionData_SO action)
    {
        history.Add(action);
        index = history.Count - 1;
        Show(action);
        if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
    }

    public void ShowPrevious()
    {
        if (history == null || history.Count == 0) return;
        index = (index - 1 + history.Count) % history.Count;
        Show(history[index]);
    }

    public void ShowNext()
    {
        if (history == null || history.Count == 0) return;
        index = (index + 1) % history.Count;
        Show(history[index]);
    }

    private void Show(ActionData_SO a)
    {
        if (activityName != null) activityName.text = a.ActivityName;
        if (chipTime     != null) chipTime.text     = Delta(-a.TimeCost,  "h");
        if (chipHealth   != null) chipHealth.text   = Delta(a.Health,    " HP");
        if (chipStress   != null) chipStress.text   = Delta(-a.Stress,   " Str");
        if (chipMoney    != null) chipMoney.text     = DeltaMoney(-a.MoneyCost);
    }

    private static string Delta(int val, string unit)
    {
        string sign = val >= 0 ? "+" : "−";
        return $"{sign}{Mathf.Abs(val)}{unit}";
    }

    private static string DeltaMoney(int val)
    {
        string sign = val >= 0 ? "+" : "−";
        return $"{sign}${Mathf.Abs(val)}";
    }
}

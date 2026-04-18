using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;

public class ShowOnCanvas : MonoBehaviour
{
    [SerializeField] private Activity _activity;
    [SerializeField] private TMP_Text _money;
    [SerializeField] private TMP_Text _hours;
    [SerializeField] private TMP_Text _health;
    [SerializeField] private TMP_Text _stress;
    [SerializeField] private TMP_Text _name;

    private ActionData_SO _actionData;
    private void Awake()
    {
        _actionData = _activity.GetActivity();

        _money.text = TranslateActionDatatoString(_actionData.MoneyCost);
        _hours.text = TranslateActionDatatoString(_actionData.TimeCost);
        _stress.text = TranslateActionDatatoString(_actionData.Stress);
        _health.text = TranslateActionDatatoString(_actionData.Health);
        _name.text = _actionData.ActivityName;
    }

private string TranslateActionDatatoString(int data)
    {
        int value = data;
        string sign = value > 0 ? "+" : "";

        return sign + value.ToString();
    }
}

using UnityEngine;

[CreateAssetMenu(fileName = "ActivitySO", menuName = "Scriptable Objects/ActivitySO")]
public class ActivitySO : ScriptableObject
{
    [SerializeField] private string activityName;
    [SerializeField] private string activityDescription;
    [SerializeField] private string activityFlavorText;
    
    [SerializeField] private int timeCost;
    [SerializeField] private int moneyCost;
    [SerializeField] private int finance;
    [SerializeField] private int health;
    [SerializeField] private int stress;

    public string ActivityName => activityName;
    public string ActivityDescription => activityDescription;
    public string ActivityFlavorText => activityFlavorText;
    public int TimeCost => timeCost;
    public int MoneyCost => moneyCost;
    public int Finance => finance;
    public int Health => health;
    public int Stress => stress;
}

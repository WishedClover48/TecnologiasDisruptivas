using UnityEngine;

[CreateAssetMenu(menuName = "Action/Data")]
public class ActionData_SO : ScriptableObject
{
    [SerializeField, Min(0), Tooltip("Horas que cuesta esta acción al ejecutarse.")]
    private int timeCost;

    public int TimeCost => timeCost;
}

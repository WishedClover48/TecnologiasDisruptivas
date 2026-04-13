using UnityEngine;

public class DayTimeManager : MonoBehaviour
{
    public static DayTimeManager Instance { get; private set; }


    [Header("Day Configuration")]
    [SerializeField] private int totalDays      = 4;
    [SerializeField] private int minHoursPerDay = 8;
    [SerializeField] private int maxHoursPerDay = 12;


    [Header("Global Events — Output")]
    [SerializeField] private GlobalEventSO_Int  onDayEnded;
    [SerializeField] private GlobalEventSO_Void onNewDayStarted;
    [SerializeField] private GlobalEventSO_Void onGameEnded;

    [Header("Global Events — Input")]
    [SerializeField] private GlobalEventSO_ActionData onActionPerformed;

    public int CurrentDay     { get; private set; } = 1;
    public int MaxHoursToday  { get; private set; }
    public int RemainingHours { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        ValidateHourRange();
        RandomizeHoursForDay();
    }

    private void OnEnable()
    {
        if (onActionPerformed != null)
            onActionPerformed.OnEventRaised += OnActionReceived;
    }

    private void OnDisable()
    {
        if (onActionPerformed != null)
            onActionPerformed.OnEventRaised -= OnActionReceived;
    }
    
    public void SpendTime(int hours)
    {
        RemainingHours -= hours;

        if (RemainingHours <= 0)
            EndDay();
    }

    private void EndDay()
    {
        RemainingHours = 0;

        Debug.Log($"[DayTimeManager] Día {CurrentDay} terminó.");
        onDayEnded?.RaiseEvent(CurrentDay);

        if (CurrentDay < totalDays)
            StartNewDay();
        else
        {
            Debug.Log("[DayTimeManager] Todos los días completados. Fin del juego.");
            onGameEnded?.RaiseEvent();
        }
    }

    private void StartNewDay()
    {
        CurrentDay++;
        RandomizeHoursForDay();

        Debug.Log($"[DayTimeManager] Día {CurrentDay} iniciado. Horas disponibles: {MaxHoursToday}");
        onNewDayStarted?.RaiseEvent();
    }

    private void RandomizeHoursForDay()
    {
        MaxHoursToday  = Random.Range(minHoursPerDay, maxHoursPerDay + 1);
        RemainingHours = MaxHoursToday;
    }

    private void ValidateHourRange()
    {
        if (minHoursPerDay > maxHoursPerDay)
        {
            Debug.LogWarning("[DayTimeManager] minHoursPerDay es mayor que maxHoursPerDay. Valores intercambiados.");
            (minHoursPerDay, maxHoursPerDay) = (maxHoursPerDay, minHoursPerDay);
        }
    }

    private void OnActionReceived(ActionData_SO action)
    {
        SpendTime(action.TimeCost);
    }
}

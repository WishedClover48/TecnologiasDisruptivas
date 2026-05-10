using DefaultNamespace;
using TMPro;
using UnityEngine;

/// <summary>
/// Main controller for the diegetic phone screen.
/// Reads from DayTimeManager and PlayerManager singletons and drives
/// PhoneStatBar components + text labels. Reacts to all relevant global events.
/// </summary>
public class UI_Phone : MonoBehaviour
{
    public static UI_Phone Instance { get; private set; }

    // ── Header section ────────────────────────────────────────────────────
    [Header("Header")]
    [Tooltip("e.g. 'Day 2 / 4'")]
    [SerializeField] private TMP_Text dayLabel;
    [Tooltip("Horizontal bar showing remaining hours this day.")]
    [SerializeField] private PhoneStatBar hoursBar;
    [Tooltip("Numeric display inside / below the hours bar.")]
    [SerializeField] private TMP_Text hoursValueLabel;

    // ── Stat bars ─────────────────────────────────────────────────────────
    [Header("Stat Bars")]
    [SerializeField] private PhoneStatBar healthBar;
    [SerializeField] private PhoneStatBar mentalHealthBar;   // inverted Stress
    [SerializeField] private PhoneStatBar financeBar;

    // ── Money display ─────────────────────────────────────────────────────
    [Header("Money")]
    [SerializeField] private TMP_Text moneyLabel;

    // ── History ───────────────────────────────────────────────────────────
    [Header("History")]
    [SerializeField] private UI_PhoneHistory history;

    // ── Global events ─────────────────────────────────────────────────────
    [Header("Global Events")]
    [SerializeField] private GlobalEventSO_Int        onDayEnd;
    [SerializeField] private GlobalEventSO_ActionData onActivityApplied;
    [SerializeField] private GlobalEventSO_Void       onNewDayStarted;

    // ── Internal refs ─────────────────────────────────────────────────────
    private PlayerManager  pm;
    private DayTimeManager dtm;

    // ── Lifecycle ─────────────────────────────────────────────────────────

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        pm  = PlayerManager.Instance;
        dtm = DayTimeManager.Instance;

        if (onActivityApplied != null) onActivityApplied.OnEventRaised += OnActivityApplied;
        if (onDayEnd          != null) onDayEnd.OnEventRaised          += OnDayEnd;
        if (onNewDayStarted   != null) onNewDayStarted.OnEventRaised   += OnNewDayStarted;

        RefreshAll();
    }

    private void OnDestroy()
    {
        if (onActivityApplied != null) onActivityApplied.OnEventRaised -= OnActivityApplied;
        if (onDayEnd          != null) onDayEnd.OnEventRaised          -= OnDayEnd;
        if (onNewDayStarted   != null) onNewDayStarted.OnEventRaised   -= OnNewDayStarted;
    }

    // ── Event handlers ────────────────────────────────────────────────────

    private void OnActivityApplied(ActionData_SO action)
    {
        RefreshAll();
        history?.AddToHistory(action);
    }

    private void OnDayEnd(int _)   => RefreshAll();
    private void OnNewDayStarted() => RefreshAll();

    // ── Refresh ───────────────────────────────────────────────────────────

    private void RefreshAll()
    {
        RefreshHeader();
        RefreshStatBars();
        RefreshMoney();
    }

    private void RefreshHeader()
    {
        if (dayLabel != null)
            dayLabel.text = $"Day {dtm.CurrentDay} / {dtm.TotalDays}";

        float hoursNorm = dtm.MaxHoursToday > 0
            ? (float)dtm.RemainingHours / dtm.MaxHoursToday
            : 0f;

        // Drive the bar — label is handled by hoursValueLabel separately so
        // the bar itself stays clean (no text child needed inside the shader)
        hoursBar?.SetValue(hoursNorm);

        if (hoursValueLabel != null)
            hoursValueLabel.text = $"{dtm.RemainingHours}h left";
    }

    private void RefreshStatBars()
    {
        float health  = pm.Health  / 100f;
        float mental  = 1f - (pm.Stress  / 100f);   // high stress → low mental
        float finance = pm.Finance / 100f;

        healthBar?.SetValue(health,  $"{pm.Health}%");
        mentalHealthBar?.SetValue(mental,  $"{Mathf.RoundToInt(mental  * 100)}%");
        financeBar?.SetValue(finance, $"{pm.Finance}%");
    }

    private void RefreshMoney()
    {
        if (moneyLabel != null)
            moneyLabel.text = $"${pm.Money}";
    }
}

using UnityEngine;

public class TVMinigameController : MonoBehaviour
{
    [Header("Antenas")]
    [SerializeField] private AntennaController[] antennas;

    [Header("Config de rondas")]
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private float secondsAlignedToAdvance = 2f;

    [Header("Config de finalización")]
    [SerializeField] private ActionData_SO activityData;
    [SerializeField] private GlobalEventSO_ActionData onActivityDone;

    private float _alignedTimer;
    private int _currentRound;
    private bool _completed;

    private void Start()
    {
        StartRound();
    }

    private void StartRound()
    {
        _alignedTimer = 0f;
        foreach (var antenna in antennas)
            antenna.SetRandomTarget();

        Debug.Log($"Ronda {_currentRound + 1}/{totalRounds} iniciada");
    }

    private void Update()
    {
        if (_completed) return;

        bool allAligned = AreAllAntennasAligned();

        if (allAligned)
        {
            _alignedTimer += Time.deltaTime;
            if (_alignedTimer >= secondsAlignedToAdvance)
            {
                AdvanceRound();
            }
        }
        else
        {
            _alignedTimer = 0f;
        }
    }

    private bool AreAllAntennasAligned()
    {
        foreach (var antenna in antennas)
        {
            if (!antenna.IsAligned)
                return false;
        }
        return true;
    }

    private void AdvanceRound()
    {
        _currentRound++;

        if (_currentRound >= totalRounds)
        {
            CompleteActivity();
        }
        else
        {
            StartRound();
        }
    }

    private void CompleteActivity()
    {
        _completed = true;
        Debug.Log("Minijuego TV completado!");
        onActivityDone.RaiseEvent(activityData);
    }
}
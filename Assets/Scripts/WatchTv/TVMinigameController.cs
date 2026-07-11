using UnityEngine;
using UnityEngine.UI;

public class TVMinigameController : MonoBehaviour
{
    [Header("Antenas")]
    [SerializeField] private AntennaController[] antennas;

    [Header("Config de rondas")]
    [SerializeField] private int totalRounds = 3;
    [SerializeField] private float secondsAlignedToAdvance = 2f;

    [Header("Feedback visual")]
    [SerializeField] private GameObject imageBad;   // Image_Bad
    [SerializeField] private GameObject imageGood;  // Image_Good

    [Header("Config de finalización")]
    [SerializeField] private ActionData_SO activityData;
    [SerializeField] private GlobalEventSO_ActionData onActivityDone;

    private float _alignedTimer;
    private int _currentRound;
    private bool _completed;
    private bool _wasAligned; // para no actualizar la UI en cada frame innecesariamente

    private void Start()
    {
        StartRound();
        SetFeedback(false);
    }

    private void Update()
    {
        if (_completed) return;

        bool allAligned = AreAllAntennasAligned();

        // Solo actualizamos el feedback cuando cambia el estado
        if (allAligned != _wasAligned)
        {
            SetFeedback(allAligned);
            _wasAligned = allAligned;
        }

        if (allAligned)
        {
            _alignedTimer += Time.deltaTime;
            if (_alignedTimer >= secondsAlignedToAdvance)
                AdvanceRound();
        }
        else
        {
            _alignedTimer = 0f;
        }
    }

    private void SetFeedback(bool isGood)
    {
        if (imageBad != null)  imageBad.SetActive(!isGood);
        if (imageGood != null) imageGood.SetActive(isGood);
    }

    private bool AreAllAntennasAligned()
    {
        foreach (var antenna in antennas)
            if (!antenna.IsAligned) return false;
        return true;
    }

    private void StartRound()
    {
        _alignedTimer = 0f;
        foreach (var antenna in antennas)
            antenna.SetRandomTarget();

        Debug.Log($"Ronda {_currentRound + 1}/{totalRounds} iniciada");
    }

    private void AdvanceRound()
    {
        _currentRound++;
        if (_currentRound >= totalRounds)
            CompleteActivity();
        else
            StartRound();
    }

    private void CompleteActivity()
    {
        _completed = true;
        SetFeedback(true); // queda en verde al terminar
        Debug.Log("Minijuego TV completado!");
        onActivityDone.RaiseEvent(activityData);
    }
}
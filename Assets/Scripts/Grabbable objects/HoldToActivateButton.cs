using Oculus.Interaction;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class HoldToActivateButton : MonoBehaviour
{
    [Header("Interaction")]
    [SerializeField] private GrabInteractable _grabInteractable;
    [SerializeField] private float _holdDuration = 1.5f;

    [Header("Feedback")]
    [SerializeField] private Image _progress;

    [Header("Event")]
    [SerializeField] private UnityEvent _onActivated;

    private Coroutine _fillRoutine;
    private bool      _activated;

    private void Awake()
    {
        if (_grabInteractable == null)
            _grabInteractable = GetComponent<GrabInteractable>();
    }

    private void Start()
    {
        if (_progress != null) _progress.fillAmount = 0f;
    }

    private void OnEnable()
    {
        if (_grabInteractable != null)
            _grabInteractable.WhenStateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        if (_grabInteractable != null)
            _grabInteractable.WhenStateChanged -= OnStateChanged;
        StopFill();
    }

    private void OnStateChanged(InteractableStateChangeArgs args)
    {
        if (_activated) return;

        if (args.NewState == InteractableState.Select)
            BeginFill();
        else
            StopFill();
    }

    private void BeginFill()
    {
        StopFill();
        _fillRoutine = StartCoroutine(FillRoutine());
    }

    private IEnumerator FillRoutine()
    {
        float elapsed = 0f;
        while (elapsed < _holdDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            if (_progress != null)
                _progress.fillAmount = Mathf.Clamp01(elapsed / _holdDuration);
            yield return null;
        }

        if (_progress != null) _progress.fillAmount = 1f;
        _activated   = true;
        _fillRoutine = null;
        _onActivated?.Invoke();
    }

    private void StopFill()
    {
        if (_fillRoutine != null)
        {
            StopCoroutine(_fillRoutine);
            _fillRoutine = null;
        }
        if (_progress != null && !_activated)
            _progress.fillAmount = 0f;
    }
}

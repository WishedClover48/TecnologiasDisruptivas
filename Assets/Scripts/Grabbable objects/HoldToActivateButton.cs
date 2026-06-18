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

    [Header("Trigger fallback (works at any distance)")]
    [SerializeField] private bool _allowTriggerHold = true;
    [SerializeField] private TriggerSide _triggerSide = TriggerSide.Any;

    public enum TriggerSide { Any, Left, Right }

    [Header("Feedback")]
    [SerializeField] private Image _progress;

    [Header("Event")]
    [SerializeField] private UnityEvent _onActivated;

    private Coroutine _fillRoutine;
    private bool      _activated;

    private void Awake()
    {
        if (_grabInteractable == null)
            _grabInteractable = GetComponentInChildren<GrabInteractable>(true);
    }

    private void OnEnable()
    {
        _activated = false;
        if (_progress != null) _progress.fillAmount = 0f;
    }

    private void Update()
    {
        if (_activated) return;

        if (ShouldFill())
            BeginFill();
        else
            StopFill();
    }

    private bool ShouldFill()
    {
        if (_grabInteractable != null && _grabInteractable.State == InteractableState.Select)
            return true;

        if (_allowTriggerHold && TriggerHeld())
            return true;

        return false;
    }

    private bool TriggerHeld()
    {
        switch (_triggerSide)
        {
            case TriggerSide.Left:  return Trigger(OVRInput.Controller.LTouch);
            case TriggerSide.Right: return Trigger(OVRInput.Controller.RTouch);
            default:                return Trigger(OVRInput.Controller.LTouch)
                                        || Trigger(OVRInput.Controller.RTouch);
        }
    }

    private static bool Trigger(OVRInput.Controller controller)
    {
        return OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger, controller) > 0.5f
            || OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger,  controller) > 0.5f;
    }

    private void BeginFill()
    {
        if (_fillRoutine == null)
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

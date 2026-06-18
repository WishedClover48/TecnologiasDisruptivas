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

    [Header("Aim + trigger (point the laser, then hold trigger)")]
    [SerializeField] private bool _allowTriggerHold = true;
    [SerializeField] private bool _requireAim = true;
    [SerializeField] private float _rayLength = 8f;
    [SerializeField] private TriggerSide _triggerSide = TriggerSide.Any;

    public enum TriggerSide { Any, Left, Right }

    [Header("Keep static (don't move when grabbed)")]
    [SerializeField] private bool _lockInPlace = true;

    [Header("Feedback")]
    [SerializeField] private Image _progress;

    [Header("Event")]
    [SerializeField] private UnityEvent _onActivated;

    private Coroutine  _fillRoutine;
    private bool       _activated;
    private Vector3    _lockedPos;
    private Quaternion _lockedRot;
    private bool       _poseCached;
    private Transform  _rightAnchor;
    private Transform  _leftAnchor;

    private void Awake()
    {
        if (_grabInteractable == null)
            _grabInteractable = GetComponentInChildren<GrabInteractable>(true);

        _rightAnchor = FindAnchor("RightControllerAnchor", "RightHandAnchor");
        _leftAnchor  = FindAnchor("LeftControllerAnchor",  "LeftHandAnchor");
    }

    private static Transform FindAnchor(string primary, string fallback)
    {
        var go = GameObject.Find(primary) ?? GameObject.Find(fallback);
        return go != null ? go.transform : null;
    }

    private void OnEnable()
    {
        _activated = false;
        if (_progress != null) _progress.fillAmount = 0f;

        _lockedPos  = transform.localPosition;
        _lockedRot  = transform.localRotation;
        _poseCached = true;
    }

    private void LateUpdate()
    {
        if (_lockInPlace && _poseCached)
        {
            transform.localPosition = _lockedPos;
            transform.localRotation = _lockedRot;
        }
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

        if (_allowTriggerHold && TriggerConfirm())
            return true;

        return false;
    }

    private bool TriggerConfirm()
    {
        if (!_requireAim)
        {
            return (SideAllows(TriggerSide.Right) && Trigger(OVRInput.Controller.RTouch))
                || (SideAllows(TriggerSide.Left)  && Trigger(OVRInput.Controller.LTouch));
        }

        if (SideAllows(TriggerSide.Right) && Aimed(_rightAnchor) && Trigger(OVRInput.Controller.RTouch))
            return true;
        if (SideAllows(TriggerSide.Left) && Aimed(_leftAnchor) && Trigger(OVRInput.Controller.LTouch))
            return true;

        return false;
    }

    private bool SideAllows(TriggerSide side)
    {
        return _triggerSide == TriggerSide.Any || _triggerSide == side;
    }

    private bool Aimed(Transform anchor)
    {
        if (anchor == null) return false;
        if (!Physics.Raycast(anchor.position, anchor.forward, out RaycastHit hit, _rayLength))
            return false;
        return hit.transform == transform || hit.transform.IsChildOf(transform);
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

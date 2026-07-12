using Oculus.Interaction;
using Oculus.Interaction.HandGrab;
using Oculus.Interaction.Input;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DumbbellRepCounter : MonoBehaviour
{
    [SerializeField] private Transform _trackedObject;
    [SerializeField] private GrabInteractable _grabInteractable;
    [SerializeField] private HandGrabInteractable _handGrabInteractable;
    [SerializeField] private GameObject _uiRoot;
    [SerializeField] private TMP_Text _statusText;
    [SerializeField] private Image _progressFill;
    [SerializeField] private int _targetReps = 10;
    [SerializeField] private ExerciseHand _requiredHand = ExerciseHand.Any;
    [SerializeField] private float _liftHeight = 0.45f;
    [SerializeField] private float _loweredTolerance = 0.12f;
    [SerializeField] private bool _allowKeyboardDebug;
    [SerializeField] private UnityEvent _onCompleted;
    [Header("Rep Origin")]
    [SerializeField] private Transform _repOrigin;
    [SerializeField] private float _repOriginHeightOffset = 0f;
    [SerializeField] private bool _followRepOrigin = true;
    
    public event Action<DumbbellRepCounter, bool, ExerciseHand> GrabStateChanged;
    public event Action<DumbbellRepCounter, int, int> RepChanged;
    public event Action<DumbbellRepCounter> SetCompleted;

    private int _currentReps;
    private float _baselineHeight;
    private bool _isGrabbed;
    private bool _waitingForLower;
    private bool _completed;
    private bool _subscribed;
    private ExerciseHand _holdingHand = ExerciseHand.Any;
    private Func<bool> _canCountProvider;

    public int CurrentReps => _currentReps;
    public int TargetReps => _targetReps;
    public ExerciseHand HoldingHand => _holdingHand;
    public bool IsGrabbed => _isGrabbed;

    public void Configure(Transform trackedObject, GrabInteractable grabInteractable, TMP_Text statusText, Image progressFill, int targetReps)
    {
        _trackedObject = trackedObject;
        _grabInteractable = grabInteractable;
        _statusText = statusText;
        _progressFill = progressFill;
        _targetReps = Mathf.Max(1, targetReps);
        if (_uiRoot == null)
        {
            _uiRoot = FindUiRoot();
        }
        SubscribeToGrabEvents();
        ResetWorkout();
    }

    public void BeginSet(int targetReps, ExerciseHand requiredHand)
    {
        _targetReps = Mathf.Max(1, targetReps);
        _requiredHand = requiredHand;
        ResetWorkout();
        RefreshGrabStateFromCurrentSelection();
    }

    public void SetCanCountProvider(Func<bool> canCountProvider)
    {
        _canCountProvider = canCountProvider;
    }

    private void Awake()
    {
        if (_grabInteractable == null)
        {
            _grabInteractable = GetComponent<GrabInteractable>();
        }

        if (_handGrabInteractable == null)
        {
            _handGrabInteractable = GetComponent<HandGrabInteractable>();
        }

        if (_trackedObject == null)
        {
            _trackedObject = transform;
        }

        if (_uiRoot == null)
        {
            _uiRoot = FindUiRoot();
        }

        _targetReps = Mathf.Max(1, _targetReps);
        ResetWorkout();
    }

    private void OnEnable()
    {
        SubscribeToGrabEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromGrabEvents();
    }

    private void Update()
    {
        if (_completed)
        {
            return;
        }

        if (_allowKeyboardDebug && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            AddRep();
            return;
        }

        if (!_isGrabbed || _trackedObject == null || !IsHoldingWithRequiredHand())
        {
            return;
        }

        if (!CanCount())
        {
            UpdateStatus("Stand in the circle");
            return;
        }

        float relativeHeight = _trackedObject.position.y - GetRepBaselineHeight();

        if (!_waitingForLower && relativeHeight >= _liftHeight)
        {
            AddRep();
            _waitingForLower = true;
        }
        else if (_waitingForLower && relativeHeight <= _loweredTolerance)
        {
            _waitingForLower = false;
            UpdateStatus("Lift again");
        }
    }

    public void SetRepOrigin(Transform repOrigin, float heightOffset = 0f)
    {
        _repOrigin = repOrigin;
        _repOriginHeightOffset = heightOffset;
        CaptureBaseline();
    }

    private void CaptureBaseline()
    {
        if (_repOrigin != null)
        {
            _baselineHeight = _repOrigin.position.y + _repOriginHeightOffset;
            return;
        }

        _baselineHeight = _trackedObject != null ? _trackedObject.position.y : transform.position.y;
    }

    private float GetRepBaselineHeight()
    {
        if (_repOrigin != null && _followRepOrigin)
        {
            return _repOrigin.position.y + _repOriginHeightOffset;
        }

        return _baselineHeight;
    }
    
    [ContextMenu("Reset Workout")]
    public void ResetWorkout()
    {
        _currentReps = 0;
        _completed = false;
        _waitingForLower = false;
        _isGrabbed = false;
        _holdingHand = ExerciseHand.Any;
        CaptureBaseline();
        UpdateStatus("Grab the dumbbell");
        UpdateProgress();
        SetUiVisible(false);
        RepChanged?.Invoke(this, _currentReps, _targetReps);
        GrabStateChanged?.Invoke(this, false, _holdingHand);
    }

    private void HandleGrabSelected(GrabInteractor interactor)
    {
        HandleGrabbed(ResolveHand(interactor));
    }

    private void HandleGrabReleased(GrabInteractor interactor)
    {
        HandleReleased();
    }

    private void HandleHandGrabSelected(HandGrabInteractor interactor)
    {
        HandleGrabbed(ResolveHand(interactor));
    }

    private void HandleHandGrabReleased(HandGrabInteractor interactor)
    {
        HandleReleased();
    }

    private void HandleGrabbed(ExerciseHand hand)
    {
        _isGrabbed = true;
        _holdingHand = hand;
        CaptureBaseline();
        _waitingForLower = false;
        SetUiVisible(false);
        UpdateStatus(IsHoldingWithRequiredHand() ? "Lift up" : $"Use your {HandToText(_requiredHand)} hand");
        GrabStateChanged?.Invoke(this, true, _holdingHand);
    }

    private void HandleReleased()
    {
        _isGrabbed = false;
        _holdingHand = ExerciseHand.Any;

        if (!_completed)
        {
            SetUiVisible(false);
            UpdateStatus("Grab the dumbbell");
        }

        GrabStateChanged?.Invoke(this, false, _holdingHand);
    }

    private void AddRep()
    {
        if (_completed)
        {
            return;
        }

        _currentReps++;
        UpdateProgress();
        RepChanged?.Invoke(this, _currentReps, _targetReps);

        if (_currentReps >= _targetReps)
        {
            _completed = true;
            SetUiVisible(false);
            UpdateStatus("Workout complete");
            SetCompleted?.Invoke(this);
            _onCompleted?.Invoke();
            return;
        }

        UpdateStatus("Lower it");
    }

    private void UpdateProgress()
    {
        if (_progressFill != null)
        {
            _progressFill.fillAmount = Mathf.Clamp01((float)_currentReps / _targetReps);
        }
    }

    private void UpdateStatus(string instruction)
    {
        if (_statusText == null)
        {
            return;
        }

        _statusText.text = $"{instruction}\n{_currentReps}/{_targetReps} reps";
    }

    private void SetUiVisible(bool visible)
    {
        if (_uiRoot != null && _uiRoot.activeSelf != visible)
        {
            _uiRoot.SetActive(visible);
        }
    }

    private GameObject FindUiRoot()
    {
        if (_statusText != null)
        {
            Canvas canvas = _statusText.GetComponentInParent<Canvas>(true);
            if (canvas != null)
            {
                return canvas.gameObject;
            }
        }

        if (_progressFill != null)
        {
            Canvas canvas = _progressFill.GetComponentInParent<Canvas>(true);
            if (canvas != null)
            {
                return canvas.gameObject;
            }
        }

        return null;
    }

    private void SubscribeToGrabEvents()
    {
        if (_subscribed)
        {
            return;
        }

        if (_grabInteractable != null)
        {
            _grabInteractable.WhenSelectingInteractorAdded.Action += HandleGrabSelected;
            _grabInteractable.WhenSelectingInteractorRemoved.Action += HandleGrabReleased;
        }

        if (_handGrabInteractable != null)
        {
            _handGrabInteractable.WhenSelectingInteractorAdded.Action += HandleHandGrabSelected;
            _handGrabInteractable.WhenSelectingInteractorRemoved.Action += HandleHandGrabReleased;
        }

        _subscribed = true;
    }

    private void UnsubscribeFromGrabEvents()
    {
        if (!_subscribed)
        {
            return;
        }

        if (_grabInteractable != null)
        {
            _grabInteractable.WhenSelectingInteractorAdded.Action -= HandleGrabSelected;
            _grabInteractable.WhenSelectingInteractorRemoved.Action -= HandleGrabReleased;
        }

        if (_handGrabInteractable != null)
        {
            _handGrabInteractable.WhenSelectingInteractorAdded.Action -= HandleHandGrabSelected;
            _handGrabInteractable.WhenSelectingInteractorRemoved.Action -= HandleHandGrabReleased;
        }

        _subscribed = false;
    }

    private bool CanCount()
    {
        return _canCountProvider == null || _canCountProvider();
    }

    private void RefreshGrabStateFromCurrentSelection()
    {
        if (_handGrabInteractable != null)
        {
            foreach (HandGrabInteractor interactor in _handGrabInteractable.SelectingInteractors)
            {
                HandleGrabbed(ResolveHand(interactor));
                return;
            }
        }

        if (_grabInteractable != null)
        {
            foreach (GrabInteractor interactor in _grabInteractable.SelectingInteractors)
            {
                HandleGrabbed(ResolveHand(interactor));
                return;
            }
        }
    }

    private bool IsHoldingWithRequiredHand()
    {
        return _requiredHand == ExerciseHand.Any || _holdingHand == _requiredHand;
    }

    private ExerciseHand ResolveHand(HandGrabInteractor interactor)
    {
        if (interactor != null && interactor.Hand != null)
        {
            return FromMetaHandedness(interactor.Hand.Handedness);
        }

        return ResolveHandFromHierarchy(interactor);
    }

    private ExerciseHand ResolveHand(GrabInteractor interactor)
    {
        return ResolveHandFromHierarchy(interactor);
    }

    private ExerciseHand ResolveHandFromHierarchy(Component component)
    {
        if (component == null)
        {
            return ExerciseHand.Any;
        }

        MonoBehaviour[] behaviours = component.GetComponentsInParent<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IHand hand)
            {
                return FromMetaHandedness(hand.Handedness);
            }

            if (behaviour is IController controller)
            {
                return FromMetaHandedness(controller.Handedness);
            }
        }

        Transform current = component.transform;
        while (current != null)
        {
            string lowerName = current.name.ToLowerInvariant();
            if (lowerName.Contains("left"))
            {
                return ExerciseHand.Left;
            }

            if (lowerName.Contains("right"))
            {
                return ExerciseHand.Right;
            }

            current = current.parent;
        }

        return ExerciseHand.Any;
    }

    private ExerciseHand FromMetaHandedness(Handedness handedness)
    {
        return handedness == Handedness.Left ? ExerciseHand.Left : ExerciseHand.Right;
    }

    private string HandToText(ExerciseHand hand)
    {
        switch (hand)
        {
            case ExerciseHand.Left:
                return "left";
            case ExerciseHand.Right:
                return "right";
            default:
                return "either";
        }
    }
}

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ExerciseTVGuide : MonoBehaviour
{
    [Serializable]
    public class ExerciseTarget
    {
        public DumbbellRepCounter Dumbbell;
        public string ObjectName = "blue dumbbell";
        public ExerciseHand RequiredHand = ExerciseHand.Right;
        public int TargetReps = 10;

        [NonSerialized] public int Reps;
        [NonSerialized] public bool Complete;
        [NonSerialized] public bool ReleasedEarly;
        [NonSerialized] public bool HeldByWrongHand;
    }

    [Serializable]
    public class ExercisePhase
    {
        [TextArea] public string StartMessage = "Grab the blue dumbbell with your right hand.";
        public string CounterFormat = "{hand} hand: {reps}/{target} reps";
        [TextArea] public string ReleasedMessage = "Pick up the {object} with your {hand} hand.";
        [TextArea] public string WrongHandMessage = "Use your {hand} hand for the {object}.";
        public ExerciseTarget[] Targets;
    }

    [SerializeField] private TMP_Text _messageText;
    [SerializeField] private GameObject _displayRoot;
    [SerializeField] private ExercisePhase[] _phases;
    [SerializeField, TextArea] private string _completeMessage = "Exercise complete.";
    [SerializeField] private bool _createDefaultTvIfMissing = true;
    [SerializeField] private bool _beginOnEnable;
    [SerializeField] private bool _hideDisplayUntilStarted = true;
    [SerializeField] private UnityEvent _onExerciseCompleted;
    [Header("Exercise Zone")]
    [SerializeField] private bool _requirePlayerInCircle = true;
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private Transform _exerciseCircleCenter;
    [SerializeField] private float _exerciseCircleRadius = 0.85f;
    [SerializeField, TextArea] private string _outsideCircleMessage = "Stand in the circle in front of the TV.";

    private int _currentPhaseIndex;
    private bool _exerciseRunning;
    private bool _completionRaised;
    private bool _targetsSubscribed;

    public event Action ExerciseCompleted;

    private ExercisePhase CurrentPhase =>
        _phases != null && _currentPhaseIndex >= 0 && _currentPhaseIndex < _phases.Length
            ? _phases[_currentPhaseIndex]
            : null;

    private void Awake()
    {
        if (_messageText == null)
        {
            _messageText = GetComponentInChildren<TMP_Text>(true);
        }

        if (_messageText == null && _createDefaultTvIfMissing)
        {
            CreateDefaultTv();
        }

        if (_exerciseCircleCenter == null && _createDefaultTvIfMissing)
        {
            CreateDefaultCircle();
        }

        EnsureDefaultPhases();

        if (_hideDisplayUntilStarted)
        {
            SetDisplayVisible(false);
        }
        else
        {
            SetMessage(string.Empty);
        }
    }

    private void Reset()
    {
        EnsureDefaultPhases();
    }

    private void OnEnable()
    {
        SubscribeTargets();
        if (_beginOnEnable)
        {
            BeginExercise();
        }
        else if (_hideDisplayUntilStarted)
        {
            SetDisplayVisible(false);
        }
    }

    private void OnDisable()
    {
        ClearTargetCountProviders();
        UnsubscribeTargets();
    }

    private void Update()
    {
        ExercisePhase phase = CurrentPhase;
        ExerciseTarget target = phase != null ? FirstIncompleteTarget(phase) : null;
        if (_exerciseRunning && target != null && target.Dumbbell != null && target.Dumbbell.IsGrabbed)
        {
            UpdateMessage();
        }
    }

    [ContextMenu("Begin Exercise")]
    public void BeginExercise()
    {
        _exerciseRunning = true;
        _completionRaised = false;
        SetDisplayVisible(true);
        _currentPhaseIndex = 0;
        BeginCurrentPhase();
    }

    private void BeginCurrentPhase()
    {
        ExercisePhase phase = CurrentPhase;
        if (phase == null)
        {
            _exerciseRunning = false;
            ClearTargetCountProviders();
            SetMessage(_completeMessage);
            RaiseExerciseCompleted();
            return;
        }

        if (phase.Targets != null)
        {
            foreach (ExerciseTarget target in phase.Targets)
            {
                if (target == null || target.Dumbbell == null)
                {
                    continue;
                }

                target.Reps = 0;
                target.Complete = false;
                target.ReleasedEarly = false;
                target.HeldByWrongHand = false;
                target.TargetReps = Mathf.Max(1, target.TargetReps);
                target.Dumbbell.SetCanCountProvider(IsPlayerInCircle);
                target.Dumbbell.BeginSet(target.TargetReps, target.RequiredHand);
            }
        }

        SetMessage(phase.StartMessage);
    }

    private void HandleDumbbellGrabChanged(DumbbellRepCounter dumbbell, bool isGrabbed, ExerciseHand hand)
    {
        if (!_exerciseRunning)
        {
            return;
        }

        ExercisePhase phase = CurrentPhase;
        ExerciseTarget target = FindCurrentTarget(dumbbell);
        if (phase == null || target == null || target.Complete)
        {
            return;
        }

        target.HeldByWrongHand = isGrabbed && target.RequiredHand != ExerciseHand.Any && hand != target.RequiredHand;
        target.ReleasedEarly = !isGrabbed && !target.Complete;

        UpdateMessage();
    }

    private void HandleDumbbellRepChanged(DumbbellRepCounter dumbbell, int reps, int targetReps)
    {
        if (!_exerciseRunning)
        {
            return;
        }

        ExerciseTarget target = FindCurrentTarget(dumbbell);
        if (target == null)
        {
            return;
        }

        target.Reps = reps;
        target.TargetReps = targetReps;
        target.ReleasedEarly = false;
        target.HeldByWrongHand = false;
        UpdateMessage();
    }

    private void HandleDumbbellCompleted(DumbbellRepCounter dumbbell)
    {
        if (!_exerciseRunning)
        {
            return;
        }

        ExerciseTarget target = FindCurrentTarget(dumbbell);
        if (target == null)
        {
            return;
        }

        target.Complete = true;
        target.ReleasedEarly = false;
        target.HeldByWrongHand = false;

        if (IsCurrentPhaseComplete())
        {
            _currentPhaseIndex++;
            BeginCurrentPhase();
            return;
        }

        UpdateMessage();
    }

    private void UpdateMessage()
    {
        ExercisePhase phase = CurrentPhase;
        if (phase == null)
        {
            SetMessage(_completeMessage);
            return;
        }

        ExerciseTarget target = FirstIncompleteTarget(phase);
        if (target == null)
        {
            SetMessage(_completeMessage);
            return;
        }

        if (target.HeldByWrongHand)
        {
            SetMessage(FormatMessage(phase.WrongHandMessage, target));
            return;
        }

        if (target.ReleasedEarly)
        {
            SetMessage(FormatMessage(phase.ReleasedMessage, target));
            return;
        }

        if (target.Dumbbell != null && !target.Dumbbell.IsGrabbed && target.Reps == 0)
        {
            SetMessage(phase.StartMessage);
            return;
        }

        if (target.Dumbbell != null && target.Dumbbell.IsGrabbed && !IsPlayerInCircle())
        {
            SetMessage(FormatMessage(_outsideCircleMessage, target));
            return;
        }

        SetMessage(FormatMessage(phase.CounterFormat, target));
    }

    private bool IsCurrentPhaseComplete()
    {
        ExercisePhase phase = CurrentPhase;
        if (phase == null || phase.Targets == null || phase.Targets.Length == 0)
        {
            return true;
        }

        foreach (ExerciseTarget target in phase.Targets)
        {
            if (target != null && !target.Complete)
            {
                return false;
            }
        }

        return true;
    }

    private ExerciseTarget FirstIncompleteTarget(ExercisePhase phase)
    {
        if (phase.Targets == null)
        {
            return null;
        }

        foreach (ExerciseTarget target in phase.Targets)
        {
            if (target != null && !target.Complete)
            {
                return target;
            }
        }

        return null;
    }

    private ExerciseTarget FindCurrentTarget(DumbbellRepCounter dumbbell)
    {
        ExercisePhase phase = CurrentPhase;
        if (phase == null || phase.Targets == null)
        {
            return null;
        }

        foreach (ExerciseTarget target in phase.Targets)
        {
            if (target != null && target.Dumbbell == dumbbell)
            {
                return target;
            }
        }

        return null;
    }

    private string FormatMessage(string template, ExerciseTarget target)
    {
        string hand = HandToText(target.RequiredHand);
        return template
            .Replace("{object}", target.ObjectName)
            .Replace("{hand}", hand)
            .Replace("{reps}", target.Reps.ToString())
            .Replace("{target}", target.TargetReps.ToString());
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

    private void SetMessage(string message)
    {
        if (_messageText != null)
        {
            _messageText.text = message;
        }
    }

    private void SetDisplayVisible(bool visible)
    {
        if (_displayRoot != null)
        {
            _displayRoot.SetActive(visible);
            return;
        }

        if (_messageText != null)
        {
            _messageText.gameObject.SetActive(visible);
            if (!visible)
            {
                _messageText.text = string.Empty;
            }
        }
    }

    private void RaiseExerciseCompleted()
    {
        if (_completionRaised)
        {
            return;
        }

        _completionRaised = true;
        ExerciseCompleted?.Invoke();
        _onExerciseCompleted?.Invoke();
    }

    private bool IsPlayerInCircle()
    {
        if (!_requirePlayerInCircle || _exerciseCircleCenter == null)
        {
            return true;
        }

        Transform player = ResolvePlayerTransform();
        if (player == null)
        {
            return true;
        }

        Vector3 playerPosition = player.position;
        Vector3 circlePosition = _exerciseCircleCenter.position;
        Vector2 playerXZ = new Vector2(playerPosition.x, playerPosition.z);
        Vector2 circleXZ = new Vector2(circlePosition.x, circlePosition.z);
        return Vector2.Distance(playerXZ, circleXZ) <= _exerciseCircleRadius;
    }

    private Transform ResolvePlayerTransform()
    {
        if (_playerTransform != null)
        {
            return _playerTransform;
        }

        Camera mainCamera = Camera.main;
        return mainCamera != null ? mainCamera.transform : null;
    }

    private void SubscribeTargets()
    {
        if (_targetsSubscribed)
        {
            return;
        }

        ForEachUniqueDumbbell(dumbbell =>
        {
            dumbbell.GrabStateChanged += HandleDumbbellGrabChanged;
            dumbbell.RepChanged += HandleDumbbellRepChanged;
            dumbbell.SetCompleted += HandleDumbbellCompleted;
        });

        _targetsSubscribed = true;
    }

    private void UnsubscribeTargets()
    {
        if (!_targetsSubscribed)
        {
            return;
        }

        ForEachUniqueDumbbell(dumbbell =>
        {
            dumbbell.GrabStateChanged -= HandleDumbbellGrabChanged;
            dumbbell.RepChanged -= HandleDumbbellRepChanged;
            dumbbell.SetCompleted -= HandleDumbbellCompleted;
        });

        _targetsSubscribed = false;
    }

    private void ClearTargetCountProviders()
    {
        ForEachUniqueDumbbell(dumbbell => dumbbell.SetCanCountProvider(null));
    }

    private void ForEachUniqueDumbbell(Action<DumbbellRepCounter> action)
    {
        if (_phases == null)
        {
            return;
        }

        for (int phaseIndex = 0; phaseIndex < _phases.Length; phaseIndex++)
        {
            ExercisePhase phase = _phases[phaseIndex];
            if (phase == null || phase.Targets == null)
            {
                continue;
            }

            for (int targetIndex = 0; targetIndex < phase.Targets.Length; targetIndex++)
            {
                DumbbellRepCounter dumbbell = phase.Targets[targetIndex]?.Dumbbell;
                if (dumbbell == null || HasAppearedBefore(dumbbell, phaseIndex, targetIndex))
                {
                    continue;
                }

                action(dumbbell);
            }
        }
    }

    private bool HasAppearedBefore(DumbbellRepCounter dumbbell, int phaseIndex, int targetIndex)
    {
        for (int i = 0; i <= phaseIndex; i++)
        {
            ExercisePhase phase = _phases[i];
            if (phase == null || phase.Targets == null)
            {
                continue;
            }

            int maxTarget = i == phaseIndex ? targetIndex : phase.Targets.Length;
            for (int j = 0; j < maxTarget; j++)
            {
                if (phase.Targets[j]?.Dumbbell == dumbbell)
                {
                    return true;
                }
            }
        }

        return false;
    }

    private void EnsureDefaultPhases()
    {
        if (_phases != null && _phases.Length > 0)
        {
            return;
        }

        _phases = new[]
        {
            new ExercisePhase
            {
                StartMessage = "Grab the blue dumbbell with your right hand.",
                Targets = new[]
                {
                    new ExerciseTarget
                    {
                        ObjectName = "blue dumbbell",
                        RequiredHand = ExerciseHand.Right,
                        TargetReps = 10
                    }
                }
            },
            new ExercisePhase
            {
                StartMessage = "Grab the blue dumbbell with your left hand.",
                Targets = new[]
                {
                    new ExerciseTarget
                    {
                        ObjectName = "blue dumbbell",
                        RequiredHand = ExerciseHand.Left,
                        TargetReps = 10
                    }
                }
            }
        };
    }

    private void CreateDefaultTv()
    {
        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Cube);
        body.name = "TV Body";
        body.transform.SetParent(transform, false);
        body.transform.localPosition = Vector3.zero;
        body.transform.localScale = new Vector3(1.45f, 0.85f, 0.08f);
        body.GetComponent<Renderer>().material = CreateMaterial(new Color(0.035f, 0.037f, 0.04f));
        Destroy(body.GetComponent<Collider>());

        GameObject screen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        screen.name = "TV Screen";
        screen.transform.SetParent(transform, false);
        screen.transform.localPosition = new Vector3(0f, 0f, -0.045f);
        screen.transform.localScale = new Vector3(1.24f, 0.65f, 0.015f);
        screen.GetComponent<Renderer>().material = CreateMaterial(new Color(0.015f, 0.035f, 0.06f));
        Destroy(screen.GetComponent<Collider>());

        GameObject canvasObject = new GameObject("TV Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(transform, false);
        canvasObject.transform.localPosition = new Vector3(0f, 0f, -0.058f);
        canvasObject.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        canvasObject.transform.localScale = Vector3.one * 0.0018f;

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(620f, 320f);

        GameObject textObject = new GameObject("Message", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(canvasObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = new Vector2(32f, 24f);
        textRect.offsetMax = new Vector2(-32f, -24f);

        _messageText = textObject.GetComponent<TMP_Text>();
        _messageText.alignment = TextAlignmentOptions.Center;
        _messageText.fontSize = 48f;
        _messageText.color = Color.white;
        _displayRoot = canvasObject;
    }

    private void CreateDefaultCircle()
    {
        GameObject circle = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        circle.name = "Exercise Circle";
        circle.transform.SetParent(transform, false);
        circle.transform.localPosition = new Vector3(0f, -1.25f, -1.25f);
        circle.transform.localScale = new Vector3(_exerciseCircleRadius * 2f, 0.01f, _exerciseCircleRadius * 2f);
        circle.GetComponent<Renderer>().material = CreateMaterial(new Color(0.08f, 0.45f, 0.95f, 0.55f));
        Destroy(circle.GetComponent<Collider>());
        _exerciseCircleCenter = circle.transform;
    }

    private Material CreateMaterial(Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }

        Material material = new Material(shader);
        material.color = color;
        return material;
    }
}

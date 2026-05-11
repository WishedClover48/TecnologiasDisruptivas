using UnityEngine;
using DefaultNamespace;
public class ExerciseActivityStarter : MonoBehaviour, IActivityCompletionHandler
{
    [SerializeField] private ExerciseTVGuide _exerciseGuide;
    [SerializeField] private Activity _activity;
    [SerializeField] private GameObject _tvRoot;
    [SerializeField] private bool _hideTvOnStart = true;

    private bool _exerciseStarted;
    private bool _rewardApplied;
    private bool _subscribed;

    public bool IsHandlingActivity => _exerciseStarted;

    private void Awake()
    {
        if (_activity == null)
        {
            _activity = GetComponent<Activity>();
        }

        if (_exerciseGuide == null)
        {
            _exerciseGuide = GetComponentInChildren<ExerciseTVGuide>(true);
        }

        if (_tvRoot == null && _exerciseGuide != null)
        {
            _tvRoot = _exerciseGuide.gameObject;
        }

        if (_hideTvOnStart && _tvRoot != null)
        {
            _tvRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        SubscribeToGuide();
    }

    private void OnDisable()
    {
        UnsubscribeFromGuide();
    }

    private void SubscribeToGuide()
    {
        if (_exerciseGuide != null && !_subscribed)
        {
            _exerciseGuide.ExerciseCompleted += HandleExerciseCompleted;
            _subscribed = true;
        }
    }

    private void UnsubscribeFromGuide()
    {
        if (_exerciseGuide != null && _subscribed)
        {
            _exerciseGuide.ExerciseCompleted -= HandleExerciseCompleted;
            _subscribed = false;
        }
    }

    public bool HandleActivityCompleted(Activity activityFromTrigger)
    {
        if (_exerciseStarted)
        {
            return true;
        }

        if (activityFromTrigger != null)
        {
            _activity = activityFromTrigger;
        }

        SubscribeToGuide();

        _exerciseStarted = true;
        _rewardApplied = false;

        if (_tvRoot != null)
        {
            _tvRoot.SetActive(true);
        }

        if (_exerciseGuide != null)
        {
            _exerciseGuide.enabled = true;
            _exerciseGuide.BeginExercise();
        }
        else
        {
            Debug.LogWarning("ExerciseActivityStarter has no ExerciseTVGuide assigned.");
            _exerciseStarted = false;
            return false;
        }

        return true;
    }

    private void HandleExerciseCompleted()
    {
        if (!_exerciseStarted || _rewardApplied)
        {
            return;
        }

        _rewardApplied = true;
        //_activity?.DoActivity();
        PlayerManager.Instance.ApplyActivity(_activity.GetActivity());
        _exerciseStarted = false;
    }
}

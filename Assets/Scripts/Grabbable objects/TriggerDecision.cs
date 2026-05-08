using Oculus.Interaction;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class TriggerDecision : MonoBehaviour
{
    [SerializeField] private GrabInteractable _grabInteractable;
    [SerializeField] private Activity _activity;
    [SerializeField] private MonoBehaviour[] _completionHandlers;

    [SerializeField] private Canvas _objectCanvas;
    [SerializeField] private Canvas _hoverCanvas;
    [SerializeField] private Image _progress;
    [SerializeField] private float _fillDuration = 2f;

    private Coroutine _fillCoroutine;
    private MonoBehaviour[] _resolvedCompletionHandlers;
    private bool _completionHandled;

    private bool _done = false;
    public bool Done { get { return _done; } }

    private void Start()
    {
        _grabInteractable.WhenStateChanged += ObjectInteraction;

        _objectCanvas?.gameObject.SetActive(false);
        _hoverCanvas?.gameObject.SetActive(false);

        _done = false;
    }

    private void OnDestroy()
    {
        if (_grabInteractable != null)
            _grabInteractable.WhenStateChanged -= ObjectInteraction;
    }

    private void ObjectInteraction(InteractableStateChangeArgs args)
    {
        if (IsAnyCompletionHandlerBusy())
        {
            StopCharge();
            _objectCanvas?.gameObject.SetActive(false);
            _hoverCanvas?.gameObject.SetActive(false);
            return;
        }

        if (args.NewState == InteractableState.Select)
        {
            _objectCanvas?.gameObject.SetActive(true);
            _hoverCanvas?.gameObject.SetActive(false);
            _completionHandled = false;

            // Start filling
            if (_progress != null)
            {
                _progress.fillAmount = 0f;

                if (_fillCoroutine != null)
                    StopCoroutine(_fillCoroutine);

                _fillCoroutine = StartCoroutine(FillProgress());
                _done = false;
            }
        }
        else if (args.NewState == InteractableState.Hover)
        {
            _hoverCanvas?.gameObject.SetActive(true);
            _objectCanvas?.gameObject.SetActive(false);
        }
        else
        {
            _objectCanvas?.gameObject.SetActive(false);
            _hoverCanvas?.gameObject.SetActive(false);

            StopCharge();
        }
    }

    private IEnumerator FillProgress()
    {
        if (_fillDuration <= 0f)
        {
            _progress.fillAmount = 1f;
            FinishedGrabbing();
            yield break;
        }
        float elapsed = 0f;
        while (elapsed < _fillDuration)
        {

            elapsed += Time.deltaTime;
            _progress.fillAmount = Mathf.Clamp01(elapsed / _fillDuration);
            yield return null;
        }
        FinishedGrabbing();
        _progress.fillAmount = 1f;
        _fillCoroutine = null;
    }

    private void FinishedGrabbing()
    {
        if (_completionHandled)
        {
            return;
        }

        _completionHandled = true;
        _fillCoroutine = null;
        _done = true;
        _objectCanvas?.gameObject.SetActive(false);
        _hoverCanvas?.gameObject.SetActive(false);

        if (TryHandleActivityCompleted())
        {
            return;
        }

        _activity?.DoActivity();
    }

    private bool TryHandleActivityCompleted()
    {
        MonoBehaviour[] handlers = GetCompletionHandlers();
        if (handlers == null)
        {
            return false;
        }

        foreach (MonoBehaviour handlerBehaviour in handlers)
        {
            if (handlerBehaviour is IActivityCompletionHandler handler &&
                handler.HandleActivityCompleted(_activity))
            {
                return true;
            }
        }

        return false;
    }

    private bool IsAnyCompletionHandlerBusy()
    {
        MonoBehaviour[] handlers = GetCompletionHandlers();
        if (handlers == null)
        {
            return false;
        }

        foreach (MonoBehaviour handlerBehaviour in handlers)
        {
            if (handlerBehaviour is IActivityCompletionHandler handler && handler.IsHandlingActivity)
            {
                return true;
            }
        }

        return false;
    }

    private MonoBehaviour[] GetCompletionHandlers()
    {
        if (_completionHandlers != null && _completionHandlers.Length > 0)
        {
            return _completionHandlers;
        }

        if (_resolvedCompletionHandlers == null)
        {
            _resolvedCompletionHandlers = GetComponents<MonoBehaviour>();
        }

        return _resolvedCompletionHandlers;
    }

    private void StopCharge()
    {
        if (_fillCoroutine != null)
        {
            StopCoroutine(_fillCoroutine);
            _fillCoroutine = null;
        }

        if (_progress != null)
        {
            _progress.fillAmount = 0f;
        }
    }
}

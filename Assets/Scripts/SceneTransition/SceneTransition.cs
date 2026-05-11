using DefaultNamespace;
using System.Collections;
using UnityEngine;

public class SceneTransition : MonoBehaviour
{
    public void TransitionToScene(string sceneName)
    {
        StartCoroutine(RunTransitionEffect(sceneName));
    }

    public void TransitionToActivity(ActionData_SO activity)
    {
        PlayerManager.Instance.CurrentActivity = activity;
        //StartCoroutine(RunTransitionEffect(activity.ActivityName));
        LoadScene(activity.ActivityName);
    }

    public void TransitionFromActivity(string sceneName)
    {
        PlayerManager.Instance.ApplyActivity(PlayerManager.Instance.CurrentActivity);
        PlayerManager.Instance.CurrentActivity = null;
        StartCoroutine(RunTransitionEffect(sceneName));
    }

    IEnumerator RunTransitionEffect(string sceneName)
    {
        float duration = 1f;
        float elapsedTime = 0f;
        CanvasGroup canvasGroup = new GameObject("FadeCanvas").AddComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Clamp01(elapsedTime / duration);
            yield return null;
        }
        LoadScene(sceneName);
    }

    private void LoadScene(string sceneName) 
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}

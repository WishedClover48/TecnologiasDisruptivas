using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class WelcomeTutorial : MonoBehaviour, ITutorial
{
    [Serializable] private class Internal
    {
        public TypeText welcomeField;
        public TypeText nameField;
        public CanvasGroup group;
    }
    
    public Action onComplete;
    private bool isDone;
    [SerializeField] private float fadeOutDuration = 1f;
    [Space] 
    [SerializeField] private Internal @internal;
    
    public void Activate()
    {
        gameObject.SetActive(true);
        @internal.welcomeField.Activate(onComplete: () =>
        { @internal.nameField.Activate(onComplete: () => isDone = true);
        });
    }
    public void Deactivate()
    {
        onComplete?.Invoke();
        onComplete = null;
        gameObject.SetActive(false);
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        StartCoroutine(FadeOutRoutine());
    }
    
    private IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;

        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            @internal.group.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
            yield return null;
        }

        @internal.group.alpha = 0f;
        Deactivate();
    }
}

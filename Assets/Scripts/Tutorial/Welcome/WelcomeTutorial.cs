using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class WelcomeTutorial : MonoBehaviour
{
    [Serializable] private class Internal
    {
        public TypeText welcomeField;
        public TypeText nameField;
        public CanvasGroup group;
    }
    
    private bool isDone;
    [SerializeField] private float fadeOutDuration = 1f;
    [Space] 
    [SerializeField] private Internal @internal;
    
    private void Start()
    {
        @internal.welcomeField.Activate(onComplete: () =>
        { @internal.nameField.Activate(onComplete: () => isDone = true);
        });
    }

    public void FadeOut()
    {
        StartCoroutine(FadeOutRoutine());
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
        gameObject.SetActive(false);
    }
}

using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class TypeText : MonoBehaviour
{
    [SerializeField] private float charsPerSecond;
    
    private TMP_Text textComponent;
    private string text;

    private void Awake()
    {
        textComponent = GetComponent<TMP_Text>();
        text = textComponent.text;
        textComponent.text = null; //empty it
    }

    public void Activate(Action onComplete = null, float speed = 0)
    {
        if (speed != 0) charsPerSecond = speed;
        StartCoroutine(TypeTextRoutine(onComplete));
    }

    private IEnumerator TypeTextRoutine(Action onComplete)
    {
        textComponent.text = string.Empty;
        float interval = 1f / charsPerSecond;

        foreach (char c in text)
        {
            textComponent.text += c;
            yield return new WaitForSeconds(interval);
        }

        onComplete?.Invoke();
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ArrowEffect : MonoBehaviour
{
    [SerializeField] private Image img;
    [SerializeField] private float duration;
    [SerializeField] private float waitBetweenCycles = 0.5f;

    private void Awake()
    {
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Vertical;
        img.fillOrigin = (int)Image.OriginVertical.Bottom;
        img.fillAmount = 1f;
    }

    private void OnEnable()
    {
        StartCoroutine(Loop());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    private IEnumerator Loop()
    {
        while (true)
        {
            img.fillOrigin = (int)Image.OriginVertical.Bottom;
            yield return StartCoroutine(AnimateFill(1f, 0f));

            img.fillOrigin = (int)Image.OriginVertical.Top;
            yield return StartCoroutine(AnimateFill(0f, 1f));

            yield return new WaitForSeconds(waitBetweenCycles);
        }
    }

    private IEnumerator AnimateFill(float from, float to)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            img.fillAmount = Mathf.Lerp(from, to, elapsed / duration);
            yield return null;
        }
        img.fillAmount = to;
    }
}

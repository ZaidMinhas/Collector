using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class FadeElement : MonoBehaviour
{
    [Min(0f)] public float fadeInDuration = 1f;
    [Min(0f)] public float stayDuration = 2f;
    [Min(0f)] public float fadeOutDuration = 1f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }

    private IEnumerator Start()
    {
        yield return FadeTo(1f, fadeInDuration);

        yield return new WaitForSecondsRealtime(
            Mathf.Max(0f, stayDuration));

        yield return FadeTo(0f, fadeOutDuration);
    }

    private IEnumerator FadeTo(float targetAlpha, float duration)
    {
        float startingAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(
                startingAlpha,
                targetAlpha,
                Mathf.Clamp01(elapsed / duration));

            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }
}
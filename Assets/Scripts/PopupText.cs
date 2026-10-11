using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PopupText : MonoBehaviour
{
    public bool isPlayOnLoad = true;

    [SerializeField] private Image blackOverlay;
    [SerializeField, Min(1)] private int blinkCount = 2;
    [SerializeField, Min(0.01f)] private float blinkInterval = 0.2f;
    [SerializeField, Min(0f)] private float fadeDuration = 0.6f;

    private CanvasGroup instructions;
    private Coroutine resumeRoutine;
    private float previousTimeScale = 1f;
    private bool ownsPause;

    private void Awake()
    {
        instructions = GetComponent<CanvasGroup>();
    }

    public void Start()
    {
        if (isPlayOnLoad)
        {
            previousTimeScale = Time.timeScale;
            ownsPause = true;
            Time.timeScale = 0;
        }
    }


    public void Resume()
    {
        if (resumeRoutine != null) return;
        instructions.interactable = false;
        resumeRoutine = StartCoroutine(BlinkAndReveal());
    }

    private IEnumerator BlinkAndReveal()
    {
        // Use real time because gameplay remains paused until the reveal finishes.
        for (int i = 0; i < Mathf.Max(1, blinkCount); i++)
        {
            instructions.alpha = 0f;
            yield return WaitForIntroSeconds(Mathf.Max(0.01f, blinkInterval));
            instructions.alpha = 1f;
            yield return WaitForIntroSeconds(Mathf.Max(0.01f, blinkInterval));
        }
        instructions.alpha = 0f;

        var circle = FindFirstObjectByType<CircleTransition>();
        while (circle != null && circle.IsTransitioning)
            yield return null;

        if (blackOverlay != null)
        {
            Color startingColor = blackOverlay.color;
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                if (Manager.PauseMenu.IsPaused)
                {
                    yield return null;
                    continue;
                }
                elapsed += Time.unscaledDeltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                t = t * t * (3f - 2f * t);
                blackOverlay.color = new Color(startingColor.r, startingColor.g,
                    startingColor.b, startingColor.a * (1f - t));
                yield return null;
            }
            blackOverlay.color = Color.clear;
            blackOverlay.gameObject.SetActive(false);
        }

        while (Manager.PauseMenu.IsPaused) yield return null;
        instructions.blocksRaycasts = false;
        RestoreTimeScale();
        resumeRoutine = null;
        gameObject.SetActive(false);
    }

    private void RestoreTimeScale()
    {
        if (!ownsPause) return;
        Manager.PauseMenu.SetGameplayTimeScale(previousTimeScale);
        ownsPause = false;
    }

    private void OnDisable()
    {
        if (resumeRoutine != null)
            StopCoroutine(resumeRoutine);
        resumeRoutine = null;
        RestoreTimeScale();
    }

    private IEnumerator WaitForIntroSeconds(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (!Manager.PauseMenu.IsPaused) elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CircleTransition : MonoBehaviour
{
    int currentLevel = 1;

    [Header("References")]
    [SerializeField] private Image overlay;

    [Header("Settings")]
    [SerializeField] private float transitionDuration = 2f;
    [SerializeField] private Vector2 center = new Vector2(0.5f, 0.5f);

    private Material transitionMaterial;
    private bool isTransitioning;
    public bool IsTransitioning => isTransitioning;

    private static readonly int RadiusID = Shader.PropertyToID("_Radius");
    private static readonly int CenterID = Shader.PropertyToID("_Center");
    private static readonly int AspectID = Shader.PropertyToID("_Aspect");

    private float maxRadius;

    private void Awake()
    {
        // Keep the transition Canvas between scenes.
        if (FindObjectsByType<CircleTransition>(
            FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        transitionMaterial = new Material(overlay.material);
        overlay.material = transitionMaterial;

        RectTransform rect = overlay.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        overlay.raycastTarget = false;

        float aspect = (float)Screen.width / Screen.height;

        transitionMaterial.SetFloat(AspectID, aspect);
        transitionMaterial.SetVector(
            CenterID, new Vector4(center.x, center.y, 0, 0));

        maxRadius = Mathf.Sqrt(
            Mathf.Pow(aspect * 0.5f, 2) + 0.25f
        ) + 0.05f;

        // Start fully black, then reveal the first level.
        SetRadius(0f);
        StartCoroutine(RevealInitialScene());
    }

    private IEnumerator RevealInitialScene()
    {
        isTransitioning = true;
        yield return AnimateRadius(0f, maxRadius, null);
        isTransitioning = false;
    }

    public void GoToNextLevel(string levelName)
    {
        if (isTransitioning)
            return;

        StartCoroutine(TransitionToLevel(levelName));
    }

    private IEnumerator TransitionToLevel(string levelName)
    {
        isTransitioning = true;

        // Close the circle until the screen is black.
        yield return AnimateRadius(maxRadius, 0f, null);

        // Load the new scene while the screen is black.
        SceneManager.LoadScene(levelName);

        // Reveal the new scene.
        yield return AnimateRadius(0f, maxRadius, null);

        isTransitioning = false;
    }

    private IEnumerator AnimateRadius(
        float from, float to, System.Action onComplete)
    {
        float elapsed = 0f;

        while (elapsed < transitionDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float t = Mathf.Clamp01(elapsed / transitionDuration);

            // Smooth easing.
            t = t * t * (3f - 2f * t);

            SetRadius(Mathf.Lerp(from, to, t));

            yield return null;
        }

        SetRadius(to);
        onComplete?.Invoke();
    }

    private void SetRadius(float radius)
    {
        transitionMaterial.SetFloat(RadiusID, radius);
    }

    private void OnDestroy()
    {
        if (transitionMaterial != null)
            Destroy(transitionMaterial);
    }

    public void Reload()
    {
        GoToNextLevel("Level" + currentLevel);
    }

    public void Progress()
    {
        transitionDuration *= 2f;
        currentLevel++;
        GoToNextLevel("Level" + currentLevel);
        transitionDuration /= 2f;
    }
}

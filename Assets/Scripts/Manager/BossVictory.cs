using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    [RequireComponent(typeof(HealthComponent))]
    public class BossVictory : MonoBehaviour
    {
        private const string CreditsScene = "CreditMenu";
        private HealthComponent health;
        private bool victoryHandled;

        private void Awake()
        {
            health = GetComponent<HealthComponent>();
            health.OnDeath += OnBossDefeated;
        }

        private void OnBossDefeated()
        {
            if (victoryHandled || gameObject.scene.name != "Level5") return;
            victoryHandled = true;

            PlayerVictoryAudio.Play();
            KidnapAudio.Stop();
            // Prevent further combat during the existing unscaled circle transition.
            Time.timeScale = 0f;
            SceneManager.sceneLoaded += ResumeAfterCreditsLoad;

            var transition = FindFirstObjectByType<CircleTransition>();
            if (transition != null)
                // Host this on the persistent transition: the boss is destroyed on death.
                transition.StartCoroutine(OpenCreditsWhenReady(transition));
            else
                SceneManager.LoadSceneAsync(CreditsScene);
        }

        private static IEnumerator OpenCreditsWhenReady(CircleTransition transition)
        {
            while (transition != null && transition.IsTransitioning)
                yield return null;

            if (transition != null)
                transition.GoToNextLevel(CreditsScene);
            else
                SceneManager.LoadSceneAsync(CreditsScene);
        }

        private static void ResumeAfterCreditsLoad(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != CreditsScene) return;
            SceneManager.sceneLoaded -= ResumeAfterCreditsLoad;
            Time.timeScale = 1f;
        }

        private void OnDestroy()
        {
            if (health != null) health.OnDeath -= OnBossDefeated;
        }
    }
}

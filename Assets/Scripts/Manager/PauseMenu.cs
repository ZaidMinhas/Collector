using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{
    public class PauseMenu : MonoBehaviour
    {
        private const string OptionsScene = "OptionSetting";
        private static PauseMenu instance;
        public static bool IsPaused => instance != null && instance.paused;

        private bool paused;
        private bool busy;
        private bool closeRequested;
        private float previousTimeScale;
        private Scene gameplayScene;
        private readonly List<GraphicRaycaster> blockedRaycasters = new List<GraphicRaycaster>();
        private readonly List<EventSystem> blockedEventSystems = new List<EventSystem>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Create()
        {
            if (instance == null)
                new GameObject("Pause Menu").AddComponent<PauseMenu>();
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        private void Update()
        {
            if (Keyboard.current == null || !Keyboard.current.escapeKey.wasPressedThisFrame || busy)
                return;
            if (paused) Resume();
            else if (SceneManager.GetActiveScene().name != OptionsScene)
                StartCoroutine(Open());
        }

        private IEnumerator Open()
        {
            // The original options scene still works normally when opened from MainMenu.
            if (SceneManager.GetSceneByName(OptionsScene).isLoaded) yield break;
            if (!Application.CanStreamedLevelBeLoaded(OptionsScene))
            {
                Debug.LogWarning("Add OptionSetting to the build scene list to use the pause menu.");
                yield break;
            }

            busy = true;
            paused = true;
            previousTimeScale = Time.timeScale;
            gameplayScene = SceneManager.GetActiveScene();
            Time.timeScale = 0f;

            foreach (var raycaster in FindObjectsByType<GraphicRaycaster>(FindObjectsSortMode.None))
            {
                if (!raycaster.enabled) continue;
                blockedRaycasters.Add(raycaster);
                raycaster.enabled = false;
            }
            foreach (var events in FindObjectsByType<EventSystem>(FindObjectsSortMode.None))
            {
                if (!events.enabled) continue;
                blockedEventSystems.Add(events);
                events.enabled = false;
            }

            yield return SceneManager.LoadSceneAsync(OptionsScene, LoadSceneMode.Additive);
            busy = false;
            if (closeRequested) Resume();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!paused || scene.name != OptionsScene || mode != LoadSceneMode.Additive) return;
            foreach (var root in scene.GetRootGameObjects())
            {
                // Keep the game camera and listener; the options scene contributes only UI.
                foreach (var camera in root.GetComponentsInChildren<Camera>(true)) camera.enabled = false;
                foreach (var listener in root.GetComponentsInChildren<AudioListener>(true)) listener.enabled = false;
                foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
                {
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.overrideSorting = true;
                    canvas.sortingOrder = 1000;
                }
                foreach (var text in root.GetComponentsInChildren<TMP_Text>(true))
                    if (text.gameObject.name == "SettingText") text.text = "PAUSED";
                foreach (var button in root.GetComponentsInChildren<Button>(true))
                {
                    bool isBackButton = false;
                    for (int i = 0; i < button.onClick.GetPersistentEventCount(); i++)
                        if (button.onClick.GetPersistentMethodName(i) == "LoadGameScene") isBackButton = true;
                    if (!isBackButton) continue;
                    button.onClick = new Button.ButtonClickedEvent();
                    button.onClick.AddListener(Resume);
                    var label = button.GetComponentInChildren<TMP_Text>(true);
                    if (label != null) label.text = "RESUME";
                }
            }
        }

        public void Resume()
        {
            if (!paused || busy) return;
            StartCoroutine(Close());
        }

        private IEnumerator Close()
        {
            busy = true;
            var options = SceneManager.GetSceneByName(OptionsScene);
            if (options.isLoaded) yield return SceneManager.UnloadSceneAsync(options);
            Restore();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            if (!paused || scene != gameplayScene) return;
            closeRequested = true;
            if (!busy) Resume();
        }

        private void Restore()
        {
            foreach (var raycaster in blockedRaycasters)
                if (raycaster != null) raycaster.enabled = true;
            foreach (var events in blockedEventSystems)
                if (events != null) events.enabled = true;
            blockedRaycasters.Clear();
            blockedEventSystems.Clear();
            Time.timeScale = gameplayScene.IsValid() && gameplayScene.isLoaded ? previousTimeScale : 1f;
            paused = false;
            busy = false;
            closeRequested = false;
        }

        public static void SetGameplayTimeScale(float value)
        {
            if (IsPaused) instance.previousTimeScale = value;
            else Time.timeScale = value;
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
            if (paused) Restore();
            instance = null;
        }
    }
}

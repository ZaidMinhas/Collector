using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Manager
{
    public class UIAudio : MonoBehaviour
    {
        private static UIAudio instance;
        private AudioSource source;
        private AudioClip cool;
        private AudioClip uncool;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Create()
        {
            if (instance == null) new GameObject("UI Audio").AddComponent<UIAudio>();
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
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode) => RegisterButtons();

        // Also cover buttons spawned at runtime, including currently inactive panels.
        private void LateUpdate() => RegisterButtons();

        private static void RegisterButtons()
        {
            foreach (var button in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
                Attach(button);
        }

        public static UIButtonAudio Attach(Button button)
        {
            var feedback = button.GetComponent<UIButtonAudio>();
            return feedback != null ? feedback : button.gameObject.AddComponent<UIButtonAudio>();
        }

        public static void PlayCool() => Play(false);
        public static void PlayUncool() => Play(true);

        private static void Play(bool backwards)
        {
            if (instance == null) Create();
            if (instance.cool == null)
                instance.cool = Resources.Load<AudioClip>("Sfx/cool button press");
            if (instance.uncool == null)
                instance.uncool = Resources.Load<AudioClip>("Sfx/uncool button press");
            var clip = backwards ? instance.uncool : instance.cool;
            if (clip == null) return;

            var sfx = AudioManager.Instance != null ? AudioManager.Instance.SfxSource : null;
            instance.source.outputAudioMixerGroup = sfx != null ? sfx.outputAudioMixerGroup : null;
            instance.source.mute = sfx != null && sfx.mute;
            instance.source.volume = sfx != null ? sfx.volume : 1f;
            // Directly opened menu scenes may not have an AudioManager yet.
            if (instance.source.outputAudioMixerGroup == null)
                instance.source.volume *= Mathf.Clamp01(PlayerPrefs.GetFloat("MasterVol", 1f)) *
                    Mathf.Clamp01(PlayerPrefs.GetFloat("SfxVol", 1f));
            instance.source.PlayOneShot(clip, 0.5f);
        }

        private void OnDestroy()
        {
            if (instance != this) return;
            SceneManager.sceneLoaded -= OnSceneLoaded;
            instance = null;
        }
    }
}

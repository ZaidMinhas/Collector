using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager
{
    [AddComponentMenu("Audio/Scene Audio Settings")]
    public class SceneAudioSettings : MonoBehaviour
    {
        [Range(0f, 4f), Tooltip("Scales all SFX in this scene: 0 = silent, 1 = normal, 2 = double amplitude. Music is unchanged.")]
        public float sfxVolumeMultiplier = 1f;

        private static readonly List<SceneAudioSettings> settings = new List<SceneAudioSettings>();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => settings.Clear();

        public static float CurrentSfxMultiplier
        {
            get
            {
                var scene = SceneManager.GetActiveScene();
                foreach (var entry in settings)
                    if (entry != null && entry.isActiveAndEnabled && entry.gameObject.scene == scene)
                        return Mathf.Clamp(entry.sfxVolumeMultiplier, 0f, 4f);
                return 1f;
            }
        }

        private void OnEnable()
        {
            if (!settings.Contains(this)) settings.Add(this);
        }

        private void OnDisable() => settings.Remove(this);
    }
}

using UnityEngine;

namespace Manager
{
    // Shared default for both scene-placed and prefab-spawned shooters.
    public static class DefenderShotAudio
    {
        private static AudioClip defaultClip;

        public static void Play()
        {
            if (AudioManager.Instance == null) return;
            if (defaultClip == null)
                defaultClip = Resources.Load<AudioClip>("Sfx/Pew");
            if (defaultClip == null) return;

            // +2 dB while keeping the existing shot volume and pitch variation.
            AudioManager.Instance.PlaySFX(defaultClip,
                Random.Range(0.25f, 0.35f) * 1.2589254f, Random.Range(0.9f, 1.1f));
        }
    }
}

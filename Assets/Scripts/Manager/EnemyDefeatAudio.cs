using UnityEngine;

namespace Manager
{
    public static class EnemyDefeatAudio
    {
        private static AudioClip clip;

        public static void Play()
        {
            if (AudioManager.Instance == null) return;
            if (clip == null)
                clip = Resources.Load<AudioClip>("Sfx/Defeat");
            if (clip == null) return;

            // +6 dB: multiply the previous amplitude by 10^(6 / 20).
            AudioManager.Instance.PlaySFX(clip, 0.5f * 1.9952623f);
        }
    }
}

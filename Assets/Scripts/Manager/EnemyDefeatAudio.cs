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

            AudioManager.Instance.PlaySFX(clip, 0.5f);
        }
    }
}

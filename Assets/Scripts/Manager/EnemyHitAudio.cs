using UnityEngine;

namespace Manager
{
    public static class EnemyHitAudio
    {
        private static AudioClip clip;

        public static void Play()
        {
            if (AudioManager.Instance == null) return;
            if (clip == null)
                clip = Resources.Load<AudioClip>("Sfx/Hit an Ennemy");
            if (clip == null) return;

            AudioManager.Instance.PlaySFX(clip, 0.5f);
        }
    }
}

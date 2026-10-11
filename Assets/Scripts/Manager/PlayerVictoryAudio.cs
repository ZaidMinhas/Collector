using UnityEngine;

namespace Manager
{
    public static class PlayerVictoryAudio
    {
        private static AudioClip clip;

        public static void Play()
        {
            if (AudioManager.Instance == null) return;
            if (clip == null)
                clip = Resources.Load<AudioClip>("Sfx/Yay You Won!");
            if (clip == null) return;

            // The persistent manager keeps this playing through the next scene load.
            AudioManager.Instance.PlaySFX(clip);
        }
    }
}

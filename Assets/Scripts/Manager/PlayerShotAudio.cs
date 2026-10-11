using UnityEngine;

namespace Manager
{
    public static class PlayerShotAudio
    {
        private static AudioClip clip;

        public static void Play()
        {
            if (AudioManager.Instance == null) return;
            if (clip == null)
                clip = Resources.Load<AudioClip>("Sfx/Pew(HigherPitched)");
            if (clip == null) return;

            // The higher pitch is already baked into this clip.
            AudioManager.Instance.PlayNonEnemySFX(clip, 0.4f);
        }
    }
}

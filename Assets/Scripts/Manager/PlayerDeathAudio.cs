using UnityEngine;

namespace Manager
{
    public static class PlayerDeathAudio
    {
        private static AudioClip clip;

        public static void Play()
        {
            if (AudioManager.Instance == null) return;
            if (clip == null)
                clip = Resources.Load<AudioClip>("Sfx/Oh No You Lost");
            if (clip == null) return;

            // The persistent manager keeps this playing through the scene reload.
            AudioManager.Instance.PlayNonEnemySFX(clip, includeGeneralBoost: false);
        }
    }
}

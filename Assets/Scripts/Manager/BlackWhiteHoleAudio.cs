using UnityEngine;

namespace Manager
{
    public static class BlackWhiteHoleAudio
    {
        private static AudioClip clip;

        public static void Play()
        {
            var manager = AudioManager.Instance;
            if (manager == null) return;
            if (clip == null)
                clip = Resources.Load<AudioClip>("Sfx/Black and White hole");
            if (clip == null) return;

            // 0.35 (-9.12 dB) plus the general +6.30 dB boost = about -2.82 dB.
            manager.PlayNonEnemySFX(clip, 0.35f);
        }
    }
}

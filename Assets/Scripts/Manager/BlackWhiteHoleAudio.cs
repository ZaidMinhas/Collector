using UnityEngine;

namespace Manager
{
    public static class BlackWhiteHoleAudio
    {
        private static AudioClip clip;

        public static void Play(Transform hole)
        {
            var manager = AudioManager.Instance;
            if (hole == null) return;
            if (manager == null || manager.SfxSource == null)
            {
                Debug.LogWarning("Hole audio requires an AudioManager with an assigned SFX source.", hole);
                return;
            }
            if (clip == null)
                clip = Resources.Load<AudioClip>("Sfx/Black and White hole");
            if (clip == null)
            {
                Debug.LogError("Hole audio could not load Resources/Sfx/Black and White hole.ogg.", hole);
                return;
            }

            // The loop belongs to this hole, so destroying the hole stops its sound.
            var soundObject = new GameObject("Black and White Hole Loop");
            soundObject.transform.SetParent(hole, false);
            var source = soundObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = manager.SfxSource.outputAudioMixerGroup;
            // AudioSource volume is capped at 1; the player's SFX mixer also applies.
            source.volume = manager.SfxSource.volume * 10.35f * AudioManager.GeneralSfxGain;
            source.mute = manager.SfxSource.mute;
            source.clip = clip;
            source.Play();
        }
    }
}

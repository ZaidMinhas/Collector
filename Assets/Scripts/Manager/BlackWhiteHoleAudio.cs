using UnityEngine;

namespace Manager
{
    public static class BlackWhiteHoleAudio
    {
        private static AudioClip clip;

        public static void Play(Transform hole)
        {
            var manager = AudioManager.Instance;
            if (manager == null || manager.SfxSource == null || hole == null) return;
            if (clip == null)
                clip = Resources.Load<AudioClip>("Sfx/Black and White hole");
            if (clip == null) return;

            // The loop belongs to this hole, so destroying the hole stops its sound.
            var soundObject = new GameObject("Black and White Hole Loop");
            soundObject.transform.SetParent(hole, false);
            var source = soundObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.loop = true;
            source.spatialBlend = 0f;
            source.outputAudioMixerGroup = manager.SfxSource.outputAudioMixerGroup;
            // Preserve the existing effect level and the player's SFX mixer settings.
            source.volume = manager.SfxSource.volume * 0.35f * AudioManager.GeneralSfxGain;
            source.mute = manager.SfxSource.mute;
            source.clip = clip;
            source.Play();
        }
    }
}

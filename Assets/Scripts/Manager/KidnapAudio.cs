using UnityEngine;

namespace Manager
{
    public static class KidnapAudio
    {
        private static AudioClip clip;
        private static AudioSource source;

        public static void Play(Transform owner)
        {
            var manager = AudioManager.Instance;
            if (manager == null || manager.SfxSource == null || owner == null) return;
            if (clip == null)
                clip = Resources.Load<AudioClip>("Sfx/Kidnap");
            if (clip == null) return;

            if (source != null && source.transform.parent != owner)
            {
                source.Stop();
                Object.Destroy(source.gameObject);
                source = null;
            }

            if (source == null)
            {
                var soundObject = new GameObject("Kidnap Loop");
                soundObject.transform.SetParent(owner, false);
                source = soundObject.AddComponent<AudioSource>();
                source.playOnAwake = false;
                source.loop = true;
                source.spatialBlend = 0f;
            }

            source.outputAudioMixerGroup = manager.SfxSource.outputAudioMixerGroup;
            source.volume = manager.SfxSource.volume * 0.5f;
            source.mute = manager.SfxSource.mute;
            source.clip = clip;
            if (!source.isPlaying) source.Play();
        }

        public static void Stop()
        {
            if (source != null) source.Stop();
        }
    }
}

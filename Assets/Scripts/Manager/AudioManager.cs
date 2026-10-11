using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace Manager
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;
        
        [Header("Audio Mixer Reference")]
        [Tooltip("Glissez votre Audio Mixer principal ici")]
        public AudioMixer audioMixer;

        [Header("Global Audio Sources")]
        [Tooltip("Source audio pour la musique de fond")]
        public AudioSource MusicSource;
        [Tooltip("Source audio par défaut pour les effets sonores")]
        public AudioSource SfxSource;

        private readonly System.Collections.Generic.List<AudioSource> shotVoices =
            new System.Collections.Generic.List<AudioSource>();

        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            LoadVolumeSettings();
        }
        
        
        // Charge les volumes stockés dans le PlayerPrefs et les applique
        public void LoadVolumeSettings()
        {
            float master = PlayerPrefs.GetFloat("MasterVol", 1f);
            float music = PlayerPrefs.GetFloat("MusicVol", 1f);
            float sfx = PlayerPrefs.GetFloat("SfxVol", 1f);

            SetGlobalVolume(master);
            SetMusicVolume(music);
            SetSfxVolume(sfx);
        }
        
        public void SetGlobalVolume(float sliderValue)
        {
            float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
            if (audioMixer != null) 
            {
                audioMixer.SetFloat("MasterVolume", db);
            }

            PlayerPrefs.SetFloat("MasterVol", sliderValue);
            PlayerPrefs.Save();
        }

        // Applique et sauvegarde le volume de la Musique
        public void SetMusicVolume(float sliderValue)
{
    float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
    Debug.Log("SetMusicVolume appelé avec la valeur slider : " + sliderValue + " (soit " + db + " dB)");
    
    if (audioMixer != null) 
    {
        bool success = audioMixer.SetFloat("MusicVolume", db);
        Debug.Log("Paramètre MusicVolume appliqué au mixer avec succès : " + success);
    }
    else
    {
        Debug.LogWarning("AudioMixer est null dans l'AudioManager !");
    }

    PlayerPrefs.SetFloat("MusicVol", sliderValue);
    PlayerPrefs.Save();
}

        // Applique et sauvegarde le volume des Effets Sonores (SFX)
        public void SetSfxVolume(float sliderValue)
        {
            float db = Mathf.Log10(Mathf.Clamp(sliderValue, 0.0001f, 1f)) * 20f;
            if (audioMixer != null) 
            {
                audioMixer.SetFloat("SfxVolume", db);
            }

            PlayerPrefs.SetFloat("SfxVol", sliderValue);
            PlayerPrefs.Save();
        }
        
        // Fonction pour jouer un effet sonore (SFX) facilement depuis n'importe quel script
        public void PlaySFX(AudioClip clip)
        {
            PlaySFX(clip, 1f);
        }

        public void PlaySFX(AudioClip clip, float volumeScale)
        {
            if (SfxSource != null && clip != null)
            {
                SfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
            }
        }

        public void PlaySFX(AudioClip clip, float volumeScale, float pitch)
        {
            if (SfxSource == null || clip == null) return;

            AudioSource voice = shotVoices.Find(source => source != null && !source.isPlaying);
            if (voice == null)
            {
                // Bound simultaneous effects so large enemy waves cannot allocate endlessly.
                if (shotVoices.Count >= 24) return;
                var voiceObject = new GameObject("SFX Voice");
                voiceObject.transform.SetParent(transform, false);
                voice = voiceObject.AddComponent<AudioSource>();
                voice.playOnAwake = false;
                voice.spatialBlend = 0f;
                shotVoices.Add(voice);
            }

            // Each shot has its own pitch; overlapping shots keep their original pitch.
            voice.outputAudioMixerGroup = SfxSource.outputAudioMixerGroup;
            voice.mute = SfxSource.mute;
            voice.volume = SfxSource.volume * Mathf.Clamp01(volumeScale);
            voice.pitch = Mathf.Clamp(pitch, 0.1f, 3f);
            voice.clip = clip;
            voice.Play();
        }

        public void PlayMusic(AudioClip clip, bool loop = true)
        {
            if (MusicSource != null && clip != null)
            {
                // Si c'est déjà exactement la même musique et qu'elle joue, on peut l'ignorer ou la redémarrer
                // Mais si c'est une nouvelle musique, on l'assigne et on la lance !
                if (MusicSource.clip != clip)
                {
                    MusicSource.clip = clip;
                }

                MusicSource.loop = loop;
        
                if (!MusicSource.isPlaying)
                {
                    MusicSource.Play();
                }
            }
        }

        public void StopMusic()
        {
            if (MusicSource != null && MusicSource.isPlaying)
            {
                MusicSource.Stop();
            }
        }
    }
}

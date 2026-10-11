using UnityEngine;
using UnityEngine.UI;

namespace Manager
{
    [RequireComponent(typeof(Slider))]
    public class VolumeSliderConnector : MonoBehaviour
    {
        public enum AudioType { Master, Music, Sfx }
        [Header("Type de Volume")]
        public AudioType audioType;

        private Slider slider;
        
        
        void Start()
        {
            slider = GetComponent<Slider>();

            // 1. Initialiser la position du curseur du slider selon la sauvegarde actuelle
            InitializeSliderValue();

            // 2. Écouter les mouvements du slider faits par le joueur
            slider.onValueChanged.AddListener(OnSliderValueChanged);
        }

        void InitializeSliderValue()
        {
            if (slider == null) return;

            // Récupère la valeur dans PlayerPrefs (1.0f par défaut)
            if (audioType == AudioType.Master)
                slider.value = PlayerPrefs.GetFloat("MasterVol", 1f);
            else if (audioType == AudioType.Music)
                slider.value = PlayerPrefs.GetFloat("MusicVol", 1f);
            else if (audioType == AudioType.Sfx)
                slider.value = PlayerPrefs.GetFloat("SfxVol", 1f);
        }

        void OnSliderValueChanged(float value)
        {
            if (AudioManager.Instance == null) return;

            // Transmet la modification à l'AudioManager permanent
            if (audioType == AudioType.Master)
                AudioManager.Instance.SetGlobalVolume(value);
            else if (audioType == AudioType.Music)
                AudioManager.Instance.SetMusicVolume(value);
            else if (audioType == AudioType.Sfx)
                AudioManager.Instance.SetSfxVolume(value);
        }
    }
}
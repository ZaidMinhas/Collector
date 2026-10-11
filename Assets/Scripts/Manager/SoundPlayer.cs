using UnityEngine;

namespace Manager
{
    [AddComponentMenu("Audio/Sound Player")]
    public class SoundPlayer : MonoBehaviour
    {
        [SerializeField, Tooltip("Sound effect to play through the shared SFX mixer group.")]
        private AudioClip clip;

        [SerializeField, Range(0f, 1f), Tooltip("Volume of this effect before the SFX and Master sliders are applied.")]
        private float volume = 1f;

        // Parameterless so buttons and animation events can call it directly.
        public void Play()
        {
            if (clip == null)
            {
                Debug.LogWarning("Assign a clip to SoundPlayer before playing it.", this);
                return;
            }

            var manager = AudioManager.Instance;
            if (manager == null || manager.SfxSource == null)
            {
                Debug.LogWarning("SoundPlayer requires an AudioManager with an assigned SFX source.", this);
                return;
            }

            manager.PlaySFX(clip, volume);
        }
    }
}

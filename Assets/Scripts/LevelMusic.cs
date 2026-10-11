using UnityEngine;
using Manager;

public class LevelMusic : MonoBehaviour
{
    public AudioClip music;
    public bool loop = true;
    [Range(0f, 1f), Tooltip("Track volume before the player's Music and Master sliders are applied.")]
    public float volume = 1f;

    private void Start()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("No AudioManager exists in the scene.", this);
            return;
        }

        if (music == null)
            return;

        AudioManager.Instance.StopMusic();
        AudioManager.Instance.PlayMusic(music, loop, volume);
    }

    private void Update()
    {
        // Allow balancing in the Inspector during playback without restarting the track.
        var manager = AudioManager.Instance;
        if (music != null && manager != null && manager.MusicSource != null &&
            manager.MusicSource.clip == music)
            manager.MusicSource.volume = Mathf.Clamp01(volume);
    }
}

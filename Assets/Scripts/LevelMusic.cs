using UnityEngine;
using Manager;

public class LevelMusic : MonoBehaviour
{
    public AudioClip music;
    public bool loop = true;

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
        AudioManager.Instance.PlayMusic(music, loop);
    }
}
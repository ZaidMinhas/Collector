using UnityEngine;

public class MortalEntity : MonoBehaviour
{
    public float lifetime; // in seconds

    public float lifeframes = 0f;

    public float timerState = 0f;
    private bool timerOn = false;

    private void Start()
    {
    }

    private void FixedUpdate()
    {
        if (timerOn)
        {
            timerState++;
            if (timerState >= lifeframes)
            {
                timerOn = false;
                StopAttachedAudio();
                Destroy(gameObject);
                UniverseManager.Universe().RefreshLists();
            }
            else if (timerState / lifeframes > 0.75f)
            {
                Vector3 s = gameObject.transform.localScale;
                float mult = 4f * (1f - (timerState / lifeframes));
                gameObject.transform.localScale = new(s.x * mult, s.y * mult, s.z * mult);
            }
        }
    }

    private void OnDisable()
    {
        StopAttachedAudio();
    }

    private void StopAttachedAudio()
    {
        // Cut lifetime-bound sounds immediately, before deferred destruction.
        foreach (var source in GetComponentsInChildren<AudioSource>(true))
            source.Stop();
    }

    public void StartTimer()
    {
        lifeframes = lifetime * 50f;
        timerOn = true;
    }
}

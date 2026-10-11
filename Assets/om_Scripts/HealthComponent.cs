using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{

    public float defaultMaxHealth;
    public float currentHealth;
    public bool isDestroyedOnDeath = false;
    public bool isDebugMode;

    private float nextHitSoundTime;

    public Action OnTakeDamage;
    public Action<float> OnHealthChange;
    public Action OnDeath;

    public void Awake()
    {
        currentHealth = defaultMaxHealth;
    }

    public void setHealth(float health)
    {
        this.currentHealth = defaultMaxHealth =  health;
    }
    
    

    public void Start()
    {
        if (isDestroyedOnDeath)
        {
            OnDeath += () =>
            {
                if (gameObject.GetComponent<DestroyableEnemy>() != null) gameObject.GetComponent<DestroyableEnemy>().Kill();
                else Destroy(gameObject);
            };

        }

        if (isDebugMode)
        {
            OnTakeDamage += () => Debug.Log(name + " OnTakeDamage");
            OnDeath += () => Debug.Log(name + "OnDeath");
        }
    }

    public void GiveHealth(float health)
    {
        TakeDamage(-health);
    }

    public void TakeDamage(float damage)
    {
        if (damage > 0f && currentHealth > 0f &&
            GetComponent<DestroyableEnemy>() != null && Time.unscaledTime >= nextHitSoundTime)
        {
            Manager.EnemyHitAudio.Play();
            // Lasers can apply damage repeatedly in one frame; avoid stacked hit sounds.
            nextHitSoundTime = Time.unscaledTime + 0.08f;
        }

        currentHealth -= damage;
        OnTakeDamage?.Invoke();
        OnHealthChange?.Invoke(currentHealth/defaultMaxHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }
}

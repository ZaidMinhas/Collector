using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{

    public float defaultMaxHealth;
    public float currentHealth;
    public bool isDestroyedOnDeath = false;
    public bool isDebugMode;

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

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        OnTakeDamage?.Invoke();
        OnHealthChange?.Invoke(currentHealth/defaultMaxHealth);

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }
}

using System;
using UnityEngine;

public class HealthComponent : MonoBehaviour
{
    
    public float defaultMaxHealth;
    public float currentHealth;
    public bool isDestroyedOnDeath = false;
    public bool isDebugMode;

    public Action OnTakeDamage;
    public Action OnDeath;

    public void Awake()
    {
        currentHealth = defaultMaxHealth;
    }

    public void setHealth(float health)
    {
        this.currentHealth = health;
    }

    public void Start()
    {
        if (isDestroyedOnDeath)
        {
            OnDeath += () => Destroy(gameObject);
            
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

        if (currentHealth <= 0)
        {
            OnDeath?.Invoke();
        }
    }
}

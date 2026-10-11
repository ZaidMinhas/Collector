using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public List<Sprite> barStages = new List<Sprite>();
    public HealthComponent entityToTrack;
    public Animator animator;
    public Image barImg;
    public SpriteRenderer barRenderer;

    // Keep the field used by existing scene overrides; scenes do not need rewriting.
    [SerializeField] private HealthComponent healthComponent;
    private Slider slider;
    private HealthComponent trackedEntity;

    private void Start()
    {
        slider = GetComponent<Slider>();
        trackedEntity = entityToTrack != null ? entityToTrack : healthComponent;
        if (trackedEntity == null)
        {
            Debug.LogWarning("Assign an entity to the health bar.", this);
            return;
        }

        trackedEntity.OnHealthChange += OnHealthChanged;
        Refresh(trackedEntity.defaultMaxHealth > 0f
            ? trackedEntity.currentHealth / trackedEntity.defaultMaxHealth : 0f);
    }

    private void OnHealthChanged(float health)
    {
        Refresh(health);
        if (animator != null) animator.SetTrigger("TakeHit");
    }

    private void Refresh(float health)
    {
        health = Mathf.Clamp01(health);
        if (slider != null) slider.value = health;
        if (barStages == null || barStages.Count == 0) return;

        int index = Mathf.Clamp(barStages.Count - Mathf.CeilToInt(health * barStages.Count),
            0, barStages.Count - 1);
        if (barImg != null) barImg.sprite = barStages[index];
        if (barRenderer != null) barRenderer.sprite = barStages[index];
    }

    private void OnDestroy()
    {
        if (trackedEntity != null) trackedEntity.OnHealthChange -= OnHealthChanged;
    }
}

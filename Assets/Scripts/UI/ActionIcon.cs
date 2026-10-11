using UnityEngine;

public class ActionIcon : MonoBehaviour
{
    public PlayerAction playerAction;
    public RectTransform cooldownOverlay;

    private void Awake()
    {
        if (playerAction == null || cooldownOverlay == null)
        {
            Debug.LogError("Assign the action and cooldown overlay.", this);
            enabled = false;
            return;
        }

        playerAction.ResetRuntimeState();

        cooldownOverlay.anchorMin = Vector2.zero;
        SetHeight(0f);
    }

    private void OnEnable()
    {
        if (playerAction == null || cooldownOverlay == null)
            return;

        playerAction.OnUsePass += UseAction;
        RefreshOverlay();
    }

    private void OnDisable()
    {
        if (playerAction != null)
            playerAction.OnUsePass -= UseAction;
    }

    public void UseAction()
    {
        RefreshOverlay();
    }

    private void Update()
    {
        if (playerAction.cooldown <= 0)
        {
            return;
        }
        playerAction.TickCooldown();
        RefreshOverlay();
    }

    private void RefreshOverlay()
    {
        if (playerAction.cooldown <= 0)
        {
            return;
        }
        int max = playerAction.maxUses;

        if (max <= 0)
        {
            SetHeight(0f);
            return;
        }

        int missing = Mathf.Clamp(
            max - playerAction.currentUses, 0, max);

        float progress = 0f;

        if (missing > 0 && playerAction.cooldown > 0f)
        {
            progress = 1f - Mathf.Clamp01(
                playerAction.CooldownRemaining / playerAction.cooldown);
        }

        SetHeight(Mathf.Clamp01((missing - progress) / max));
    }

    private void SetHeight(float fraction)
    {
        cooldownOverlay.anchorMax = new Vector2(1f, 1f - fraction);
        cooldownOverlay.offsetMin = Vector2.zero;
        cooldownOverlay.offsetMax = Vector2.zero;
    }
}
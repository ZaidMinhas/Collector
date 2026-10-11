using UnityEngine;
using UnityEngine.UI;

public class ActionIcon : MonoBehaviour
{
    public PlayerAction playerAction;
    public RectTransform cooldownOverlay;

    private Image cooldownImage;

    private void Awake()
    {
        if (playerAction == null || cooldownOverlay == null)
        {
            Debug.LogError("Assign the action and cooldown overlay.", this);
            enabled = false;
            return;
        }

        cooldownImage = cooldownOverlay.GetComponent<Image>();
        if (cooldownImage == null)
        {
            Debug.LogError("The cooldown overlay needs an Image component.", this);
            enabled = false;
            return;
        }

        cooldownImage.type = Image.Type.Filled;
        cooldownImage.fillMethod = Image.FillMethod.Vertical;
        cooldownImage.fillOrigin = (int)Image.OriginVertical.Bottom;

        playerAction.ResetRuntimeState();
        SetFill(0f);
    }

    private void OnEnable()
    {
        if (playerAction == null || cooldownImage == null)
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
        playerAction.TickCooldown();
        RefreshOverlay();
    }

    private void RefreshOverlay()
    {
        if (cooldownImage == null)
            return;

        if (playerAction.cooldown <= 0f || playerAction.maxUses <= 0)
        {
            SetFill(0f);
            return;
        }

        int max = playerAction.maxUses;
        int missing = Mathf.Clamp(max - playerAction.currentUses, 0, max);
        float progress = missing > 0
            ? 1f - Mathf.Clamp01(playerAction.CooldownRemaining / playerAction.cooldown)
            : 0f;

        SetFill(Mathf.Clamp01((missing - progress) / max));
    }

    private void SetFill(float missingFraction)
    {
        // Preserve the previous 1 - fraction display direction.
        cooldownImage.fillAmount = 1f - missingFraction;
    }
}

using System;
using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAction",
    menuName = "Scriptable Objects/PlayerAction")]
public class PlayerAction : ScriptableObject
{
    public string actionName;
    public int maxUses;
    public int currentUses;
    public float cooldown;
    public float lifeTime;
    public KeyCode key;

    public Action OnUseFail;
    public Action OnUsePass;

    [NonSerialized] private float nextUseTime;
    [NonSerialized] private bool rechargePending;

    public float CooldownRemaining => rechargePending
        ? Mathf.Max(0f, nextUseTime - Time.time)
        : 0f;

    public void UseAction()
    {
        TickCooldown();
        if (currentUses <= 0)
        {
            OnUseFail?.Invoke();
            return;
        }

        currentUses--;

        // Spending another charge does not restart an active recharge.
        if (!rechargePending)
        {
            nextUseTime = Time.time + Mathf.Max(0f, cooldown);
            rechargePending = true;
        }

        OnUsePass?.Invoke();
    }

    public void TickCooldown()
    {
        if (!rechargePending)
            return;

        if (currentUses >= maxUses)
        {
            rechargePending = false;
            return;
        }

        if (cooldown <= 0f)
        {
            currentUses = maxUses;
            rechargePending = false;
            return;
        }

        // Restore one charge per elapsed cooldown, until full.
        while (rechargePending && Time.time >= nextUseTime)
        {
            currentUses = Mathf.Min(currentUses + 1, maxUses);
            if (currentUses >= maxUses)
                rechargePending = false;
            else
                nextUseTime += cooldown;
        }
    }

    public void ResetRuntimeState()
    {
        currentUses = Mathf.Max(0, maxUses);
        nextUseTime = 0f;
        rechargePending = false;
    }
}

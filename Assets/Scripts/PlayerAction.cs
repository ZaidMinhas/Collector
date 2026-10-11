using UnityEngine;

[CreateAssetMenu(fileName = "PlayerAction", menuName = "Scriptable Objects/PlayerAction")]
public class PlayerAction : ScriptableObject
{
    public string actionName;
    public int maxUses;
    public int currentUses;
    public float cooldown;
    public float lifeTime;
    public KeyCode key;
}
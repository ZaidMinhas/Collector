using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "TurretConfig", menuName = "Scriptable Objects/TurretConfig")]
public class TurretConfig : ScriptableObject
{
    [Header("Turret Parameters")]
    [Min(0.01f)] public float maxHealth = 100f;
    [Min(0f)] public float range = 10f;
    [Tooltip("Enable to prevent shooting through layers selected in the Turret obstacle mask.")]
    public bool requireLineOfSight;

    [Header("Gun Parameters")]
    [Min(1)] public int burstBullets = 3;
    [Tooltip("Seconds between bullets within one burst.")]
    [Min(0.01f)] public float burstInterval = 0.1f;
    [Tooltip("Seconds after the last shot before another burst can begin.")]
    [Min(0.01f)] public float fireInterval = 2f;

    [Header("Homing Projectile Parameters")]
    [FormerlySerializedAs("bulletConfig")]
    public HomingConfig homingConfig = HomingConfig.Default;
}

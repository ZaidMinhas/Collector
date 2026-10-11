using UnityEngine;

[CreateAssetMenu(fileName = "SoldierConfig", menuName = "Scriptable Objects/SoldierConfig")]
public class SoldierConfig : ScriptableObject
{
    [Header("Enemy Parameters")]
    [Range(0, 100)] public int baseSpeed = 1;
    [Range(0, 360)] public float searchRangeAngle = 1;
    [Range(0, 100)] public int maxHealth = 100;
    [Header("Gun Parameters")]
    [Range(0, 100)] public float burstBullets = 3;
    [Range(0, 5)] public float burstFireRate = 20;
    [Range(0, 100)] public float fireInterval = 20;
    [Header("Bullet Parameters")]
    public BulletConfig bulletConfig;
}

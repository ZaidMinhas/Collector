using UnityEngine;

[CreateAssetMenu(fileName = "SoldierConfig", menuName = "Scriptable Objects/SoldierConfig")]
public class SoldierConfig : ScriptableObject
{
    [Range(0, 100)] public int baseSpeed = 1;
    [Range(0, 360)] public float searchRangeAngle = 1;

}

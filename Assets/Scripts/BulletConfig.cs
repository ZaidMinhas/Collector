using System;
using UnityEngine;

[Serializable]
public struct BulletConfig
{
    [Range(0, 100)] public float speed;
    [Range(0, 5)] public float damage;
}

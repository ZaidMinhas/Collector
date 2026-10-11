using System;
using UnityEngine;

[Serializable]
public struct HomingConfig
{
    [Min(0f)] public float speed;
    [Min(0f)] public float damage;
    [Tooltip("Maximum steering speed in degrees per second.")]
    [Min(0f)] public float turnSpeed;
    [Min(0.01f)] public float lifetime;
    public bool affectedByGravity;
    public float gravityModifier;
    [Tooltip("Use -90 for artwork pointing up; 0 for artwork pointing right.")]
    public float spriteRotationOffset;

    public static HomingConfig Default => new HomingConfig
    {
        speed = 10f,
        damage = 1f,
        turnSpeed = 180f,
        lifetime = 8f,
        affectedByGravity = false,
        gravityModifier = 1f,
        spriteRotationOffset = 0f
    };
}

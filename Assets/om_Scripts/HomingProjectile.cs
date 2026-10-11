using UnityEngine;

// Use this instead of Projectile and BulletMovement on a missile prefab.
public class HomingProjectile : Projectile
{
    [HideInInspector] public Transform target;
    [HideInInspector] [Min(0f)] public float turnSpeed = 180f;
    [HideInInspector] [Min(0.01f)] public float lifetime = 8f;
    [HideInInspector] [Tooltip("Use -90 if the sprite artwork points up; 0 if it points right.")]
    public float spriteRotationOffset;

    public void Launch(float angle, HomingConfig config, Transform newTarget, Transform shooter)
    {
        target = newTarget;
        turnSpeed = Mathf.Max(0f, config.turnSpeed);
        lifetime = Mathf.Max(0.01f, config.lifetime);
        affectedByGravity = config.affectedByGravity;
        gravityModifier = config.gravityModifier;
        spriteRotationOffset = config.spriteRotationOffset;
        //SetOwner(shooter);
        base.Launch(angle, new BulletConfig
        {
            speed = Mathf.Max(0f, config.speed),
            damage = Mathf.Max(0f, config.damage)
        });
        rBody.SetRotation(angle + spriteRotationOffset);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    protected void Start()
    {
        base.Start();
        Destroy(gameObject, Mathf.Max(0.01f, lifetime));
    }

    protected void FixedUpdate()
    {
        Vector2 velocity = rBody.linearVelocity;
        if (target != null)
        {
            Vector2 toTarget = (Vector2)(target.position - transform.position);
            if (toTarget.sqrMagnitude > 0.000001f)
            {
                float currentAngle = velocity.sqrMagnitude > 0.000001f
                    ? Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg
                    : rBody.rotation - spriteRotationOffset;
                float targetAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
                float newAngle = Mathf.MoveTowardsAngle(currentAngle, targetAngle,
                    Mathf.Max(0f, turnSpeed) * Time.fixedDeltaTime);
                float radians = newAngle * Mathf.Deg2Rad;
                float movementSpeed = velocity.sqrMagnitude > 0.000001f
                    ? velocity.magnitude : Mathf.Max(0f, speed);
                rBody.linearVelocity = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians))
                    * movementSpeed;
            }
        }

        // Gravity changes velocity after steering; it is never overwritten by a
        // separate Transform.Translate movement system.
        base.FixedUpdate();

        velocity = rBody.linearVelocity;
        if (velocity.sqrMagnitude > 0.000001f)
        {
            float heading = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            rBody.SetRotation(heading + spriteRotationOffset);
        }
    }
}

using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(HealthComponent))]
public class Soldier : MonoBehaviour
{
    public Transform player;
    public Transform planet;
    public SoldierConfig soldierConfig;
    public Projectile bullet;
    private HealthComponent healthComponent;
    

    [HideInInspector] public float angle;
    [HideInInspector] public float radius;

    private float targetAngle;
    private bool hasTargetAngle;
    private float lastDir = 1f;
    private SpriteRenderer spriteRenderer;
    private float nextShotTime = 0;
    private bool isShooting;
    private Coroutine burstRoutine;

    void Awake()
    {
        healthComponent = GetComponent<HealthComponent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    void Start()
    {
        radius = planet.GetComponent<SpriteRenderer>().bounds.extents.x;
        healthComponent.setHealth(soldierConfig.maxHealth);
        healthComponent.OnDeath += Die;

        // Initialize angle in radians
        Vector2 offset = transform.position - planet.position;
        angle = Mathf.Atan2(offset.y, offset.x);
        UpdateRotation(0f);
    }

    void Update()
    {
        float targetDir = GetTargetDir();

        if (targetDir != 0f)
            Move(targetDir);
        else
        {
            Shoot();
        }

        UpdateRotation(targetDir);
    }

    public void Shoot()
    {
        if (!isActiveAndEnabled || isShooting || bullet == null || player == null ||
            soldierConfig == null || soldierConfig.burstBullets < 1f ||
            Time.time < nextShotTime)
            return;

        // Lock before starting: Update must not start another burst next frame.
        isShooting = true;
        burstRoutine = StartCoroutine(ShootBurst());
    }

    private IEnumerator ShootBurst()
    {
        int bulletCount = Mathf.Max(1, Mathf.RoundToInt(soldierConfig.burstBullets));
        // burstFireRate is bullets per second (20 means 0.05 seconds apart).
        float shotInterval = soldierConfig.burstFireRate;

        for (int i = 0; i < bulletCount; i++)
        {
            if (player == null || bullet == null)
                break;

            // Aim each shot from the Soldier's current position.
            Vector3 spawnPosition = transform.position;
            Vector2 direction = (Vector2)(player.position - spawnPosition);
            if (direction.sqrMagnitude > 0.000001f)
            {
                float rotation = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Projectile shot = Instantiate(bullet, spawnPosition, Quaternion.Euler(0f, 0f, rotation));
                
                shot.SetVelocity(rotation, 2);
                shot.affectedByGravity = true;
            }

            if (i < bulletCount - 1)
                yield return new WaitForSeconds(shotInterval);
        }

        // Cooldown starts after the final shot, with no extra shot delay.
        nextShotTime = Time.time + Mathf.Max(0.01f, soldierConfig.fireInterval);
        isShooting = false;
        burstRoutine = null;
    }

    private void OnDisable()
    {
        if (burstRoutine != null)
            StopCoroutine(burstRoutine);
        burstRoutine = null;
        isShooting = false;
        if (soldierConfig != null)
            nextShotTime = Time.time + Mathf.Max(0.01f, soldierConfig.fireInterval);
    }

    public void Move(float dir)
    {
        // Clamp the angular step so we reach the target without overshooting.
        float step = (soldierConfig.baseSpeed / radius) * Time.deltaTime;
        if (hasTargetAngle)
        {
            float remaining = Mathf.Abs(Mathf.DeltaAngle(
                angle * Mathf.Rad2Deg, targetAngle)) * Mathf.Deg2Rad;
            step = Mathf.Min(step, remaining);
        }
        angle -= step * dir;

        Vector2 offset = new Vector2(
            Mathf.Cos(angle),
            Mathf.Sin(angle)
        ) * radius;

        transform.position = (Vector2)planet.position + offset;
    }

    public float GetTargetDir()
    {
        float playerAngle = GetAngle(player.position);
        float enemyAngle = angle * Mathf.Rad2Deg;

        float halfRange = soldierConfig.searchRangeAngle / 2f;

        // Check if existing target is still within player's range
        if (hasTargetAngle)
        {
            float targetOffset = Mathf.DeltaAngle(
                playerAngle,
                targetAngle
            );

            if (Mathf.Abs(targetOffset) > halfRange)
                hasTargetAngle = false;
        }

        // Pick a new random target if needed
        if (!hasTargetAngle)
        {
            targetAngle = playerAngle + Random.Range(
                -halfRange,
                halfRange
            );

            hasTargetAngle = true;
        }

        // Find shortest direction around planet
        float targetDifference = Mathf.DeltaAngle(
            enemyAngle,
            targetAngle
        );

        // Keep the selected target after arrival until the player leaves its range.
        if (Mathf.Abs(targetDifference) <= 0.01f)
            return 0f;

        // Negative because Move() subtracts angle
        return -Mathf.Sign(targetDifference);
    }

    private float GetAngle(Vector2 position)
    {
        Vector2 offset = position - (Vector2)planet.position;

        return Mathf.Atan2(offset.y, offset.x) * Mathf.Rad2Deg;
    }
    
    private void UpdateRotation(float dir)
    {
        // Rotate so the sprite's local DOWN points toward the planet
        float angleDegrees = angle * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(
            0f,
            0f,
            angleDegrees - 90f
        );

        // Remember the last movement direction
        if (dir != 0f)
            lastDir = dir;

        // Flip sprite depending on movement direction
        spriteRenderer.flipX = lastDir < 0f;
    }
    

    public void Die()
    {
        Destroy(gameObject);
    }
}
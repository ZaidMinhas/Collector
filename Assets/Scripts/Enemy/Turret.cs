using System.Collections;
using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class Turret : MonoBehaviour
{
    public Transform player;
    public TurretConfig turretConfig;
    public Projectile bullet;

    [Header("Optional Gun Setup")]
    [Tooltip("A child gun or barrel to rotate. Leave empty to keep all artwork still.")]
    public Transform aimPivot;
    public Transform shootPoint;
    [Tooltip("Use -90 for gun artwork pointing up; 0 for artwork pointing right.")]
    public float aimRotationOffset;
    [Tooltip("Select only scenery that blocks shots, excluding the turret and player.")]
    public LayerMask obstacleMask;

    private HealthComponent healthComponent;
    private Coroutine burstRoutine;
    private bool isShooting;
    private float nextShotTime;

    private void Awake()
    {
        healthComponent = GetComponent<HealthComponent>();
    }

    private void Start()
    {
        if (turretConfig == null)
        {
            Debug.LogError("Turret requires a TurretConfig.", this);
            enabled = false;
            return;
        }

        healthComponent.setHealth(turretConfig.maxHealth);
        healthComponent.OnDeath += Die;
        if (aimPivot == transform)
        {
            Debug.LogWarning("Use a child gun as Aim Pivot to keep the turret base still.", this);
            aimPivot = null;
        }
    }

    public void Init(Transform targetPlayer)
    {
        player = targetPlayer;
    }

    private void Update()
    {
        if (Time.timeScale <= 0f) return;
        if (healthComponent.currentHealth <= 0f)
            return;

        AimAtPlayer();
        if (CanSeeTarget())
            Shoot();
        else if (isShooting)
            StopBurst();
    }

    private void AimAtPlayer()
    {
        if (player == null /*|| aimPivot == null*/)
            return;
        Vector2 direction = (Vector2)(player.position - transform.position);
        if (direction.sqrMagnitude <= 0.000001f)
            return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle + aimRotationOffset - 90f);
    }

    public bool CanSeeTarget()
    {
        if (player == null || turretConfig == null)
            return false;
        Vector2 offset = (Vector2)(player.position - transform.position);
        float range = Mathf.Max(0f, turretConfig.range);
        if (offset.sqrMagnitude > range * range)
            return false;

        if (turretConfig.requireLineOfSight)
        {
            Vector2 origin = shootPoint != null
                ? (Vector2)shootPoint.position : (Vector2)transform.position;
            Vector2 direction = (Vector2)player.position - origin;
            RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized,
                direction.magnitude, obstacleMask);
            if (hit.collider != null)
                return false;
        }
        return true;
    }

    public void Shoot()
    {
        if (!isActiveAndEnabled || isShooting || bullet == null ||
            healthComponent.currentHealth <= 0f || !CanSeeTarget() ||
            Time.time < nextShotTime)
            return;

        isShooting = true;
        burstRoutine = StartCoroutine(ShootBurst());
    }

    private IEnumerator ShootBurst()
    {
        int count = Mathf.Max(1, turretConfig.burstBullets);
        float interval = Mathf.Max(0.01f, turretConfig.burstInterval);
        for (int i = 0; i < count; i++)
        {
            if (bullet == null || healthComponent.currentHealth <= 0f || !CanSeeTarget())
                break;

            Vector3 position = shootPoint != null ? shootPoint.position : transform.position;
            Vector2 direction = (Vector2)(player.position - position);
            if (direction.sqrMagnitude > 0.000001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Projectile shot = Instantiate(bullet, position, Quaternion.Euler(0f, 0f, angle));
                HomingConfig config = turretConfig.homingConfig;
                if (shot is HomingProjectile homingShot)
                {
                    homingShot.Launch(angle, config, player, transform);
                }
                else
                {
                    // Ordinary projectiles use the shared non-homing settings.
                    shot.SetOwner(transform);
                    shot.affectedByGravity = config.affectedByGravity;
                    shot.gravityModifier = config.gravityModifier;
                    shot.Launch(angle, new BulletConfig
                    {
                        speed = Mathf.Max(0f, config.speed),
                        damage = Mathf.Max(0f, config.damage)
                    });
                    Destroy(shot.gameObject, Mathf.Max(0.01f, config.lifetime));
                }
                Manager.DefenderShotAudio.Play();
            }
            if (i < count - 1)
                yield return new WaitForSeconds(interval);
        }
        BeginCooldown();
        isShooting = false;
        burstRoutine = null;
    }

    private void BeginCooldown()
    {
        if (turretConfig != null)
            nextShotTime = Time.time + Mathf.Max(0.01f, turretConfig.fireInterval);
    }

    private void StopBurst()
    {
        if (burstRoutine != null)
            StopCoroutine(burstRoutine);
        burstRoutine = null;
        isShooting = false;
        BeginCooldown();
    }

    private void OnDisable()
    {
        StopBurst();
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
            healthComponent.OnDeath -= Die;
    }

    public void Die()
    {
        StopBurst();
        enabled = false;
        Destroy(gameObject);
    }

    private void OnDrawGizmosSelected()
    {
        if (turretConfig == null)
            return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, Mathf.Max(0f, turretConfig.range));
    }
}

using UnityEngine;

public class Soldier : MonoBehaviour
{
    public Transform player;
    public Transform planet;
    public SoldierConfig soldierConfig;

    public float angle;
    public float radius;

    private float targetAngle;
    private bool hasTargetAngle;
    private float lastDir = 1f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        radius = planet.GetComponent<SpriteRenderer>().bounds.extents.x;

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

        UpdateRotation(targetDir);
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
}
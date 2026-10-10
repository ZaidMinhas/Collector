using UnityEngine;

// Attach to a Soldier, or assign a Soldier from another object.
public class SoldierSearchDebugger : MonoBehaviour
{
    public Soldier soldier;
    public bool showSearchAngle = true;
    [Min(0f)] public float radiusOverride;
    [Range(8, 180)] public int arcSegments = 60;
    public Color searchColor = Color.yellow;
    public Color insideColor = Color.green;
    public Color outsideColor = Color.red;

    private void Reset()
    {
        soldier = GetComponent<Soldier>();
    }

    private void OnDrawGizmos()
    {
        Soldier target = soldier != null ? soldier : GetComponent<Soldier>();
        if (!showSearchAngle || target == null || target.planet == null ||
            target.player == null || target.soldierConfig == null)
            return;

        Vector3 center = target.planet.position;
        Vector2 playerOffset = (Vector2)(target.player.position - center);
        if (playerOffset.sqrMagnitude < 0.000001f)
            return;

        float radius = radiusOverride;
        if (radius <= 0f)
        {
            SpriteRenderer planetSprite = target.planet.GetComponent<SpriteRenderer>();
            radius = planetSprite != null
                ? planetSprite.bounds.extents.x
                : Vector2.Distance(target.transform.position, center);
        }
        if (radius <= 0f)
            return;

        float playerAngle = Mathf.Atan2(playerOffset.y, playerOffset.x) * Mathf.Rad2Deg;
        float halfRange = target.soldierConfig.searchRangeAngle / 2f;
        float startAngle = playerAngle - halfRange;
        float endAngle = playerAngle + halfRange;

        Color previousColor = Gizmos.color;
        Gizmos.color = searchColor;
        Gizmos.DrawLine(center, PointOnCircle(center, radius, startAngle));
        Gizmos.DrawLine(center, PointOnCircle(center, radius, endAngle));
        int segments = Mathf.Clamp(arcSegments, 8, 180);
        Vector3 previousPoint = PointOnCircle(center, radius, startAngle);
        for (int i = 1; i <= segments; i++)
        {
            float degrees = Mathf.Lerp(startAngle, endAngle, (float)i / segments);
            Vector3 point = PointOnCircle(center, radius, degrees);
            Gizmos.DrawLine(previousPoint, point);
            previousPoint = point;
        }

        // Cyan marks the player direction at the center of the search sector.
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center, PointOnCircle(center, radius, playerAngle));

        Vector2 enemyOffset = (Vector2)(target.transform.position - center);
        float enemyAngle = Application.isPlaying
            ? target.angle * Mathf.Rad2Deg
            : Mathf.Atan2(enemyOffset.y, enemyOffset.x) * Mathf.Rad2Deg;
        bool inside = Mathf.Abs(Mathf.DeltaAngle(enemyAngle, playerAngle)) <= halfRange;
        Gizmos.color = inside ? insideColor : outsideColor;
        Gizmos.DrawLine(center, PointOnCircle(center, radius, enemyAngle));
        Gizmos.DrawWireSphere(target.transform.position, radius * 0.025f);
        Gizmos.color = previousColor;
    }

    private static Vector3 PointOnCircle(Vector3 center, float radius, float degrees)
    {
        float radians = degrees * Mathf.Deg2Rad;
        return center + new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * radius;
    }
}

using UnityEngine;

public class visionFlyingTurret : MonoBehaviour
{
    private float radius = 4;
    private float timer;

    private GameObject newBullet;
    // public LayerMask targetMask;
    public LayerMask obstacleMask;
    [SerializeField] GameObject player;
    [SerializeField] GameObject bullet;
    [SerializeField] Transform spawnPoint;


    // Update is called once per frame
    void Update()
    {
        CanSeeTarget();
        spawnBullet();
    }

    public void spawnBullet()
    {
        timer += Time.deltaTime;

        if (CanSeeTarget() && timer > 0.5)
        {
            timer =0;
            Instantiate(bullet,spawnPoint);
        }
    }
    public bool CanSeeTarget()
    {
        Vector2 dirToTarget = (player.transform.position - transform.position).normalized;

        // 1. Distance check
        if (Vector2.Distance(transform.position, player.transform.position) > radius)
            return false;

        // // 2. Angle check
        // float checkAngle = Vector2.Angle(transform.up, dirToTarget);
        // if (checkAngle > angle/ 2f)
        //     return false;

        // 3. Line-of-sight raycast
        RaycastHit2D hit = Physics2D.Raycast(transform.position, dirToTarget, radius, obstacleMask);
        if (hit && hit.collider != player.GetComponent<Collider2D>())
            return false;

        return true;
    }
}

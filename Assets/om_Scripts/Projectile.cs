using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 1.0f;
    public float angle = 45f;

    public bool affectedByGravity = true;

    private Rigidbody2D rBody;

    private void Awake()
    {
        rBody = GetComponent<Rigidbody2D>();
    }

    public void SetVelocity(float angle, float speed)
    {
        float radians = angle * Mathf.Deg2Rad;
        Vector3 velocity = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * speed;
        rBody.linearVelocity = velocity;
    }

    private void Update()
    {
        if (!affectedByGravity) return;
        UniverseManager u = UniverseManager.Universe();

        foreach (StaticMass m in u.allMasses)
        {
            Vector3 pullDir = m.transform.position - transform.position;
            float r = pullDir.magnitude;
            float f = u.G * (m.mass / (r*r));
            Vector3 pullVec = pullDir * f;
            rBody.linearVelocity = (Vector3)rBody.linearVelocity + (pullVec*Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Destroy(gameObject);
    }
}

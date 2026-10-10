using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool affectedByGravity = true;
    public float gravityModifier = 1.0f;
    public float damage = 1;
    public float speed = 1;
    private bool isHitting;

    private Rigidbody2D rBody;

    private void Awake()
    {
        rBody = GetComponent<Rigidbody2D>();
        rBody.gravityScale = 0f;
    }

    private void Start()
    {
        StartCoroutine(DelayHitting());
    }

    public void SetVelocity(float angle)
    {
        SetVelocity(angle, speed);
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
            float f = u.G * (m.mass / Mathf.Pow(r, m.rExponent));
            Vector3 pullVec = pullDir * (f * gravityModifier);
            rBody.linearVelocity = (Vector3)rBody.linearVelocity + (pullVec*Time.deltaTime);
        }
    }

    //By Zaid
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isHitting) return;
        
        HealthComponent target = other.GetComponentInParent<HealthComponent>();
        if (target == null) return;
        
        target.TakeDamage(damage);
        Destroy(gameObject);
    }
    
    IEnumerator DelayHitting()
    {
        yield return new WaitForSeconds(0.2f);
        isHitting = true;
    }

}

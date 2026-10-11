using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public bool affectedByGravity = true;
    public float gravityModifier = 1.0f;
    public float damage = 1;
    public float speed = 1;
    private bool isHitting;
    public bool isDebug;

    public Rigidbody2D rBody;
    private Transform owner;

    private void Awake()
    {
        rBody = GetComponent<Rigidbody2D>();
        rBody.gravityScale = 0f;
    }

    protected virtual void Start()
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
        transform.up = velocity;
    }

    public void Launch(float angle, BulletConfig bulletConfig)
    {
        this.damage = bulletConfig.damage;
        this.speed = bulletConfig.speed;
        SetVelocity(angle, bulletConfig.speed);
    }

    public void SetOwner(Transform shooter)
    {
        owner = shooter;
    }

    protected virtual void FixedUpdate()
    {
        if (!affectedByGravity) return;
        rBody.linearVelocity = UniverseManager.Universe().UpdateVelocityForBody(
            transform.position, rBody.linearVelocity, gravityModifier, Time.deltaTime);
        transform.up = rBody.linearVelocity;
    }

    //By Zaid
    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (owner != null && other.transform.IsChildOf(owner)) return;
        if (!isHitting) return;

        if (isDebug)
        {
            Debug.Log(gameObject.name + " collided with " + other.name);
        }
        isHitting = false;
        Destroy(gameObject);
        HealthComponent target = other.GetComponentInParent<HealthComponent>();
        if (target == null) return;
        target.TakeDamage(damage);
    }
    
    IEnumerator DelayHitting()
    {
        yield return new WaitForSeconds(0.1f);
        isHitting = true;
    }
}

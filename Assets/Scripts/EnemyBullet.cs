using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet : MonoBehaviour
{
    [Min(0f)] public float speed = 10f;
    [Min(0.01f)] public float lifetime = 5f;
    [Min(0)] public int damage = 1;

    [Tooltip("Select the Player and/or Enemy hitbox layers this bullet can damage.")]
    public LayerMask targetLayers = ~0;

    private Rigidbody2D body;
    private GameObject owner;
    private bool launched;
    private bool hasHit;

    private void Awake()
    {
        body = GetComponent<Rigidbody2D>();
        body.gravityScale = 0f;
    }

    // Passing the shooter prevents the bullet from damaging its owner.
    public void Launch(Vector2 direction, GameObject shooter = null)
    {
        owner = shooter;
        hasHit = false;
        launched = true;
        body.linearVelocity = direction.normalized * speed;
        Destroy(gameObject, Mathf.Max(0.01f, lifetime));
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!launched || hasHit)
            return;

        if (owner != null && other.transform.IsChildOf(owner.transform))
            return;

        // Use the collider object's layer, including child hitboxes.
        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
            return;

        IDamagable target = other.GetComponentInParent<IDamagable>();
        if (target == null)
            return;

        Component targetComponent = target as Component;
        if (owner != null && targetComponent != null &&
            targetComponent.transform.IsChildOf(owner.transform))
            return;

        // Destroy is deferred, so block any further hits immediately.
        hasHit = true;
        body.linearVelocity = Vector2.zero;
        target.TakeDamage(damage);
        Destroy(gameObject);
    }
}

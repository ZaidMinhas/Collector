using UnityEngine;

public class DestroyOnCollide : MonoBehaviour
{
    public bool onlyOnTrigger = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (onlyOnTrigger && !collision.isTrigger) return;
        Destroy(gameObject);
    }
}

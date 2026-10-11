using UnityEngine;

public class DestroyableEnemy : MonoBehaviour
{
    private void Start()
    {
        if (gameObject.GetComponent<Collider2D>() == null) return;
        UniverseManager.Universe().allEnemies.Add(this);
    }
}

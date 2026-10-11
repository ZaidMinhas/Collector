using UnityEngine;

public class DestroyableEnemy : MonoBehaviour
{
    public bool isBoss = false;

    private void Start()
    {
        if (gameObject.GetComponent<Collider2D>() == null) return;
        if (gameObject.GetComponent<HealthComponent>() == null) return;
        UniverseManager.Universe().allEnemies.Add(this);
    }

    public void Kill()
    {
        UniverseManager.Universe().allEnemies.Remove(this);
        UniverseManager.Universe().RefreshLists();
        Destroy(gameObject);
    }
}

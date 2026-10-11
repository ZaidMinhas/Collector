using System.Collections.Generic;
using UnityEngine;

public class UniverseManager : MonoBehaviour
{
    public float G = 9.8f;

    public int maxGravityHoles = 8;

    public List<StaticMass> allMasses;

    public List<DestroyableEnemy> allEnemies;

    private static UniverseManager _universe;

    private void Awake()
    {
        _universe = this;
    }

    public static UniverseManager Universe() => _universe;

    public bool AtCapacity()
    {
        int cnt = 0;
        foreach (StaticMass mass in allMasses)
        {
            if (mass == null) continue;
            if (mass.gameObject.GetComponent<MortalEntity>() != null) cnt++;
        }

        return cnt >= maxGravityHoles;
    }

    public Vector3 UpdateVelocityForBody(Vector3 bodyPos, Vector3 currentVelocity, float gravityMult, float timeMult)
    {
        foreach (StaticMass m in allMasses)
        {
            if (m == null) continue;
            Vector3 pullDir = m.transform.position - bodyPos;
            float r = pullDir.magnitude;
            float f = G * (m.mass / Mathf.Pow(r, m.rExponent));
            Vector3 pullVec = pullDir * (f * gravityMult);
            currentVelocity += (pullVec * timeMult);
        }
        return currentVelocity;
    }

    public void RefreshLists()
    {
        allMasses.RemoveAll(m => m == null);
        allEnemies.RemoveAll(e => e == null);
    }
}

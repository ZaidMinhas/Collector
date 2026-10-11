using System.Collections.Generic;
using UnityEngine;

public class UniverseManager : MonoBehaviour
{
    public float G = 9.8f;

    public List<StaticMass> allMasses;

    private static UniverseManager _universe;

    private void Awake()
    {
        _universe = this;
    }

    public static UniverseManager Universe() => _universe;

    public Vector3 UpdateVelocityForBody(Vector3 bodyPos, Vector3 currentVelocity, float gravityMult, float timeMult)
    {
        foreach (StaticMass m in allMasses)
        {
            Vector3 pullDir = m.transform.position - bodyPos;
            float r = pullDir.magnitude;
            float f = G * (m.mass / Mathf.Pow(r, m.rExponent));
            Vector3 pullVec = pullDir * (f * gravityMult);
            currentVelocity += (pullVec * timeMult);
        }
        return currentVelocity;
    }
}

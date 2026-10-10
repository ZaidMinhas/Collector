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
}

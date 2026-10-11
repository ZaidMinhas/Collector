using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    public PlanetManager planetToOrbit;
    public float orbitSpeed;

    private float targetAlt = 0f;
    private int fCnt = 0;

    private void Start()
    {
        if (planetToOrbit == null) return;
        targetAlt = (planetToOrbit.transform.position - transform.position).magnitude;
    }

    private void Update()
    {
        if (planetToOrbit == null) return;

        Vector3 directionToPlanet = (planetToOrbit.transform.position - transform.position).normalized;
        Vector3 moveDirection = Vector3.Cross(directionToPlanet, Vector3.forward).normalized;
        transform.position += orbitSpeed * Time.deltaTime * moveDirection.normalized;

        // bind to default alt
        // only do occassionally
        if (planetToOrbit == null) return;
        if (fCnt++ > 1024f) { fCnt = 0; return; }
        Vector3 dir = (transform.position - planetToOrbit.transform.position).normalized;
        transform.position = planetToOrbit.transform.position + (dir * targetAlt);
    }
}

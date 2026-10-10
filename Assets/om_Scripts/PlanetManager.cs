using UnityEngine;

public class PlanetManager : MonoBehaviour
{
    public float planetMass = 1.0f;
    
    public PlanetManager planetToOrbit;
    public float orbitSpeed;

    private void FixedUpdate()
    {
        if (planetToOrbit == null) return;

        Vector3 directionToPlanet = (planetToOrbit.transform.position - transform.position).normalized;
        Vector3 moveDirection = Vector3.Cross(directionToPlanet, Vector3.forward).normalized;
        transform.position += orbitSpeed * Time.fixedDeltaTime * moveDirection.normalized;
    }
}

using UnityEngine;

public class PlanetManager : MonoBehaviour
{   
    public PlanetManager planetToOrbit;
    public float orbitSpeed;

    private void Update()
    {
        if (planetToOrbit == null) return;

        Vector3 directionToPlanet = (planetToOrbit.transform.position - transform.position).normalized;
        Vector3 moveDirection = Vector3.Cross(directionToPlanet, Vector3.forward).normalized;
        transform.position += orbitSpeed * Time.deltaTime * moveDirection.normalized;
    }
}

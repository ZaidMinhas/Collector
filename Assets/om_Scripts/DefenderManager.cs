using UnityEngine;

public class DefenderManager : MonoBehaviour
{
    public LeaderManager leader;
    public PlanetManager planet;

    public float movementSpeed = 6.0f;


    private void Update()
    {
        Vector3 directionToPlanet = (planet.transform.position - transform.position).normalized;

        // MOVEMENT

        HandleAutoMovement(directionToPlanet, movementSpeed);
    }

    private void HandleAutoMovement(Vector3 directionToPlanet, float speed)
    {
        Vector3 moveOffset = Vector3.Cross(directionToPlanet, Vector3.forward).normalized;
        moveOffset *= Time.deltaTime * speed;

        Vector3 leftOption = transform.position + moveOffset;
        Vector3 rightOption = transform.position - moveOffset;
        float leftDist = (leader.transform.position - leftOption).magnitude;
        float rightDist = (leader.transform.position - rightOption).magnitude;

        transform.position = (leftDist < rightDist) ? leftOption : rightOption;

        // ROTATION: Make UFO's bottom face the planet
        // For 2D (assuming Z axis is forward)
        float btDir = Mathf.Atan2(directionToPlanet.y, directionToPlanet.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, btDir);
    }
}

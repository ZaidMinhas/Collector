using System;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class DefenderManager : MonoBehaviour
{
    public LeaderManager leader;
    public PlanetManager planet;

    public float movementSpeed = 5.0f;
    public float defendRange = 1.0f;

    private float targetAlt = 0f;
    private float moveDir = 1.0f;
    private bool defendPass = false;

    private void Start()
    {
        targetAlt = (planet.transform.position - transform.position).magnitude;
        System.Random rng = new();
        moveDir = (rng.NextDouble() < 0.5f) ? 1.0f : -1.0f;
        movementSpeed += 2f * (float)rng.NextDouble();
        defendRange += (float)rng.NextDouble()*1.5f;
        FaceMoveDir();
    }


    private void Update()
    {
        if (leader == null) return;

        Vector3 directionToPlanet = (planet.transform.position - transform.position).normalized;

        // MOVEMENT

        HandleAutoMovement(directionToPlanet, movementSpeed);
    }

    private void HandleAutoMovement(Vector3 directionToPlanet, float speed)
    {
        Vector3 moveOffset = Vector3.Cross(directionToPlanet, Vector3.forward).normalized;
        moveOffset *= Time.deltaTime * speed;

        Vector3 moveTo = transform.position + (moveDir * moveOffset);
        float distToLeader = (leader.transform.position - transform.position).magnitude;

        if (!defendPass && (distToLeader < defendRange))
        {
            defendPass = true;
        }
        if (defendPass && (distToLeader > defendRange))
        {
            defendPass = false; moveDir *= -1f; FaceMoveDir();
        }

        transform.position = moveTo;

        // bind to default alt
        Vector3 dir = (transform.position - planet.transform.position).normalized;
        float altOff = 0f;
        float jumpRange = defendRange * 0.75f;
        if (distToLeader < jumpRange) altOff = (jumpRange - distToLeader);
        transform.position = planet.transform.position + (dir * (targetAlt + altOff));

        // ROTATION: Make bottom face the planet
        // For 2D (assuming Z axis is forward)
        float btDir = Mathf.Atan2(directionToPlanet.y, directionToPlanet.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, btDir);
    }

    private void FaceMoveDir()
    {
        transform.localScale = new(Mathf.Abs(transform.localScale.x) * -moveDir, 
            transform.localScale.y, transform.localScale.z);

    }
}

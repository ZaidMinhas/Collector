using UnityEngine;

public class HoverSway : MonoBehaviour
{
    [Header("Side To Side")]
    [Min(0f)] public float swayDistance = 0.15f;
    [Min(0f)] public float swaySpeed = 0.5f;

    [Header("Tilt")]
    [Min(0f)] public float tiltAngle = 6f;

    [Header("Bob")]
    [Min(0f)] public float bobHeight = 0.06f;
    [Min(0f)] public float bobSpeed = 0.9f;

    [Header("Wobble")]
    [Range(0f, 1f)] public float unsteadiness = 0.4f;

    Vector3 appliedOffset;
    Quaternion appliedTilt = Quaternion.identity;
    float seed;

    void OnEnable()
    {
        seed = Random.Range(0f, 100f);
        appliedOffset = Vector3.zero;
        appliedTilt = Quaternion.identity;
    }

    void OnDisable()
    {
        transform.localPosition -= appliedOffset;
        transform.localRotation *= Quaternion.Inverse(appliedTilt);
        appliedOffset = Vector3.zero;
        appliedTilt = Quaternion.identity;
    }

    void LateUpdate()
    {
        float t = Time.time;
        float tau = Mathf.PI * 2f;

        float swayPhase = t * swaySpeed * tau;
        float noiseX = (Mathf.PerlinNoise(seed, t * swaySpeed * 1.7f) - 0.5f) * 2f;
        float noiseY = (Mathf.PerlinNoise(seed + 31f, t * bobSpeed * 1.3f) - 0.5f) * 2f;

        float side = Mathf.Lerp(Mathf.Sin(swayPhase), noiseX, unsteadiness);
        float sideVelocity = Mathf.Cos(swayPhase);
        float up = Mathf.Lerp(Mathf.Sin(t * bobSpeed * tau + seed), noiseY, unsteadiness);

        Vector3 offset = new Vector3(side * swayDistance, up * bobHeight, 0f);
        float roll = -Mathf.Lerp(sideVelocity, noiseX, unsteadiness) * tiltAngle;
        Quaternion tilt = Quaternion.Euler(0f, 0f, roll);

        transform.localPosition += offset - appliedOffset;
        transform.localRotation = transform.localRotation * Quaternion.Inverse(appliedTilt) * tilt;

        appliedOffset = offset;
        appliedTilt = tilt;
    }
}

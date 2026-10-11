using UnityEngine;

public class ShootingStars : MonoBehaviour
{
    private int timer = 0;
    private bool flipDir = false;

    private void FixedUpdate()
    {
        if (timer++ > 108)
        {
            timer = 0;
            Vector3 s = transform.localScale;
            transform.localScale = flipDir ? new(-s.x, s.y, s.z) : new(s.x, -s.y, s.z);
            flipDir = !flipDir;
        }
    }
}

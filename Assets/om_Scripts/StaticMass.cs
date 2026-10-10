using UnityEngine;

public class StaticMass : MonoBehaviour
{
    // if negative, object will repel
    public float mass;

    // in real life, gravity is inv. proportional to the square of the distance from the object (r^2)
    // set this to 3 to change to (r^3), 4 for (r^4), etc.
    public float rExponent = 2f;

    void Start()
    {
        
    }

    void Update()
    {
        
    }
}

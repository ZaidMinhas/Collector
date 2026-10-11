using UnityEngine;

public class fire : MonoBehaviour
{
    [SerializeField] float fireSpeed;
    [SerializeField] GameObject bullet;
    private GameObject insOfBullet;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        firingBullet();
    }

    public void firingBullet()
    {
        

    }

}

using System;
using Unity.VisualScripting;
using UnityEngine;


// GUIDED BULLET LOGIC
// the code is responsivle for constant movment of the bullet and the time it takes to self destruct if it hits nothing
public class BulletMovement : MonoBehaviour
{
    [SerializeField] float bulletSpeed;
    [SerializeField] float rotationSpeed;
    GameObject player;
    float timer;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        timer = Time.time + 2;

    }
    void Update()
    {   
        rotateBullet();
        GuidedbulletMovement();
        destroyBullet();

    }

    public void GuidedbulletMovement(){
        transform.Translate(bulletSpeed * Time.deltaTime * bulletDirection());  
    }

    public void rotateBullet()
    {
    Vector2 direction = bulletDirection();

    float rotationAngle = Mathf.Atan2(
        direction.y,
        direction.x
    ) * Mathf.Rad2Deg;

    Quaternion targetRotation = Quaternion.Euler(
        0f, 0f, rotationAngle
    );

    transform.rotation = Quaternion.RotateTowards(
        transform.rotation,
        targetRotation,
        rotationSpeed * Time.deltaTime
    );
    }

    public Vector2 bulletDirection()
    {
        Vector2 bulletTrajectory = player.transform.position - transform.position;
        return bulletTrajectory;
    }
    void OnTriggerEnter2D(Collider2D other)
    {
        Destroy(gameObject);
    }

    public void destroyBullet()
    {
        if (timer < Time.time)
        {
            Destroy(gameObject);
        }
    }
}

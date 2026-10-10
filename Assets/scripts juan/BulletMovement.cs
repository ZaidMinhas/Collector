using System;
using Unity.VisualScripting;
using UnityEngine;


// GUIDED BULLET LOGIC
// the code is responsivle for constant movment of the bullet and the time it takes to self destruct if it hits nothing
public class BulletMovement : MonoBehaviour
{
    [SerializeField] float bulletSpeed;
    GameObject player;

    float timer;


    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        timer = Time.time + 2;

    }
    void Update()
    {
        GuidedbulletMovement();
        destroyBullet();

    }

    public void GuidedbulletMovement(){
        transform.Translate(bulletSpeed * Time.deltaTime * bulletDirection());
    }

    public void rotateBullet()
    {
        
    }

    public Vector2 bulletDirection()
    {
        Vector2 bulletTrajectory = player.transform.position - transform.position;
        return bulletTrajectory;
    }

    void OnCollisionEnter2D()
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

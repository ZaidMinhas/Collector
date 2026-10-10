using Unity.VisualScripting;
using UnityEngine;

public class UfoManager : MonoBehaviour
{
    public PlanetManager planetManager;

    public Projectile defaultProjectile;

    public float moveSpeed = 5.0f;
    public float shootSpeed = 1.0f;

    void Start()
    {

    }

    void Update()
    {
        if (defaultProjectile == null) return;

        // PROJECTILES

        if (Shooting())
        {
            // Get mouse position in world space
            Vector3 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
            mouseWorldPos.z = transform.position.z; // Ensure same Z as UFO

            // Calculate direction and angle
            Vector3 dir = (mouseWorldPos - transform.position).normalized;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

            // Instantiate and set projectile
            Projectile proj = Instantiate(defaultProjectile, transform.position, Quaternion.identity);
            proj.SetVelocity(angle, shootSpeed);
            proj.affectedByGravity = true;
        }
    }

    private void FixedUpdate()
    {
        if (planetManager == null) return;

        // MOVEMENT

        Vector3 directionToPlanet = (planetManager.transform.position - transform.position).normalized;
        Vector3 moveDirection = Vector3.zero;

        if (MoveUp()) moveDirection -= directionToPlanet;
        if (MoveDown()) moveDirection += directionToPlanet;

        if (MoveRight()) moveDirection -= Vector3.Cross(directionToPlanet, Vector3.forward).normalized;
        if (MoveLeft()) moveDirection += Vector3.Cross(directionToPlanet, Vector3.forward).normalized;


        if (moveDirection != Vector3.zero)
        {
            transform.position += moveSpeed * Time.fixedDeltaTime * moveDirection.normalized;
        }

        // ROTATION: Make UFO's bottom face the planet
        // For 2D (assuming Z axis is forward)
        float angle = Mathf.Atan2(directionToPlanet.y, directionToPlanet.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);

    }

    private bool MoveLeft() => Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);

    private bool MoveRight() => Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

    private bool MoveUp() => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

    private bool MoveDown() => Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

    private bool Shooting() => Input.GetKeyDown(KeyCode.Mouse0);
}

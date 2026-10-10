using Unity.VisualScripting;
using UnityEngine;

public class UfoManager : MonoBehaviour
{
    public PlanetManager planetManager;

    public Projectile defaultProjectile;

    public Abductable abductionTarget;
    public GameObject abductionBeam;

    public GameObject forceField;

    public StaticMass placeableMass;
    public StaticMass placeableNegMass;

    public float moveSpeed = 5.0f;
    public float shotSpeed = 8.0f;

    private void Start()
    {
        abductionBeam.SetActive(false);
        forceField.SetActive(false);
    }

    void Update()
    {
        if (defaultProjectile == null) return;
        if (planetManager == null) return;
        if (placeableMass == null) return;
        if (placeableNegMass == null) return;

        // used below a bunch
        Vector3 directionToPlanet = (planetManager.transform.position - transform.position).normalized;

        // MOVEMENT

        HandleMovement(directionToPlanet);

        // PROJECTILES

        if (Shoot()) HandleShooting(1, 0f);

        // MASS PLACEMENT

        if (PlaceMass()) HandleMassPlacement(directionToPlanet, false);
        if (PlaceNegMass()) HandleMassPlacement(directionToPlanet, true);

        // ABDUCTION

        if (Abduct()) HandleAbduction();
        if (EndAbduct()) abductionBeam.SetActive(false);

        // SHIELD

        if (Shield()) HandleShield(2f);

        // LASER

        // tba

        // MULTISHOT

        if (Multishot()) HandleShooting(8, 45f);

    }

    private float shieldTimer = 0f;
    private float shieldMax = 0f; // # of seconds
    private bool shieldOn = false;

    private void FixedUpdate()
    {
        if (!shieldOn) return;
        if (shieldTimer++ >= (shieldMax*50f))
        {
            forceField.SetActive(false);
            shieldOn = false;
        }
    }


    // SEPARATE HANDLER FUNCTIONS

    private void HandleMovement(Vector3 directionToPlanet)
    {
        Vector3 moveDirection = Vector3.zero;

        if (MoveUp()) moveDirection -= directionToPlanet;
        if (MoveDown()) moveDirection += directionToPlanet;

        if (MoveRight()) moveDirection -= Vector3.Cross(directionToPlanet, Vector3.forward).normalized;
        if (MoveLeft()) moveDirection += Vector3.Cross(directionToPlanet, Vector3.forward).normalized;


        if (moveDirection != Vector3.zero)
        {
            transform.position += moveSpeed * Time.deltaTime * moveDirection.normalized;
        }

        // ROTATION: Make UFO's bottom face the planet
        // For 2D (assuming Z axis is forward)
        float btDir = Mathf.Atan2(directionToPlanet.y, directionToPlanet.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, btDir);
    }

    private void HandleShooting(int bulletCount, float spread)
    {
        if (bulletCount == 0) return;
        if (shieldOn) return;

        // Get mouse position in world space
        Vector3 mouseScreenPos = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, Camera.main.nearClipPlane));
        mouseWorldPos.z = transform.position.z; // Ensure same Z as UFO

        // Calculate direction and angle
        Vector3 dir = (mouseWorldPos - transform.position).normalized;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        float c = bulletCount;
        for (float i = 0f; i < c; i++)
        {
            float offset = (c == 1) ? 0f : -(spread / 2f) + (spread * (i / (c - 1f)));
            float shotAngle = angle + offset;
            float radians = shotAngle * Mathf.Deg2Rad;
            Vector3 spawnOffset = new Vector3(Mathf.Cos(radians), Mathf.Sin(radians), 0f) * 0.5f;
            Vector3 spawnPos = transform.position + spawnOffset;

            Projectile proj = Instantiate(defaultProjectile, spawnPos, Quaternion.identity);
            proj.SetVelocity(shotAngle, shotSpeed);
            proj.affectedByGravity = true;
        }
    }

    private void HandleMassPlacement(Vector3 directionToPlanet, bool negativeMass)
    {
        StaticMass placeable = negativeMass ? placeableNegMass : placeableMass;
        StaticMass sm = Instantiate(placeable.gameObject).GetComponent<StaticMass>();
        sm.gameObject.transform.position = transform.position + directionToPlanet * 0.75f;
        UniverseManager.Universe().allMasses.Add(sm);
    }

    private void HandleAbduction()
    {
        abductionBeam.SetActive(true);

        // find and destroy target
        if (abductionTarget != null)
        {
            if ((abductionTarget.transform.position - transform.position).magnitude < 1f)
            {
                Bounds bounds = abductionTarget.GetComponent<Renderer>().bounds;
                Vector3 ufoPos = transform.position;
                Vector3 planetPos = planetManager.transform.position;
                Vector3 dir = (planetPos - ufoPos).normalized;
                if (bounds.IntersectRay(new(ufoPos, dir)))
                {
                    Destroy(abductionTarget.gameObject);
                    /*temp*/
                    gameObject.GetComponent<BidenBlast>().abductText.enabled = true;
                }
            }
        }
    }

    private void HandleShield(float durationSeconds)
    {
        forceField.SetActive(true);
        shieldTimer = 0f;
        shieldMax = durationSeconds;
        shieldOn = true;
    }




    // KEY STROKES

    private bool MoveLeft() => Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow);

    private bool MoveRight() => Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow);

    private bool MoveUp() => Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow);

    private bool MoveDown() => Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow);

    private bool Shoot() => Input.GetKeyDown(KeyCode.Mouse0);

    private bool Abduct() => Input.GetKeyDown(KeyCode.Space);

    private bool EndAbduct() => Input.GetKeyUp(KeyCode.Space);

    private bool PlaceMass() => Input.GetKeyDown(KeyCode.Alpha1);

    private bool PlaceNegMass() => Input.GetKeyDown(KeyCode.Alpha2);

    private bool Shield() => Input.GetKeyDown(KeyCode.Alpha3);

    private bool Laser() => Input.GetKeyDown(KeyCode.Alpha4);

    private bool Multishot() => Input.GetKeyDown(KeyCode.Alpha5);
}

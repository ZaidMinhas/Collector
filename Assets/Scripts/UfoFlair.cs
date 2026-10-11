using UnityEngine;
using UnityEngine.Rendering;

public class UfoFlair : MonoBehaviour
{
    [Header("Source")]
    public Transform ufo;
    [Min(1f)] public float pixelsPerUnit = 32f;
    public int sortingOrder = 0;
    [Min(0f)] public float depthOffset = 0.05f;

    [Header("Engine Trail")]
    [Min(0f)] public float trailRate = 40f;
    [Min(0.001f)] public float trailFullSpeed = 5f;
    [Min(0f)] public float trailMinSpeed = 0.6f;
    public Vector2 trailLifetime = new Vector2(0.3f, 0.7f);
    public Vector2 trailSizePixels = new Vector2(1f, 3f);
    [Min(0f)] public float trailSpread = 0.15f;
    [Min(0f)] public float trailBrightness = 1.8f;
    public Color[] trailColors =
    {
        new Color(1f, 0.3f, 0.35f),
        new Color(1f, 0.85f, 0.3f),
        new Color(0.4f, 1f, 0.5f),
        new Color(0.35f, 0.9f, 1f),
        new Color(1f, 0.45f, 1f)
    };

    Vector3 localCenter;
    float worldWidth;

    ParticleSystem trailSystem;
    Material trailMaterial;
    Texture2D squareTexture;

    Vector3 lastPosition;
    float trailAccumulator;

    void Reset()
    {
        ufo = transform.parent;
    }

    void Start()
    {
        if (ufo == null) ufo = transform.parent;
        if (ufo == null) ufo = transform;

        MeasureUfo();
        lastPosition = ufo.position;

        squareTexture = new Texture2D(1, 1, TextureFormat.RGBA32, false)
        {
            name = "UfoTrailPixel",
            filterMode = FilterMode.Point,
            hideFlags = HideFlags.DontSave
        };
        squareTexture.SetPixel(0, 0, Color.white);
        squareTexture.Apply();

        trailMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            name = "UfoTrailMaterial",
            hideFlags = HideFlags.DontSave,
            mainTexture = squareTexture
        };
        trailSystem = CreateSystem();
    }

    void OnDestroy()
    {
        if (trailSystem != null) Destroy(trailSystem.gameObject);
        Destroy(trailMaterial);
        Destroy(squareTexture);
    }

    void LateUpdate()
    {
        if (ufo == null) return;

        Camera cam = Camera.main;
        Vector3 forward = cam != null ? cam.transform.forward : Vector3.forward;
        Vector3 right = cam != null ? cam.transform.right : Vector3.right;
        Vector3 up = cam != null ? cam.transform.up : Vector3.up;
        Vector3 center = ufo.TransformPoint(localCenter);

        trailMaterial.color = Color.white * trailBrightness;
        trailSystem.GetComponent<ParticleSystemRenderer>().sortingOrder = sortingOrder;

        float dt = Time.deltaTime;
        Vector3 position = ufo.position;
        Vector3 velocity = dt > 0f ? (position - lastPosition) / dt : Vector3.zero;
        lastPosition = position;
        velocity -= forward * Vector3.Dot(velocity, forward);

        float speed = velocity.magnitude;
        if (speed < trailMinSpeed) return;

        Vector3 back = -velocity / speed;

        trailAccumulator += trailRate * Mathf.Clamp01(speed / trailFullSpeed) * dt;
        while (trailAccumulator >= 1f)
        {
            trailAccumulator -= 1f;

            Vector2 jitter = Random.insideUnitCircle * worldWidth * trailSpread;
            Color c = trailColors.Length > 0 ? trailColors[Random.Range(0, trailColors.Length)] : Color.white;

            var emit = new ParticleSystem.EmitParams
            {
                position = center + back * worldWidth * 0.45f + right * jitter.x + up * jitter.y + forward * depthOffset,
                velocity = velocity * -0.2f + (Vector3)(Random.insideUnitCircle * 0.3f),
                startSize = Random.Range(trailSizePixels.x, trailSizePixels.y) / Mathf.Max(1f, pixelsPerUnit),
                startLifetime = Random.Range(trailLifetime.x, trailLifetime.y),
                startColor = c,
                rotation = 0f,
                applyShapeToPosition = false
            };
            trailSystem.Emit(emit, 1);
        }
    }

    void MeasureUfo()
    {
        bool found = false;
        Bounds bounds = new Bounds(ufo.position, Vector3.one);
        foreach (Renderer r in ufo.GetComponentsInChildren<Renderer>())
        {
            if (r is ParticleSystemRenderer || r.transform.IsChildOf(transform)) continue;
            if (!found) bounds = r.bounds;
            else bounds.Encapsulate(r.bounds);
            found = true;
        }

        localCenter = ufo.InverseTransformPoint(bounds.center);
        worldWidth = bounds.extents.x * 2f;
    }

    ParticleSystem CreateSystem()
    {
        var go = new GameObject("UFO Trail") { hideFlags = HideFlags.DontSave };
        go.transform.SetParent(transform, false);

        var system = go.AddComponent<ParticleSystem>();
        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = system.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0f;
        main.maxParticles = 2000;

        var emission = system.emission;
        emission.enabled = false;
        var shape = system.shape;
        shape.enabled = false;

        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
        var colorOverLifetime = system.colorOverLifetime;
        colorOverLifetime.enabled = true;
        colorOverLifetime.color = fade;

        var sizeOverLifetime = system.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 1f), new Keyframe(1f, 0.2f)));

        var psRenderer = go.GetComponent<ParticleSystemRenderer>();
        psRenderer.sharedMaterial = trailMaterial;
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        psRenderer.shadowCastingMode = ShadowCastingMode.Off;
        psRenderer.receiveShadows = false;
        psRenderer.sortingOrder = sortingOrder;

        system.Play();
        return system;
    }
}

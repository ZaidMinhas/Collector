using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class OrbitSparkles : MonoBehaviour
{
    public enum SparkleShape { Square, Plus }

    [Header("Source")]
    public CrystalOrbit orbit;

    [Header("Ring Dust")]
    [Min(0f)] public float ringRate = 15f;
    [Min(0f)] public float ringSpread = 0.3f;

    [Header("Crystal Trails")]
    [Min(0f)] public float crystalRate = 10f;
    [Min(0f)] public float crystalSpread = 0.15f;

    [Header("Look")]
    public SparkleShape shape = SparkleShape.Plus;
    public Vector2 sizePixels = new Vector2(2f, 4f);
    public Vector2 lifetime = new Vector2(0.8f, 1.6f);
    [Min(0f)] public float drift = 0.2f;
    public bool twinkle = true;

    [Header("Color")]
    public bool useOrbitColors = true;
    public Gradient colorGradient = new Gradient();
    [Min(0f)] public float intensity = 1.5f;
    public int sortingOrder = 101;

    ParticleSystem ps;
    ParticleSystemRenderer psRenderer;
    Material runtimeMaterial;
    Texture2D sparkleTexture;
    SparkleShape cachedShape;
    float ringAccumulator;
    float crystalAccumulator;

    void Reset()
    {
        orbit = GetComponentInParent<CrystalOrbit>();
    }

    void Awake()
    {
        if (orbit == null) orbit = GetComponentInParent<CrystalOrbit>();
        ps = GetComponent<ParticleSystem>();
        psRenderer = GetComponent<ParticleSystemRenderer>();
        ConfigureParticleSystem();
    }

    void OnDestroy()
    {
        DestroySafe(runtimeMaterial);
        DestroySafe(sparkleTexture);
    }

    void Update()
    {
        if (orbit == null) return;

        if (sparkleTexture == null || cachedShape != shape) BuildTexture();
        runtimeMaterial.color = Color.white * intensity;
        psRenderer.sortingOrder = sortingOrder;
        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = twinkle;

        float dt = Time.deltaTime;

        ringAccumulator += ringRate * dt;
        while (ringAccumulator >= 1f)
        {
            ringAccumulator -= 1f;
            Vector3 p = orbit.GetPoint(Random.Range(0f, 360f)) + Random.insideUnitSphere * ringSpread;
            Emit(p);
        }

        crystalAccumulator += crystalRate * dt;
        while (crystalAccumulator >= 1f)
        {
            crystalAccumulator -= 1f;
            foreach (Transform crystal in orbit.crystals)
            {
                if (crystal == null) continue;
                Emit(crystal.position + Random.insideUnitSphere * crystalSpread);
            }
        }
    }

    void Emit(Vector3 position)
    {
        float ppu = Mathf.Max(1f, orbit.pixelsPerUnit);
        var emit = new ParticleSystem.EmitParams
        {
            position = position,
            velocity = Random.insideUnitSphere * drift,
            startSize = Random.Range(sizePixels.x, sizePixels.y) / ppu,
            startLifetime = Random.Range(lifetime.x, lifetime.y),
            startColor = PickColor(),
            applyShapeToPosition = false
        };
        ps.Emit(emit, 1);
    }

    Color PickColor()
    {
        if (!useOrbitColors) return colorGradient.Evaluate(Random.value);
        return orbit.UsesColor ? orbit.RingGradient.Evaluate(Random.value) : Color.white;
    }

    void ConfigureParticleSystem()
    {
        var main = ps.main;
        main.loop = true;
        main.playOnAwake = true;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0f;
        main.maxParticles = 2000;

        var emission = ps.emission;
        emission.enabled = false;
        var shapeModule = ps.shape;
        shapeModule.enabled = false;

        var colorOverLifetime = ps.colorOverLifetime;
        colorOverLifetime.enabled = true;
        var fade = new Gradient();
        fade.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[]
            {
                new GradientAlphaKey(0f, 0f),
                new GradientAlphaKey(1f, 0.1f),
                new GradientAlphaKey(1f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            });
        colorOverLifetime.color = fade;

        var sizeOverLifetime = ps.sizeOverLifetime;
        sizeOverLifetime.enabled = twinkle;
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f,
            new AnimationCurve(new Keyframe(0f, 0.2f), new Keyframe(0.25f, 1f), new Keyframe(1f, 0f)));

        runtimeMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            hideFlags = HideFlags.DontSave,
            name = "OrbitSparkleMaterial"
        };
        psRenderer.sharedMaterial = runtimeMaterial;
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        psRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        psRenderer.receiveShadows = false;

        BuildTexture();
        if (!ps.isPlaying) ps.Play();
    }

    void BuildTexture()
    {
        DestroySafe(sparkleTexture);

        int size = shape == SparkleShape.Plus ? 3 : 1;
        sparkleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave,
            name = "OrbitSparkleTexture"
        };

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
        {
            bool on = size == 1 || x == 1 || y == 1;
            sparkleTexture.SetPixel(x, y, on ? Color.white : Color.clear);
        }
        sparkleTexture.Apply();

        runtimeMaterial.mainTexture = sparkleTexture;
        cachedShape = shape;
    }

    static void DestroySafe(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }
}

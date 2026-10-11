using UnityEngine;
using UnityEngine.Rendering;

public class PlanetParticles : MonoBehaviour
{
    [Header("Source")]
    public Transform planet;

    [Header("Look")]
    public OrbitSparkles.SparkleShape shape = OrbitSparkles.SparkleShape.Square;
    [Min(1f)] public float pixelsPerUnit = 20f;
    public bool rainbow = false;
    [Range(0f, 1f)] public float lighten = 0f;
    [Min(0f)] public float intensity = 1.5f;
    public int sortingOrder = 102;

    [Header("Atmosphere Dust")]
    public bool atmosphere = true;
    [Min(0f)] public float atmosphereRate = 15f;
    [Min(0f)] public float atmosphereHeight = 0.25f;
    public float atmosphereDrift = 8f;
    public Vector2 atmosphereSize = new Vector2(1f, 2f);
    public Vector2 atmosphereLifetime = new Vector2(3f, 6f);
    [Range(0f, 1f)] public float atmosphereOpacity = 0.5f;

    [Header("Debris Belt")]
    public bool belt = true;
    [Range(0, 2000)] public int beltCount = 300;
    public Vector2 beltRadius = new Vector2(1.6f, 2.2f);
    [Range(0f, 89f)] public float beltTilt = 75f;
    [Range(-180f, 180f)] public float beltAngle = 20f;
    [Min(0f)] public float beltThickness = 0.05f;
    public float beltSpeed = 10f;
    public Vector2 beltSize = new Vector2(1f, 3f);

    [Header("Spiral")]
    public bool spiral = true;
    [Min(0f)] public float spiralRate = 12f;
    [Min(1.1f)] public float spiralStartRadius = 3f;
    public float spiralSpin = 40f;
    [Min(0.01f)] public float spiralPull = 0.4f;
    public Vector2 spiralSize = new Vector2(2f, 3f);

    [Header("Surface Sparks")]
    public bool sparks = true;
    public Vector2 sparkInterval = new Vector2(0.8f, 2.5f);
    public Vector2Int sparkCount = new Vector2Int(6, 14);
    [Min(0f)] public float sparkSpeed = 2f;
    [Min(0f)] public float sparkGravity = 3f;
    public Vector2 sparkSize = new Vector2(1f, 3f);
    public Vector2 sparkLifetime = new Vector2(0.6f, 1.2f);

    ParticleSystem atmosphereSystem;
    ParticleSystem beltSystem;
    ParticleSystem spiralSystem;
    ParticleSystem sparkSystem;

    Material runtimeMaterial;
    Texture2D sparkleTexture;
    OrbitSparkles.SparkleShape cachedShape;
    ParticleSystem.Particle[] buffer = new ParticleSystem.Particle[256];

    float atmosphereAccumulator;
    float spiralAccumulator;
    float nextSparkTime;

    float[] beltAngles = new float[0];
    float[] beltRadii = new float[0];
    float[] beltHeights = new float[0];
    float[] beltSizes = new float[0];
    Color[] beltColors = new Color[0];

    void Reset()
    {
        planet = transform.parent;
    }

    void OnValidate()
    {
        beltAngles = new float[0];
    }

    void OnEnable()
    {
        if (planet == null) planet = transform.parent;

        runtimeMaterial = new Material(Shader.Find("Sprites/Default"))
        {
            name = "PlanetParticlesMaterial",
            hideFlags = HideFlags.DontSave
        };
        BuildTexture();

        atmosphereSystem = CreateSystem("Atmosphere Dust", FadeInOut());
        beltSystem = CreateSystem("Debris Belt", null);
        spiralSystem = CreateSystem("Spiral", FadeIn());
        sparkSystem = CreateSystem("Surface Sparks", FadeOut());

        beltAngles = new float[0];
        nextSparkTime = Time.time + Random.Range(sparkInterval.x, sparkInterval.y);
    }

    void OnDisable()
    {
        DestroyOwned(atmosphereSystem);
        DestroyOwned(beltSystem);
        DestroyOwned(spiralSystem);
        DestroyOwned(sparkSystem);
        Destroy(runtimeMaterial);
        Destroy(sparkleTexture);
    }

    void Update()
    {
        if (planet == null) return;

        if (sparkleTexture == null || cachedShape != shape) BuildTexture();
        runtimeMaterial.color = Color.white * intensity;

        Vector3 center = PlanetPalette.Center(planet);
        float radius = PlanetPalette.Radius(planet);
        float dt = Time.deltaTime;
        Quaternion beltRotation = Quaternion.Euler(0f, 0f, beltAngle) * Quaternion.Euler(beltTilt, 0f, 0f);
        Vector3 beltAxis = beltRotation * Vector3.forward;

        SetSorting(atmosphereSystem);
        SetSorting(beltSystem);
        SetSorting(spiralSystem);
        SetSorting(sparkSystem);

        UpdateAtmosphere(center, radius, dt);
        UpdateBelt(center, radius, beltRotation);
        UpdateSpiral(center, radius, dt, beltRotation, beltAxis);
        UpdateSparks(center, radius, dt);
    }

    void UpdateAtmosphere(Vector3 center, float radius, float dt)
    {
        if (atmosphere)
        {
            atmosphereAccumulator += atmosphereRate * dt;
            while (atmosphereAccumulator >= 1f)
            {
                atmosphereAccumulator -= 1f;
                Vector3 p = center + Random.onUnitSphere * radius * (1.02f + Random.value * atmosphereHeight);
                Color c = PickColor(0.5f, 1f);
                c.a = atmosphereOpacity;
                Emit(atmosphereSystem, p, Vector3.zero, atmosphereSize, atmosphereLifetime, c);
            }
        }

        int count = Read(atmosphereSystem);
        Quaternion drift = Quaternion.AngleAxis(atmosphereDrift * dt, Vector3.up);
        for (int i = 0; i < count; i++)
            buffer[i].position = center + drift * (buffer[i].position - center);
        atmosphereSystem.SetParticles(buffer, count);
    }

    void UpdateBelt(Vector3 center, float radius, Quaternion rotation)
    {
        int count = belt ? beltCount : 0;
        if (beltAngles.Length != count) BuildBelt(count);
        if (buffer.Length < count) buffer = new ParticleSystem.Particle[count];

        float time = Time.time;
        float innerRadius = Mathf.Max(0.01f, Mathf.Min(beltRadius.x, beltRadius.y));
        for (int i = 0; i < count; i++)
        {
            float r = beltRadii[i];
            float a = (beltAngles[i] + beltSpeed * time * innerRadius / r) * Mathf.Deg2Rad;
            Vector3 local = new Vector3(Mathf.Cos(a) * r * radius, Mathf.Sin(a) * r * radius, beltHeights[i] * radius);

            buffer[i].position = center + rotation * local;
            buffer[i].velocity = Vector3.zero;
            buffer[i].startSize = beltSizes[i];
            buffer[i].startColor = beltColors[i];
            buffer[i].startLifetime = 1000f;
            buffer[i].remainingLifetime = 1000f;
            buffer[i].rotation = 0f;
        }
        beltSystem.SetParticles(buffer, count);
    }

    void BuildBelt(int count)
    {
        beltAngles = new float[count];
        beltRadii = new float[count];
        beltHeights = new float[count];
        beltSizes = new float[count];
        beltColors = new Color[count];

        float ppu = Mathf.Max(1f, pixelsPerUnit);
        for (int i = 0; i < count; i++)
        {
            beltAngles[i] = Random.Range(0f, 360f);
            beltRadii[i] = Mathf.Max(0.01f, Random.Range(beltRadius.x, beltRadius.y));
            beltHeights[i] = Random.Range(-beltThickness, beltThickness);
            beltSizes[i] = Random.Range(beltSize.x, beltSize.y) / ppu;
            beltColors[i] = PickColor(0f, 0.7f);
        }
    }

    void UpdateSpiral(Vector3 center, float radius, float dt, Quaternion rotation, Vector3 axis)
    {
        if (spiral)
        {
            spiralAccumulator += spiralRate * dt;
            float life = (spiralStartRadius - 1f) / spiralPull + 1f;
            while (spiralAccumulator >= 1f)
            {
                spiralAccumulator -= 1f;
                float a = Random.Range(0f, Mathf.PI * 2f);
                Vector3 local = new Vector3(Mathf.Cos(a), Mathf.Sin(a), 0f) * spiralStartRadius * radius;
                Emit(spiralSystem, center + rotation * local, Vector3.zero, spiralSize, new Vector2(life, life), PickColor(0.5f, 1f));
            }
        }

        int count = Read(spiralSystem);
        for (int i = 0; i < count; i++)
        {
            Vector3 rel = buffer[i].position - center;
            float r = rel.magnitude;
            if (r <= radius)
            {
                buffer[i].remainingLifetime = 0f;
                continue;
            }

            float omega = spiralSpin * Mathf.Clamp(spiralStartRadius * radius / r, 1f, 6f);
            rel = Quaternion.AngleAxis(omega * dt, axis) * rel;
            float newR = Mathf.Max(0f, r - spiralPull * radius * dt);
            buffer[i].position = center + rel / r * newR;
        }
        spiralSystem.SetParticles(buffer, count);
    }

    void UpdateSparks(Vector3 center, float radius, float dt)
    {
        if (sparks && Time.time >= nextSparkTime)
        {
            nextSparkTime = Time.time + Random.Range(sparkInterval.x, sparkInterval.y);

            Vector3 dir = Random.onUnitSphere;
            Camera cam = Camera.main;
            if (cam != null && Vector3.Dot(dir, -cam.transform.forward) < 0f) dir = -dir;

            Color burstColor = PickColor(0.7f, 1f);
            int n = Random.Range(sparkCount.x, sparkCount.y + 1);
            for (int i = 0; i < n; i++)
            {
                Vector3 velocity = (dir + Random.insideUnitSphere * 0.6f).normalized * sparkSpeed * Random.Range(0.5f, 1f);
                Color c = rainbow ? PickColor(0.7f, 1f) : burstColor;
                Emit(sparkSystem, center + dir * radius * 1.01f, velocity, sparkSize, sparkLifetime, c);
            }
        }

        int count = Read(sparkSystem);
        for (int i = 0; i < count; i++)
        {
            Vector3 rel = buffer[i].position - center;
            if (rel.magnitude < radius * 0.98f)
            {
                buffer[i].remainingLifetime = 0f;
                continue;
            }
            buffer[i].velocity -= rel.normalized * sparkGravity * dt;
        }
        sparkSystem.SetParticles(buffer, count);
    }

    Color PickColor(float from, float to)
    {
        if (rainbow) return Color.HSVToRGB(Random.value, 0.75f, 1f);
        if (!PlanetPalette.TryGet(planet, out PlanetPalette.Colors colors)) return Color.white;

        float t = Random.Range(from, to);
        Color c = t < 0.5f
            ? Color.Lerp(colors.dark, colors.mid, t * 2f)
            : Color.Lerp(colors.mid, colors.bright, (t - 0.5f) * 2f);

        Color.RGBToHSV(c, out float h, out float s, out float v);
        return Color.HSVToRGB(h, s * (1f - lighten * 0.3f), Mathf.Lerp(v, 1f, lighten));
    }

    void Emit(ParticleSystem system, Vector3 position, Vector3 velocity, Vector2 sizePixels, Vector2 lifetime, Color color)
    {
        var emit = new ParticleSystem.EmitParams
        {
            position = position,
            velocity = velocity,
            startSize = Random.Range(sizePixels.x, sizePixels.y) / Mathf.Max(1f, pixelsPerUnit),
            startLifetime = Random.Range(lifetime.x, lifetime.y),
            startColor = color,
            rotation = 0f,
            applyShapeToPosition = false
        };
        system.Emit(emit, 1);
    }

    int Read(ParticleSystem system)
    {
        int alive = system.particleCount;
        if (buffer.Length < alive) buffer = new ParticleSystem.Particle[Mathf.NextPowerOfTwo(alive)];
        return system.GetParticles(buffer);
    }

    void SetSorting(ParticleSystem system)
    {
        system.GetComponent<ParticleSystemRenderer>().sortingOrder = sortingOrder;
    }

    ParticleSystem CreateSystem(string systemName, Gradient fade)
    {
        var go = new GameObject(systemName) { hideFlags = HideFlags.DontSave };
        go.transform.SetParent(transform, false);

        var system = go.AddComponent<ParticleSystem>();
        system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        var main = system.main;
        main.loop = true;
        main.playOnAwake = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0f;
        main.maxParticles = 5000;

        var emission = system.emission;
        emission.enabled = false;
        var shapeModule = system.shape;
        shapeModule.enabled = false;

        var colorOverLifetime = system.colorOverLifetime;
        colorOverLifetime.enabled = fade != null;
        if (fade != null) colorOverLifetime.color = fade;

        var psRenderer = go.GetComponent<ParticleSystemRenderer>();
        psRenderer.sharedMaterial = runtimeMaterial;
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        psRenderer.shadowCastingMode = ShadowCastingMode.Off;
        psRenderer.receiveShadows = false;
        psRenderer.sortingOrder = sortingOrder;

        system.Play();
        return system;
    }

    static Gradient FadeInOut() => Fade(0f, 1f, 1f, 0f);
    static Gradient FadeIn() => Fade(0f, 1f, 1f, 1f);
    static Gradient FadeOut() => Fade(1f, 1f, 0.8f, 0f);

    static Gradient Fade(float a0, float a1, float a2, float a3)
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[]
            {
                new GradientAlphaKey(a0, 0f),
                new GradientAlphaKey(a1, 0.15f),
                new GradientAlphaKey(a2, 0.7f),
                new GradientAlphaKey(a3, 1f)
            });
        return gradient;
    }

    void BuildTexture()
    {
        if (sparkleTexture != null) Destroy(sparkleTexture);

        int size = shape == OrbitSparkles.SparkleShape.Plus ? 3 : 1;
        sparkleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = "PlanetParticlesTexture",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            sparkleTexture.SetPixel(x, y, size == 1 || x == 1 || y == 1 ? Color.white : Color.clear);
        sparkleTexture.Apply();

        runtimeMaterial.mainTexture = sparkleTexture;
        cachedShape = shape;
    }

    static void DestroyOwned(ParticleSystem system)
    {
        if (system != null) Destroy(system.gameObject);
    }
}

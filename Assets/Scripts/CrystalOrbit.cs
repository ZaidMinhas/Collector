using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(LineRenderer))]
public class CrystalOrbit : MonoBehaviour
{
    [Header("Orbit Shape")]
    public Transform planet;
    public float radius = 3f;
    [Range(0f, 89f)] public float tilt = 70f;
    [Range(-180f, 180f)] public float angle = -15f;

    [Header("Crystals")]
    public List<Transform> crystals = new List<Transform>();
    public float orbitSpeed = 30f;
    public bool evenSpacing = true;

    [Header("Self Spin")]
    public Vector3 spinAxis = new Vector3(0.3f, 1f, 0.2f);
    public float spinSpeed = 90f;
    public bool randomizeSpin = true;

    [Header("Float")]
    [Min(0f)] public float floatHeight = 0.25f;
    [Min(0f)] public float floatSpeed = 0.4f;

    [Header("Ring")]
    public bool drawRing = true;
    public bool ringAlwaysOnTop = false;
    public int ringSortingOrder = 100;
    [Min(1f)] public float pixelsPerUnit = 32f;
    [Min(1)] public int thicknessPixels = 2;
    public bool snapToPixelGrid = true;
    public bool dashed = true;
    [Min(1)] public int dashPixels = 4;
    [Min(0)] public int gapPixels = 3;

    [Header("Ring Spin")]
    public float ringSpinSpeed = 20f;
    public bool matchCrystalSpeed = false;
    public float tumbleSpeed = 0f;

    [Header("Ring Color (optional)")]
    public bool rainbow = false;
    public bool matchPlanetColor = true;
    public bool useColor = false;
    public Gradient colorGradient = DefaultGradient();
    [Min(0f)] public float intensity = 1f;

    float baseAngle;
    readonly List<float> angleOffsets = new List<float>();
    readonly List<Vector3> spinAxes = new List<Vector3>();
    readonly List<float> spinSpeeds = new List<float>();
    readonly List<float> floatPhases = new List<float>();
    readonly List<float> floatSpeeds = new List<float>();
    float floatTime;

    float ringSpinAngle;
    float tumbleAngle;

    LineRenderer line;
    Material runtimeMaterial;
    Texture2D patternTexture;
    readonly List<Vector3> ringPoints = new List<Vector3>();
    int cachedDash = -1;
    int cachedGap = -1;
    bool cachedDashed;

    Gradient planetGradient;
    PlanetPalette.Colors cachedPlanetColors;

    Vector3 Center => planet != null ? PlanetPalette.Center(planet) : transform.position;

    static Gradient rainbowGradient;
    static Gradient RainbowGradient => rainbowGradient ??= BuildRainbow();

    public bool UsesColor => rainbow || matchPlanetColor || useColor;
    public Gradient RingGradient => rainbow ? RainbowGradient
                                  : matchPlanetColor && UpdatePlanetGradient() ? planetGradient
                                  : colorGradient;

    void Reset()
    {
        planet = transform.parent;
    }

    void OnEnable()
    {
        line = GetComponent<LineRenderer>();
        line.enabled = true;
        cachedDash = -1;
        planetGradient = null;
    }

    void OnValidate()
    {
        planetGradient = null;
    }

    void OnDisable()
    {
        DestroySafe(runtimeMaterial);
        DestroySafe(patternTexture);
        runtimeMaterial = null;
        patternTexture = null;
    }

    void Start()
    {
        if (Application.isPlaying) SetupCrystals();
    }

    void Update()
    {
        if (!Application.isPlaying) return;

        float dt = Time.deltaTime;

        tumbleAngle = Mathf.Repeat(tumbleAngle + tumbleSpeed * dt, 360f);

        float spin = matchCrystalSpeed ? orbitSpeed : ringSpinSpeed;
        ringSpinAngle = Mathf.Repeat(ringSpinAngle + spin * dt, 360f);

        if (angleOffsets.Count != crystals.Count) SetupCrystals();

        baseAngle = Mathf.Repeat(baseAngle + orbitSpeed * dt, 360f);
        floatTime += dt;

        for (int i = 0; i < crystals.Count; i++)
        {
            Transform crystal = crystals[i];
            if (crystal == null) continue;

            crystal.position = GetPoint(baseAngle + angleOffsets[i]) + FloatOffset(i);
            crystal.Rotate(spinAxes[i], spinSpeeds[i] * dt, Space.Self);
        }
    }

    void LateUpdate()
    {
        if (line == null) line = GetComponent<LineRenderer>();

        line.enabled = drawRing;
        if (!drawRing) return;

        UpdatePatternTexture();
        ConfigureLine();
        BuildRingPoints();
    }

    public Vector3 GetPoint(float angleDegrees)
    {
        float rad = angleDegrees * Mathf.Deg2Rad;
        Vector3 flatCircle = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * radius;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle + tumbleAngle) * Quaternion.Euler(tilt, 0f, 0f);
        return Center + rotation * flatCircle;
    }

    Vector3 FloatOffset(int index)
    {
        float wave = Mathf.Sin((floatTime * floatSpeed * floatSpeeds[index] + floatPhases[index]) * Mathf.PI * 2f);
        return Vector3.up * wave * floatHeight;
    }

    void SetupCrystals()
    {
        angleOffsets.Clear();
        spinAxes.Clear();
        spinSpeeds.Clear();
        floatPhases.Clear();
        floatSpeeds.Clear();

        Vector3 fixedAxis = spinAxis.sqrMagnitude > 0.0001f ? spinAxis.normalized : Vector3.up;

        for (int i = 0; i < crystals.Count; i++)
        {
            float offset = evenSpacing && crystals.Count > 0
                ? i * 360f / crystals.Count
                : Random.Range(0f, 360f);
            angleOffsets.Add(offset);

            if (randomizeSpin)
            {
                spinAxes.Add(Random.onUnitSphere);
                spinSpeeds.Add(spinSpeed * Random.Range(0.6f, 1.4f));
                if (crystals[i] != null) crystals[i].rotation = Random.rotation;
            }
            else
            {
                spinAxes.Add(fixedAxis);
                spinSpeeds.Add(spinSpeed);
            }

            floatPhases.Add(Random.value);
            floatSpeeds.Add(Random.Range(0.8f, 1.2f));

            if (crystals[i] != null) crystals[i].position = GetPoint(baseAngle + offset) + FloatOffset(i);
        }
    }

    void UpdatePatternTexture()
    {
        if (patternTexture != null && cachedDash == dashPixels && cachedGap == gapPixels && cachedDashed == dashed)
            return;

        DestroySafe(patternTexture);

        int width = dashed ? dashPixels + gapPixels : 1;
        patternTexture = new Texture2D(width, 1, TextureFormat.RGBA32, false)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Repeat,
            hideFlags = HideFlags.DontSave,
            name = "OrbitRingPattern"
        };

        for (int x = 0; x < width; x++)
        {
            bool isDash = !dashed || x < dashPixels;
            patternTexture.SetPixel(x, 0, isDash ? Color.white : Color.clear);
        }
        patternTexture.Apply();

        cachedDash = dashPixels;
        cachedGap = gapPixels;
        cachedDashed = dashed;
    }

    void ConfigureLine()
    {
        if (runtimeMaterial == null)
        {
            Shader shader = Shader.Find("UI/Default");
            if (shader == null) shader = Shader.Find("Sprites/Default");

            runtimeMaterial = new Material(shader)
            {
                hideFlags = HideFlags.DontSave,
                name = "OrbitRingMaterial"
            };
        }

        Material material = runtimeMaterial;
        material.mainTexture = patternTexture;

        float patternWorldLength = (dashPixels + gapPixels) / pixelsPerUnit;
        material.mainTextureScale = dashed ? new Vector2(1f / patternWorldLength, 1f) : Vector2.one;
        material.mainTextureOffset = Vector2.zero;

        material.renderQueue = ringAlwaysOnTop ? (int)RenderQueue.Overlay : (int)RenderQueue.Transparent;
        material.SetInt("unity_GUIZTestMode", (int)(ringAlwaysOnTop ? CompareFunction.Always : CompareFunction.LessEqual));
        line.sortingOrder = ringSortingOrder;

        if (UsesColor)
        {
            material.color = Color.white * intensity;
            line.colorGradient = RingGradient;
        }
        else
        {
            material.color = Color.white;
            line.startColor = Color.white;
            line.endColor = Color.white;
        }

        line.sharedMaterial = material;
        line.textureMode = dashed ? LineTextureMode.Tile : LineTextureMode.Stretch;
        line.useWorldSpace = true;
        line.loop = true;
        line.numCornerVertices = 0;
        line.numCapVertices = 0;
        line.alignment = LineAlignment.View;
        line.widthMultiplier = thicknessPixels / pixelsPerUnit;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
    }

    void BuildRingPoints()
    {
        ringPoints.Clear();

        float circumference = 2f * Mathf.PI * radius;
        int segments = Mathf.Clamp(Mathf.CeilToInt(circumference * pixelsPerUnit / 2f), 32, 1024);

        for (int i = 0; i < segments; i++)
        {
            Vector3 p = GetPoint(ringSpinAngle + i * 360f / segments);
            if (snapToPixelGrid) p = SnapToPixel(p);

            if (ringPoints.Count == 0 || (p - ringPoints[ringPoints.Count - 1]).sqrMagnitude > 1e-8f)
                ringPoints.Add(p);
        }

        if (ringPoints.Count > 1 && (ringPoints[0] - ringPoints[ringPoints.Count - 1]).sqrMagnitude <= 1e-8f)
            ringPoints.RemoveAt(ringPoints.Count - 1);

        line.positionCount = ringPoints.Count;
        line.SetPositions(ringPoints.ToArray());
    }

    Vector3 SnapToPixel(Vector3 p)
    {
        p.x = Mathf.Round(p.x * pixelsPerUnit) / pixelsPerUnit;
        p.y = Mathf.Round(p.y * pixelsPerUnit) / pixelsPerUnit;
        return p;
    }

    bool UpdatePlanetGradient()
    {
        if (!PlanetPalette.TryGet(planet, out PlanetPalette.Colors colors)) return false;
        if (planetGradient != null && colors.Equals(cachedPlanetColors)) return true;

        planetGradient = new Gradient();
        planetGradient.SetKeys(
            new[]
            {
                new GradientColorKey(colors.bright, 0f),
                new GradientColorKey(colors.mid, 0.25f),
                new GradientColorKey(colors.dark, 0.5f),
                new GradientColorKey(colors.mid, 0.75f),
                new GradientColorKey(colors.bright, 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });

        cachedPlanetColors = colors;
        return true;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        const int segments = 64;
        Vector3 previous = GetPoint(0f);

        for (int i = 1; i <= segments; i++)
        {
            Vector3 next = GetPoint(i * 360f / segments);
            Gizmos.DrawLine(previous, next);
            previous = next;
        }
    }

    static Gradient BuildRainbow()
    {
        var keys = new GradientColorKey[8];
        for (int i = 0; i < keys.Length; i++)
        {
            float t = i / (keys.Length - 1f);
            keys[i] = new GradientColorKey(Color.HSVToRGB(t % 1f, 0.85f, 1f), t);
        }

        var gradient = new Gradient();
        gradient.SetKeys(keys, new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        return gradient;
    }

    static Gradient DefaultGradient()
    {
        var gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(0.24f, 1f, 0.68f), 0f),
                new GradientColorKey(new Color(0.95f, 0.53f, 1f), 0.5f),
                new GradientColorKey(new Color(0.24f, 1f, 0.68f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            });
        return gradient;
    }

    static void DestroySafe(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }
}

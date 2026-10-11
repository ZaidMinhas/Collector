using UnityEngine;
using UnityEngine.Rendering;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class PlanetGlow : MonoBehaviour
{
    [Header("Source")]
    public Transform planet;

    [Header("Shape")]
    [Min(1.01f)] public float size = 1.6f;
    [Min(0.1f)] public float falloff = 1.5f;
    [Range(1, 16)] public int bands = 5;
    [Min(1f)] public float pixelsPerUnit = 20f;

    [Header("Color")]
    public bool rainbow = false;
    public float rainbowSpeed = 0.05f;
    public float rainbowTwist = 0.5f;
    [Range(0f, 1f)] public float rainbowSaturation = 0.8f;
    public bool matchPlanetColor = true;
    public Color innerColor = new Color(1f, 0.35f, 0.65f);
    public Color outerColor = new Color(0.6f, 0.1f, 0.4f);
    [Range(0f, 1f)] public float opacity = 0.25f;
    [Min(0f)] public float intensity = 1.2f;
    public int sortingOrder = -10;

    [Header("Pulse")]
    [Range(0f, 1f)] public float pulseAmount = 0.2f;
    [Min(0f)] public float pulseSpeed = 0.25f;

    [Header("Ripples")]
    [Min(0f)] public float rippleCount = 3f;
    public float rippleSpeed = 0.3f;
    [Range(0f, 1f)] public float rippleStrength = 0.35f;

    [Header("Wobble")]
    [Range(1, 12)] public int wobbleCount = 5;
    public float wobbleSpeed = 0.15f;
    [Range(0f, 0.5f)] public float wobbleAmount = 0.08f;

    [Header("Rays")]
    [Range(0, 16)] public int rayCount = 6;
    public float raySpeed = 0.03f;
    [Range(0f, 1f)] public float rayStrength = 0.3f;

    [SerializeField, HideInInspector] Shader glowShader;

    MeshRenderer meshRenderer;
    Mesh quad;
    Material runtimeMaterial;

    static readonly int InnerColorId = Shader.PropertyToID("_InnerColor");
    static readonly int OuterColorId = Shader.PropertyToID("_OuterColor");
    static readonly int PlanetEdgeId = Shader.PropertyToID("_PlanetEdge");
    static readonly int FalloffId = Shader.PropertyToID("_Falloff");
    static readonly int BandsId = Shader.PropertyToID("_Bands");
    static readonly int PixelsId = Shader.PropertyToID("_Pixels");
    static readonly int OpacityId = Shader.PropertyToID("_Opacity");
    static readonly int IntensityId = Shader.PropertyToID("_Intensity");
    static readonly int RippleCountId = Shader.PropertyToID("_RippleCount");
    static readonly int RippleSpeedId = Shader.PropertyToID("_RippleSpeed");
    static readonly int RippleStrengthId = Shader.PropertyToID("_RippleStrength");
    static readonly int WobbleCountId = Shader.PropertyToID("_WobbleCount");
    static readonly int WobbleSpeedId = Shader.PropertyToID("_WobbleSpeed");
    static readonly int WobbleAmountId = Shader.PropertyToID("_WobbleAmount");
    static readonly int RayCountId = Shader.PropertyToID("_RayCount");
    static readonly int RaySpeedId = Shader.PropertyToID("_RaySpeed");
    static readonly int RayStrengthId = Shader.PropertyToID("_RayStrength");
    static readonly int RainbowId = Shader.PropertyToID("_Rainbow");
    static readonly int RainbowSpeedId = Shader.PropertyToID("_RainbowSpeed");
    static readonly int RainbowTwistId = Shader.PropertyToID("_RainbowTwist");
    static readonly int RainbowSaturationId = Shader.PropertyToID("_RainbowSaturation");

    void Reset()
    {
        planet = transform.parent;
        glowShader = Shader.Find("Collector/PlanetGlow");
    }

    void OnValidate()
    {
        if (glowShader == null) glowShader = Shader.Find("Collector/PlanetGlow");
    }

    void OnEnable()
    {
        meshRenderer = GetComponent<MeshRenderer>();

        quad = new Mesh { name = "PlanetGlowQuad", hideFlags = HideFlags.DontSave };
        quad.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f)
        };
        quad.uv = new[] { new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f) };
        quad.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        quad.RecalculateBounds();
        GetComponent<MeshFilter>().sharedMesh = quad;

        Shader shader = glowShader != null ? glowShader : Shader.Find("Collector/PlanetGlow");
        if (shader == null) return;

        runtimeMaterial = new Material(shader)
        {
            name = "PlanetGlowMaterial",
            hideFlags = HideFlags.DontSave
        };
        meshRenderer.sharedMaterial = runtimeMaterial;
        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;
    }

    void OnDisable()
    {
        DestroySafe(quad);
        DestroySafe(runtimeMaterial);
        quad = null;
        runtimeMaterial = null;
    }

    void LateUpdate()
    {
        if (planet == null) planet = transform.parent;
        if (planet == null || runtimeMaterial == null) return;

        float glowRadius = PlanetPalette.Radius(planet) * size;
        float wave = Application.isPlaying ? Mathf.Sin(Time.time * pulseSpeed * Mathf.PI * 2f) : 0f;

        Camera cam = Camera.main;
        transform.position = PlanetPalette.Center(planet);
        if (cam != null) transform.rotation = cam.transform.rotation;

        float diameter = glowRadius * 2f * (1f + wave * pulseAmount * 0.05f);
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        transform.localScale = new Vector3(
            diameter / Mathf.Max(0.0001f, Mathf.Abs(parentScale.x)),
            diameter / Mathf.Max(0.0001f, Mathf.Abs(parentScale.y)),
            diameter / Mathf.Max(0.0001f, Mathf.Abs(parentScale.z)));

        Color inner = innerColor;
        Color outer = outerColor;
        if (matchPlanetColor && PlanetPalette.TryGet(planet, out PlanetPalette.Colors colors))
        {
            inner = colors.bright;
            outer = colors.mid;
        }

        Material m = runtimeMaterial;
        m.SetColor(InnerColorId, inner);
        m.SetColor(OuterColorId, outer);
        m.SetFloat(PlanetEdgeId, 1f / size);
        m.SetFloat(FalloffId, falloff);
        m.SetFloat(BandsId, bands);
        m.SetFloat(PixelsId, Mathf.Max(8f, Mathf.Round(glowRadius * 2f * pixelsPerUnit)));
        m.SetFloat(OpacityId, Mathf.Clamp01(opacity * (1f + wave * pulseAmount)));
        m.SetFloat(IntensityId, intensity);
        m.SetFloat(RippleCountId, rippleCount);
        m.SetFloat(RippleSpeedId, rippleSpeed);
        m.SetFloat(RippleStrengthId, rippleStrength);
        m.SetFloat(WobbleCountId, wobbleCount);
        m.SetFloat(WobbleSpeedId, wobbleSpeed);
        m.SetFloat(WobbleAmountId, wobbleAmount);
        m.SetFloat(RayCountId, rayCount);
        m.SetFloat(RaySpeedId, raySpeed);
        m.SetFloat(RayStrengthId, rayStrength);
        m.SetFloat(RainbowId, rainbow ? 1f : 0f);
        m.SetFloat(RainbowSpeedId, rainbowSpeed);
        m.SetFloat(RainbowTwistId, rainbowTwist);
        m.SetFloat(RainbowSaturationId, rainbowSaturation);

        meshRenderer.sortingOrder = sortingOrder;
    }

    static void DestroySafe(Object obj)
    {
        if (obj == null) return;
        if (Application.isPlaying) Destroy(obj);
        else DestroyImmediate(obj);
    }
}

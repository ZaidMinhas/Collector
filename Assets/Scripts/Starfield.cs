using UnityEngine;
using UnityEngine.Rendering;

public class Starfield : MonoBehaviour
{
    [System.Serializable]
    public class StarLayer
    {
        public string name = "Stars";
        [Min(0)] public int count = 100;
        public Vector2 sizePixels = new Vector2(1f, 1f);
        [Range(0f, 1f)] public float parallax = 0.05f;
        [Min(0f)] public float brightness = 1f;
        public bool plusShape = false;
    }

    [Header("Camera")]
    public Camera targetCamera;
    [Min(0.1f)] public float distanceFromCamera = 9f;

    [Header("Look")]
    [Min(1f)] public float pixelsPerUnit = 20f;
    [Range(0f, 1f)] public float colorfulness = 0.3f;
    public string sortingLayer = "Background";
    public int sortingOrder = 1;

    [Header("Twinkle")]
    public Vector2 twinkleSpeed = new Vector2(0.2f, 1.2f);
    [Range(0f, 1f)] public float twinkleDepth = 0.7f;

    [Header("Layers")]
    public StarLayer[] layers =
    {
        new StarLayer { name = "Far", count = 160, sizePixels = new Vector2(1f, 1f), parallax = 0.02f, brightness = 0.6f },
        new StarLayer { name = "Mid", count = 60, sizePixels = new Vector2(1f, 2f), parallax = 0.08f, brightness = 0.9f },
        new StarLayer { name = "Near", count = 12, sizePixels = new Vector2(3f, 3f), parallax = 0.2f, brightness = 1.6f, plusShape = true }
    };

    class LayerState
    {
        public ParticleSystem system;
        public Material material;
        public Vector2[] positions = new Vector2[0];
        public float[] sizes;
        public Color[] colors;
        public float[] phases;
        public float[] speeds;
        public ParticleSystem.Particle[] particles = new ParticleSystem.Particle[0];
    }

    static readonly Color[] Tints =
    {
        new Color(0.7f, 0.8f, 1f),
        new Color(1f, 0.9f, 0.7f),
        new Color(1f, 0.75f, 0.9f),
        new Color(0.75f, 1f, 0.95f)
    };

    LayerState[] states = new LayerState[0];
    Texture2D squareTexture;
    Texture2D plusTexture;
    bool dirty = true;

    void OnValidate()
    {
        dirty = true;
    }

    void OnEnable()
    {
        squareTexture = BuildTexture(false);
        plusTexture = BuildTexture(true);
        dirty = true;
    }

    void OnDisable()
    {
        ClearStates();
        Destroy(squareTexture);
        Destroy(plusTexture);
    }

    void LateUpdate()
    {
        Camera cam = targetCamera != null ? targetCamera : Camera.main;
        if (cam == null) return;

        if (dirty || states.Length != layers.Length) RebuildStates();

        Transform camTransform = cam.transform;
        Vector2 view = ViewSize(cam) * 1.1f;
        Vector3 right = camTransform.right;
        Vector3 up = camTransform.up;
        Vector3 origin = camTransform.position + camTransform.forward * distanceFromCamera;
        float camX = Vector3.Dot(camTransform.position, right);
        float camY = Vector3.Dot(camTransform.position, up);
        float time = Time.time;

        for (int l = 0; l < layers.Length; l++)
        {
            StarLayer layer = layers[l];
            LayerState state = states[l];

            state.material.color = Color.white * layer.brightness;
            var psRenderer = state.system.GetComponent<ParticleSystemRenderer>();
            psRenderer.sortingLayerName = sortingLayer;
            psRenderer.sortingOrder = sortingOrder;

            float shiftX = camX * layer.parallax / view.x;
            float shiftY = camY * layer.parallax / view.y;

            for (int i = 0; i < state.positions.Length; i++)
            {
                float u = Mathf.Repeat(state.positions[i].x - shiftX, 1f) - 0.5f;
                float v = Mathf.Repeat(state.positions[i].y - shiftY, 1f) - 0.5f;

                float wave = 0.5f + 0.5f * Mathf.Sin(time * state.speeds[i] * Mathf.PI * 2f + state.phases[i]);
                wave *= wave;

                Color c = state.colors[i];
                c.a = 1f - twinkleDepth * (1f - wave);

                float size = state.sizes[i];
                if (layer.plusShape) size *= 0.6f + 0.4f * wave;

                ref ParticleSystem.Particle p = ref state.particles[i];
                p.position = origin + right * (u * view.x) + up * (v * view.y);
                p.velocity = Vector3.zero;
                p.startSize = size;
                p.startColor = c;
                p.rotation = 0f;
                p.startLifetime = 1000f;
                p.remainingLifetime = 1000f;
            }

            state.system.SetParticles(state.particles, state.positions.Length);
        }
    }

    Vector2 ViewSize(Camera cam)
    {
        float height = cam.orthographic
            ? cam.orthographicSize * 2f
            : 2f * distanceFromCamera * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        return new Vector2(height * cam.aspect, height);
    }

    void RebuildStates()
    {
        ClearStates();
        states = new LayerState[layers.Length];

        float ppu = Mathf.Max(1f, pixelsPerUnit);
        for (int l = 0; l < layers.Length; l++)
        {
            StarLayer layer = layers[l];
            var state = new LayerState();
            int count = Mathf.Max(0, layer.count);

            state.positions = new Vector2[count];
            state.sizes = new float[count];
            state.colors = new Color[count];
            state.phases = new float[count];
            state.speeds = new float[count];
            state.particles = new ParticleSystem.Particle[count];

            for (int i = 0; i < count; i++)
            {
                state.positions[i] = new Vector2(Random.value, Random.value);
                state.sizes[i] = Mathf.Round(Random.Range(layer.sizePixels.x, layer.sizePixels.y)) / ppu;
                state.colors[i] = Random.value < colorfulness ? Tints[Random.Range(0, Tints.Length)] : Color.white;
                state.phases[i] = Random.Range(0f, Mathf.PI * 2f);
                state.speeds[i] = Random.Range(twinkleSpeed.x, twinkleSpeed.y);
            }

            state.material = new Material(Shader.Find("Sprites/Default"))
            {
                name = "StarfieldMaterial",
                hideFlags = HideFlags.DontSave,
                mainTexture = layer.plusShape ? plusTexture : squareTexture
            };
            state.system = CreateSystem(layer.name, state.material);
            states[l] = state;
        }

        dirty = false;
    }

    ParticleSystem CreateSystem(string systemName, Material material)
    {
        var go = new GameObject("Starfield " + systemName) { hideFlags = HideFlags.DontSave };
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
        var shape = system.shape;
        shape.enabled = false;

        var psRenderer = go.GetComponent<ParticleSystemRenderer>();
        psRenderer.sharedMaterial = material;
        psRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        psRenderer.shadowCastingMode = ShadowCastingMode.Off;
        psRenderer.receiveShadows = false;
        psRenderer.sortingLayerName = sortingLayer;
        psRenderer.sortingOrder = sortingOrder;

        system.Play();
        return system;
    }

    void ClearStates()
    {
        foreach (LayerState state in states)
        {
            if (state == null) continue;
            if (state.system != null) Destroy(state.system.gameObject);
            if (state.material != null) Destroy(state.material);
        }
        states = new LayerState[0];
    }

    static Texture2D BuildTexture(bool plus)
    {
        int size = plus ? 3 : 1;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = plus ? "StarPlus" : "StarSquare",
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };

        for (int y = 0; y < size; y++)
        for (int x = 0; x < size; x++)
            texture.SetPixel(x, y, !plus || x == 1 || y == 1 ? Color.white : Color.clear);
        texture.Apply();
        return texture;
    }
}

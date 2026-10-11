using System.Collections.Generic;
using UnityEngine;

public static class PlanetPalette
{
    public struct Colors
    {
        public Color dark;
        public Color mid;
        public Color bright;
    }

    static readonly Dictionary<(Texture, Color), Colors> cache = new Dictionary<(Texture, Color), Colors>();

    public static bool TryGet(Transform planet, out Colors colors)
    {
        colors = default;
        if (planet == null) return false;

        Renderer planetRenderer = planet.GetComponent<Renderer>();
        if (planetRenderer == null) planetRenderer = planet.GetComponentInChildren<Renderer>();
        if (planetRenderer == null || planetRenderer.sharedMaterial == null) return false;

        Material material = planetRenderer.sharedMaterial;
        Texture texture = material.HasProperty("_BaseMap") ? material.GetTexture("_BaseMap")
                        : material.HasProperty("_MainTex") ? material.GetTexture("_MainTex")
                        : null;
        Color tint = material.HasProperty("_BaseColor") ? material.GetColor("_BaseColor")
                   : material.HasProperty("_Color") ? material.GetColor("_Color")
                   : Color.white;

        if (cache.TryGetValue((texture, tint), out colors)) return true;

        List<Color> samples = SampleTexture(texture, tint);
        samples.Sort((a, b) => Luminance(a).CompareTo(Luminance(b)));

        Color Pick(float percentile) => samples[Mathf.RoundToInt((samples.Count - 1) * percentile)];
        colors = new Colors { dark = Pick(0.3f), mid = Pick(0.6f), bright = Pick(0.95f) };

        cache[(texture, tint)] = colors;
        return true;
    }

    static List<Color> SampleTexture(Texture texture, Color tint)
    {
        var colors = new List<Color>();

        if (texture != null)
        {
            const int size = 32;
            RenderTexture rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
            RenderTexture previous = RenderTexture.active;
            Graphics.Blit(texture, rt);
            RenderTexture.active = rt;

            var readable = new Texture2D(size, size, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, size, size), 0, 0);
            readable.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(rt);

            foreach (Color c in readable.GetPixels())
                if (c.a > 0.5f) colors.Add(c * tint);

            if (Application.isPlaying) Object.Destroy(readable);
            else Object.DestroyImmediate(readable);
        }

        if (colors.Count == 0) colors.Add(tint);
        return colors;
    }

    static float Luminance(Color c) => 0.299f * c.r + 0.587f * c.g + 0.114f * c.b;
}

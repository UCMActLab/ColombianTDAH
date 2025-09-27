using UnityEngine;
using UnityEditor;
using System.IO;

public static class SoftCookieGenerator
{
    const int W = 1024, H = 1024;

    [MenuItem("Tools/Lighting/Cookies/01 Ventana Suave")]
    static void MakeSoftWindow() => SaveCookie(GenSoftWindow(margin: 100, bar: 56, feather: 48, crossGray: 0.75f));

    [MenuItem("Tools/Lighting/Cookies/02 Cortina (Sheer)")]
    static void MakeSheer() => SaveCookie(GenSheerCurtain(softness: 0.35f, vGradientBias: 0.15f, noise: 0.015f));

    [MenuItem("Tools/Lighting/Cookies/03 Estores (Venecianas)")]
    static void MakeBlinds() => SaveCookie(GenBlinds(slats: 12, gap: 18, blur: 6, minGray: 0.55f, maxGray: 1.0f));

    static void SaveCookie(Texture2D tex)
    {
        string dir = "Assets/Lighting/Cookies";
        Directory.CreateDirectory(dir);
        string path = Path.Combine(dir, tex.name + ".png");
        File.WriteAllBytes(path, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(path);
        var ti = (TextureImporter)AssetImporter.GetAtPath(path);
        ti.sRGBTexture = false;                 // ¡Clave! Las cookies son lineales
        ti.alphaIsTransparency = false;
        ti.mipmapEnabled = true;
        ti.wrapMode = TextureWrapMode.Clamp;
        ti.filterMode = FilterMode.Trilinear;
        ti.textureCompression = TextureImporterCompression.Compressed; // Si ves banding, ponla en None
        AssetDatabase.WriteImportSettingsIfDirty(path);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Cookie creada", $"Guardado:\n{path}", "OK");
    }

    // --- 01: Ventana con marco + cruz suaves y en gris ---
    static Texture2D GenSoftWindow(int margin, int bar, int feather, float crossGray)
    {
        var tex = NewTex("cookie_window_soft");
        var px = Fill(tex, 0f); // fondo negro (bloquea)
        int x0 = margin, x1 = W - margin, y0 = margin, y1 = H - margin;

        // Relleno interior blanco
        FillRect(px, x0, y0, x1, y1, 1f);

        // Feather (bordes difuminados hacia el negro exterior)
        FeatherRect(px, x0, y0, x1, y1, feather);

        // Cruz en gris (no negro) para que no “corte” en seco
        int cx = W / 2, cy = H / 2;
        FillRect(px, cx - bar / 2, y0, cx + bar / 2, y1, crossGray);
        FillRect(px, x0, cy - bar / 2, x1, cy + bar / 2, crossGray);
        FeatherRect(px, cx - bar / 2, y0, cx + bar / 2, y1, feather / 2);
        FeatherRect(px, x0, cy - bar / 2, x1, cy + bar / 2, feather / 2);

        // Un pelín de ruido para romper banding
        AddNoise(px, 0.010f);

        tex.SetPixels(pxToColors(px));
        tex.Apply(true, false);
        return tex;
    }

    // --- 02: Cortina translúcida (gradiente vertical + ruido fino) ---
    static Texture2D GenSheerCurtain(float softness, float vGradientBias, float noise)
    {
        var tex = NewTex("cookie_sheer");
        var px = Fill(tex, 0f); // negro

        // Ventana sin cruz, marco suave
        int margin = 90, feather = 64;
        int x0 = margin, x1 = W - margin, y0 = margin, y1 = H - margin;
        FillRect(px, x0, y0, x1, y1, 1f);
        FeatherRect(px, x0, y0, x1, y1, feather);

        // Cortina: gradiente vertical multiplicativo (parte superior deja pasar menos)
        for (int y = y0; y < y1; y++)
        {
            float v = (float)(y - y0) / (y1 - y0);
            float curtain = Mathf.Lerp(1f - softness - vGradientBias, 1f, v); // de gris a blanco
            for (int x = x0; x < x1; x++)
            {
                int i = y * W + x;
                px[i] *= Mathf.Clamp01(curtain);
            }
        }

        AddNoise(px, noise);
        tex.SetPixels(pxToColors(px));
        tex.Apply(true, false);
        return tex;
    }

    // --- 03: Estores (venecianas) en grises + blur leve ---
    static Texture2D GenBlinds(int slats, int gap, int blur, float minGray, float maxGray)
    {
        var tex = NewTex("cookie_blinds");
        var px = Fill(tex, 0f);
        int margin = 90;
        int x0 = margin, x1 = W - margin, y0 = margin, y1 = H - margin;

        // Área de ventana base blanca
        FillRect(px, x0, y0, x1, y1, 1f);

        int total = slats * 2;
        for (int s = 0; s < total; s++)
        {
            float t = s / (float)(total - 1);
            float y = Mathf.Lerp(y0, y1, t);
            int yStart = Mathf.RoundToInt(y);
            int yEnd = Mathf.Min(yStart + gap, y1);
            float g = Mathf.Lerp(minGray, maxGray, Mathf.PingPong(t * 2f, 1f));
            FillRect(px, x0, yStart, x1, yEnd, g);
        }

        // Suavizado vertical simple
        BoxBlurVertical(px, iterations: blur);

        // Feather en el marco
        FeatherRect(px, x0, y0, x1, y1, 48);

        AddNoise(px, 0.008f);
        tex.SetPixels(pxToColors(px));
        tex.Apply(true, false);
        return tex;
    }

    // ========= helpers =========
    static Texture2D NewTex(string name) => new Texture2D(W, H, TextureFormat.RGBA32, true, true) { name = name };
    static float[] Fill(Texture2D t, float v) { var a = new float[W * H]; for (int i = 0; i < a.Length; i++) a[i] = v; return a; }
    static void FillRect(float[] a, int x0, int y0, int x1, int y1, float v)
    {
        x0 = Mathf.Clamp(x0, 0, W); x1 = Mathf.Clamp(x1, 0, W);
        y0 = Mathf.Clamp(y0, 0, H); y1 = Mathf.Clamp(y1, 0, H);
        for (int y = y0; y < y1; y++) { int row = y * W; for (int x = x0; x < x1; x++) a[row + x] = Mathf.Clamp01(v); }
    }
    static void FeatherRect(float[] a, int x0, int y0, int x1, int y1, int feather)
    {
        feather = Mathf.Max(1, feather);
        for (int f = 0; f < feather; f++)
        {
            float k = (f + 1f) / (feather + 1f);         // 0..1
            float v = Mathf.Lerp(1f, 0f, k);             // blanco->negro
            // arriba/abajo
            for (int x = x0; x < x1; x++)
            {
                a[(y0 + f) * W + x] = Mathf.Min(a[(y0 + f) * W + x], v);
                a[(y1 - 1 - f) * W + x] = Mathf.Min(a[(y1 - 1 - f) * W + x], v);
            }
            // izquierda/derecha
            for (int y = y0; y < y1; y++)
            {
                a[y * W + (x0 + f)] = Mathf.Min(a[y * W + (x0 + f)], v);
                a[y * W + (x1 - 1 - f)] = Mathf.Min(a[y * W + (x1 - 1 - f)], v);
            }
        }
    }
    static void AddNoise(float[] a, float amp)
    {
        var r = new System.Random(1234);
        for (int i = 0; i < a.Length; i++)
        {
            float n = (float)r.NextDouble() * 2f - 1f;    // -1..1
            a[i] = Mathf.Clamp01(a[i] + n * amp);
        }
    }
    static void BoxBlurVertical(float[] a, int iterations)
    {
        iterations = Mathf.Max(0, iterations);
        if (iterations == 0) return;
        float[] tmp = new float[a.Length];
        for (int it = 0; it < iterations; it++)
        {
            for (int x = 0; x < W; x++)
            {
                float acc = 0f;
                for (int y = 0; y < H; y++)
                {
                    acc = (acc * 2f + a[y * W + x]) / 3f; // filtro simple
                    tmp[y * W + x] = acc;
                }
            }
            // copia de vuelta
            for (int i = 0; i < a.Length; i++) a[i] = tmp[i];
        }
    }
    static Color[] pxToColors(float[] a) { var c = new Color[a.Length]; for (int i = 0; i < a.Length; i++) c[i] = new Color(a[i], a[i], a[i], 1f); return c; }
}

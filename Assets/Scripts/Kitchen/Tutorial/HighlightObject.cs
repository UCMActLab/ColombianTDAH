using System.Collections.Generic;
using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    [Header("Params")]
    private Color glowColor;
    private float minIntensity;
    private float maxIntensity;
    private float frequency;

    [Tooltip("Si un material no tiene _EmissionColor, intenta pulso sobre color base.")]
    [SerializeField] private bool fallbackToBaseColor = true;

    // Prop IDs
    private static readonly int ID_EmissionColor = Shader.PropertyToID("_EmissionColor");
    private static readonly int ID_Color = Shader.PropertyToID("_Color");
    private static readonly int ID_BaseColor = Shader.PropertyToID("_BaseColor");

    private readonly List<Renderer> rends = new();
    private struct MatBackup
    {
        public bool hasEmission;
        public bool hadEmissionKeyword;
        public Color origEmissionColor;

        public bool hasBaseColor;
        public bool baseUses_BaseColor; // true=>_BaseColor, false=>_Color
        public Color origBaseColor;
    }
    private readonly List<MatBackup[]> backups = new();

    private float lastT = -1f;

    // MPBs por renderer y por submaterial
    private readonly List<MaterialPropertyBlock[]> mpbPerRenderer = new();

    public void Configure(Color color, float min, float max, float freq)
    {
        glowColor = color;
        minIntensity = min;
        maxIntensity = max;
        frequency = Mathf.Max(0.01f, freq);
    }
    private static void EnsureEmissionKeyword(Material m)
    {
        if (!m || !m.HasProperty(ID_EmissionColor)) return;

        // Asegura un color no negro (algunos shaders ignoran emisión si es 0)
        var c = m.GetColor(ID_EmissionColor);
        if (c.maxColorComponent <= 0f)
            m.SetColor(ID_EmissionColor, new Color(0.001f, 0.001f, 0.001f));

        // Activa keyword en el sharedMaterial (importante con SRP Batcher)
        m.EnableKeyword("_EMISSION");
    }
    void OnEnable()
    {
        // Recolecta todos los renderers (MeshRenderer, SkinnedMeshRenderer, SpriteRenderer…)
        GetComponentsInChildren(true, rends);

        backups.Clear();
        mpbPerRenderer.Clear();

        foreach (var r in rends)
        {
            if (r == null) { backups.Add(null); mpbPerRenderer.Add(null); continue; }

            var mats = r.sharedMaterials;
            var infos = new MatBackup[mats.Length];
            var mpbs = new MaterialPropertyBlock[mats.Length];

            for (int i = 0; i < mats.Length; i++)
            {
                var sm = mats[i];
                var info = new MatBackup();
                mpbs[i] = new MaterialPropertyBlock();

                if (sm == null)
                {
                    infos[i] = info;
                    continue;
                }

                EnsureEmissionKeyword(sm);

                // 1) Emission
                if (sm.HasProperty(ID_EmissionColor))
                {
                    info.hasEmission = true;
                    info.origEmissionColor = sm.GetColor(ID_EmissionColor);
                    info.hadEmissionKeyword = sm.IsKeywordEnabled("_EMISSION");

                    // Intentamos habilitar keyword una vez en el sharedMaterial
                    //sm.EnableKeyword("_EMISSION");
                }
                else
                {
                    info.hasEmission = false;
                }

                // 2) Base color 
                if (fallbackToBaseColor)
                {
                    if (sm.HasProperty(ID_BaseColor))
                    {
                        info.hasBaseColor = true;
                        info.baseUses_BaseColor = true;
                        info.origBaseColor = sm.GetColor(ID_BaseColor);
                    }
                    else if (sm.HasProperty(ID_Color))
                    {
                        info.hasBaseColor = true;
                        info.baseUses_BaseColor = false;
                        info.origBaseColor = sm.GetColor(ID_Color);
                    }
                    else
                    {
                        info.hasBaseColor = false;
                    }
                }

                infos[i] = info;
            }

            backups.Add(infos);
            mpbPerRenderer.Add(mpbs);
        }

        // Forzamos primer frame distinto
        lastT = -1f;
    }

    void Update()
    {
        if (rends.Count == 0) return;

        // Pulso
        float t = 0.5f + 0.5f * Mathf.Sin(Time.time * Mathf.PI * 2f * frequency);
        if (Mathf.Approximately(t, lastT)) return;
        lastT = t;

        float intensity = Mathf.Lerp(minIntensity, maxIntensity, t);
        Color emission = glowColor * intensity;
        Color tintBase = glowColor * (intensity * 0.25f); 

        // Aplicamos por submaterial usando MPB
        for (int rIdx = 0; rIdx < rends.Count; rIdx++)
        {
            var r = rends[rIdx];
            if (r == null) continue;

            var infos = backups[rIdx];
            var mpbs = mpbPerRenderer[rIdx];
            if (infos == null || mpbs == null) continue;

            int subCount = infos.Length;
            for (int i = 0; i < subCount; i++)
            {
                var info = infos[i];
                var mpb = mpbs[i];
                mpb.Clear();

                if (info.hasEmission)
                {
                    // Escribimos emisión en el MPB
                    mpb.SetColor(ID_EmissionColor, emission);
                }

                if (info.hasBaseColor)
                {
                    Color baseOrig = info.origBaseColor + tintBase;
                    if (info.baseUses_BaseColor)
                        mpb.SetColor(ID_BaseColor, baseOrig);
                    else
                        mpb.SetColor(ID_Color, baseOrig);
                }

                // Aplica el MPB al submaterial i
                r.SetPropertyBlock(mpb, i);
            }
        }
    }

    void OnDisable()
    {
        // Restauramos el estado original usando MPB vacío o con valores originales
        for (int rIdx = 0; rIdx < rends.Count; rIdx++)
        {
            var r = rends[rIdx];
            if (r == null) continue;

            var infos = backups[rIdx];
            var mpbs = mpbPerRenderer[rIdx];
            if (infos == null || mpbs == null) continue;

            int subCount = infos.Length;
            for (int i = 0; i < subCount; i++)
            {
                var info = infos[i];
                var mpb = mpbs[i];
                mpb.Clear();

                // Restauramos valores originales por MPB (sin tocar materiales)
                if (info.hasEmission)
                {
                    mpb.SetColor(ID_EmissionColor, info.origEmissionColor);
                }
                if (info.hasBaseColor)
                {
                    if (info.baseUses_BaseColor)
                        mpb.SetColor(ID_BaseColor, info.origBaseColor);
                    else
                        mpb.SetColor(ID_Color, info.origBaseColor);
                }

                r.SetPropertyBlock(mpb, i);

                var mats = r.sharedMaterials;
                if (i < mats.Length && mats[i] != null && info.hasEmission)
                {
                    if (!info.hadEmissionKeyword) mats[i].DisableKeyword("_EMISSION");
                }
            }
        }

        rends.Clear();
        backups.Clear();
        mpbPerRenderer.Clear();
    }
}

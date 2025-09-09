using System.Collections.Generic;
using UnityEngine;

public class HighlightObject : MonoBehaviour
{
    [Header("Params")]
    private Color glowColor;
    private float minIntensity;   
    private float maxIntensity;
    private float frequency;

    [Tooltip("Si un material no tiene _EmissionColor, intenta pulso sobre _Color.")]
    [SerializeField] private bool fallbackToBaseColor = true;

    private readonly List<Renderer> rends = new();
    private struct MatBackup
    {
        public bool hasEmission;
        public bool origEmissionKeyword;
        public Color origEmissionColor;
        public bool hasBaseColor;
        public Color origBaseColor;
    }
    private readonly List<MatBackup[]> backups = new();

    private float lastT = -1f;
    private Color lastEmission;

    public void Configure(Color color, float min, float max, float freq)
    {
        glowColor = color;
        minIntensity = min;
        maxIntensity = max;
        frequency = Mathf.Max(0.01f, freq);
    }

    void OnEnable()
    {
        // Recolecta todos los renderers
        GetComponentsInChildren(true, rends);

        backups.Clear();
        foreach (var r in rends)
        {
            if (r == null) { backups.Add(null); continue; }
 
            var mats = r.materials;
            var matInfos = new MatBackup[mats.Length];

            for (int i = 0; i < mats.Length; i++)
            {
                var m = mats[i];
                var info = new MatBackup();

                if (m == null)
                {
                    matInfos[i] = info;
                    continue;
                }

                // Emisión
                if (m.HasProperty("_EmissionColor"))
                {
                    info.hasEmission = true;
                    info.origEmissionColor = m.GetColor("_EmissionColor");
                    info.origEmissionKeyword = m.IsKeywordEnabled("_EMISSION");

                    // Aseguramos que la emisión esté activa para que sea visible
                    m.EnableKeyword("_EMISSION");
                }
                else
                {
                    info.hasEmission = false;
                }

                // Color base (fallback)
                if (fallbackToBaseColor && m.HasProperty("_Color"))
                {
                    info.hasBaseColor = true;
                    info.origBaseColor = m.GetColor("_Color");
                }
                else
                {
                    info.hasBaseColor = false;
                }

                matInfos[i] = info;
            }
            backups.Add(matInfos);
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
        var emission = glowColor * intensity;
        lastEmission = emission;

        // Aplica a todos
        for (int rIdx = 0; rIdx < rends.Count; rIdx++)
        {
            var r = rends[rIdx];
            if (r == null) continue;

            var mats = r.materials;        
            var infos = backups[rIdx];
            if (infos == null) continue;

            for (int i = 0; i < mats.Length && i < infos.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                var info = infos[i];

                if (info.hasEmission)
                {
                    m.SetColor("_EmissionColor", emission);
                }
                else if (info.hasBaseColor)
                {
                    var c = info.origBaseColor; 
                    m.SetColor("_Color", c + (glowColor * (intensity * 0.25f)));
                }
            }
        }
    }

    void OnDisable()
    {
        // Restauramos estado original
        for (int rIdx = 0; rIdx < rends.Count; rIdx++)
        {
            var r = rends[rIdx];
            if (r == null) continue;

            var mats = r.materials;
            var infos = backups[rIdx];
            if (infos == null) continue;

            for (int i = 0; i < mats.Length && i < infos.Length; i++)
            {
                var m = mats[i];
                if (m == null) continue;
                var info = infos[i];

                if (info.hasEmission)
                {
                    m.SetColor("_EmissionColor", info.origEmissionColor);
                    if (!info.origEmissionKeyword) m.DisableKeyword("_EMISSION");
                }
                if (info.hasBaseColor)
                {
                    m.SetColor("_Color", info.origBaseColor);
                }
            }
        }

        rends.Clear();
        backups.Clear();
    }
}

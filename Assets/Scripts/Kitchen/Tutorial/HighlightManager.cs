using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    [Header("Default Style")]
    [SerializeField] private Color defaultColor = new Color(1f, 0.9f, 0.3f);
    [SerializeField] private float defaultMin = 0.0f;
    [SerializeField] private float defaultMax = 3.0f;
    [SerializeField] private float defaultFrequency = 1.0f;

    private readonly Dictionary<GameObject, HighlightObject> actives = new();

    // Activa highlight sobre un objetivo concreto
    public void Highlight(GameObject target,
                          Color? color = null,
                          float? minIntensity = null,
                          float? maxIntensity = null,
                          float? frequency = null)
    {
        if (target == null) return;

        PruneNulls();

        if (!actives.TryGetValue(target, out var blink) || blink == null)
        {
            blink = target.GetComponent<HighlightObject>();
            if (blink == null) blink = target.AddComponent<HighlightObject>();
            actives[target] = blink;
        }

        blink.Configure(
            color ?? defaultColor,
            minIntensity ?? defaultMin,
            maxIntensity ?? defaultMax,
            frequency ?? defaultFrequency
        );
        blink.enabled = true;
    }

    // Activa highlight sobre varios objetivos
    public void HighlightMany(IEnumerable<GameObject> targets,
                              Color? color = null,
                              float? minIntensity = null,
                              float? maxIntensity = null,
                              float? frequency = null)
    {
        if (targets == null) return;
        foreach (var t in targets)
            Highlight(t, color, minIntensity, maxIntensity, frequency);
    }

    // Desactiva y elimina el highlight de un objetivo concreto
    public void Clear(GameObject target)
    {
        if (target == null) return;

        if (actives.TryGetValue(target, out var blink) && blink != null)
        {
            blink.enabled = false; 
            Destroy(blink);
        }
        actives.Remove(target);
    }

    // Desactiva y elimina todos los highlights
    public void ClearAll()
    {
        foreach (var kv in actives)
        {
            var blink = kv.Value;
            if (blink != null)
            {
                blink.enabled = false;
                Destroy(blink);
            }
        }
        actives.Clear();
    }

    public bool IsHighlighted(GameObject target)
    {
        return target != null && actives.TryGetValue(target, out var b) && b != null && b.enabled;
    }

    private void PruneNulls()
    {
        // Elimina entradas cuyo GO fue destruido
        var toRemove = new List<GameObject>();
        foreach (var kv in actives)
            if (kv.Key == null || kv.Value == null) toRemove.Add(kv.Key);
        foreach (var k in toRemove) actives.Remove(k);
    }

    void OnDisable() => ClearAll();
    void OnDestroy() => ClearAll();
}
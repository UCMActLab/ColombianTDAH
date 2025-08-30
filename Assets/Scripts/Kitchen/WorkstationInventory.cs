using System.Collections.Generic;
using UnityEngine;

public class WorkstationInventory : MonoBehaviour
{
    private readonly Dictionary<Ingredientes, int> counts = new();

    public int TotalItems
    {
        get { int s = 0; foreach (var v in counts.Values) s += v; return s; }
    }

    public bool TryAdd(Ingredientes ing, int capacity = 99)
    {
        if (TotalItems >= capacity) return false;
        counts[ing] = counts.ContainsKey(ing) ? counts[ing] + 1 : 1;
        return true;
    }

    public bool Meets(RecetaData receta)
    {
        var need = new Dictionary<Ingredientes, int>();
        foreach (var ing in receta.ingredientes)
            need[ing] = need.ContainsKey(ing) ? need[ing] + 1 : 1;

        foreach (var kv in need)
        {
            int have = counts.ContainsKey(kv.Key) ? counts[kv.Key] : 0;
            if (have < kv.Value) return false;
        }
        return true;
    }

    public void ConsumeFor(RecetaData receta)
    {
        foreach (var ing in receta.ingredientes)
        {
            if (!counts.ContainsKey(ing)) continue;
            counts[ing] = Mathf.Max(0, counts[ing] - 1);
            if (counts[ing] == 0) counts.Remove(ing);
        }
    }

    public void ClearAll() => counts.Clear();

}

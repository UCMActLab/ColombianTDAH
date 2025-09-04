using System.Collections.Generic;
using UnityEngine;

public class HighlightManager : MonoBehaviour
{
    [SerializeField] private Material overlayMat;

    private List<Renderer> rends = new();
    private List<Material[]> originals = new();

    public void Highlight(GameObject go)
    {
        Clear();
        if (!go || !overlayMat) return;

        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
        {
            rends.Add(r);
            var mats = r.materials;
            originals.Add(mats);
            var nm = new Material[mats.Length + 1];
            mats.CopyTo(nm, 0);
            nm[mats.Length] = overlayMat;
            r.materials = nm;
        }
    }

    public void Clear()
    {
        for (int i = 0; i < rends.Count; i++)
            if (rends[i] != null && originals[i] != null)
                rends[i].materials = originals[i];
        rends.Clear(); originals.Clear();
    }
}

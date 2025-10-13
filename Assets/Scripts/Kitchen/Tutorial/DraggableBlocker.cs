using System.Collections.Generic;
using UnityEngine;

public static class DraggableBlocker
{
    public enum Source { Pause, Tutorial, Modal }

    private static readonly HashSet<Source> active = new();
    public static bool Blocked => active.Count > 0;
    
    private static readonly HashSet<GameObject> allowed = new(); // Whitelist para el Tutorial
    public static void Block(Source s) { active.Add(s); }
    public static void Unblock(Source s) { active.Remove(s); }

    public static void Block() => Block(Source.Modal);
    public static void Unblock() => Unblock(Source.Modal);

    public static void ConmuteBlock(bool b)
    {
        if (b) Block(Source.Pause);
        else Unblock(Source.Pause);
    }

    public static void AllowOnly(params GameObject[] gos)
    {
        allowed.Clear();
        AddAllowed(gos);
    }

    public static void AddAllowed(params GameObject[] gos)
    {
        if (gos == null) return;
        foreach (var go in gos)
            if (go) allowed.Add(go);
    }

    public static void ClearAllowed() => allowed.Clear();

    /// <summary>
    /// Devuelve si este GO puede interactuar ahora mismo.
    /// - Si NO hay bloqueo => true
    /// - Si hay bloqueo por Pause/Modal => false
    /// - Si hay Tutorial => true solo si está en la whitelist
    /// </summary>
    public static bool IsAllowed(GameObject go)
    {
        if (!Blocked) return true; 

        bool tutorialActive = active.Contains(Source.Tutorial);
        if (tutorialActive)
            return go && allowed.Contains(go);

        return false;
    }

    public static void ResetAll()
    {
        active.Clear();
        allowed.Clear();
    }
}

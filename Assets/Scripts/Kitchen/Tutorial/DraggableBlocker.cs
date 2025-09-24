using System.Collections.Generic;
using UnityEngine;

public static class DraggableBlocker
{
    public enum Source { Pause, Tutorial, Modal }

    private static readonly HashSet<Source> active = new();
    public static bool Blocked => active.Count > 0;

    public static void Block(Source s) { active.Add(s); }
    public static void Unblock(Source s) { active.Remove(s); }

    public static void Block() => Block(Source.Modal);
    public static void Unblock() => Unblock(Source.Modal);

    public static void ConmuteBlock(bool b)
    {
        if (b) Block(Source.Pause);
        else Unblock(Source.Pause);
    }
}

using UnityEngine;

public static class TutorialBlocker
{
    public static bool Blocked { get; private set; }

    public static void Block() => Blocked = true;
    public static void Unblock() => Blocked = false;
}

using UnityEngine;

public static class DraggableBlocker
{
    public static bool Blocked { get; private set; }

    public static void Block() => Blocked = true;
    public static void Unblock() => Blocked = false;

    public static bool ConmuteBlock() => Blocked = !Blocked;
}

using UnityEngine;

public class Floatie : MovingObject
{
    private void Start()
    {
        base.Start();
        type = Box.Floatie;
        collisionReaction = true;
    }

}

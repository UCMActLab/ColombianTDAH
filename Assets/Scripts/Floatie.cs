using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Floatie : MovingObject
{
    List<int> collidedWith;  // Lista de identificadores con los que ya ha colisionado 
    private void Start()
    {
        base.Start();
        type = Box.Floatie;
        collisionReaction = true;
        collidedWith = new List<int>();
    }

    public bool TryScore(int id) // Solo sumará o se podrá colocar si aun no ha pasado por el flotador
    {
        if (collidedWith.Contains(id)) return false;
        else
        {
            collidedWith.Add(id);
            return true;
        }
    }
}

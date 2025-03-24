using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Floatie : MovingObject
{
    List<int> collidedWith;  // Lista de identificadores con los que ya ha colisionado 
    [SerializeField]
    BoxCollider jumpCollider;
    Vector3 initialJumpColliderSize;
    Vector3 initialJumpColliderPos;


    private void Start()
    {
        base.Start();
        type = Box.Floatie;
        collisionReaction = true;
        collidedWith = new List<int>();
    }
    private void Awake()
    {
        //jumpCollider = GetComponentInChildren<BoxCollider>();
        initialJumpColliderPos = jumpCollider.transform.position;
        initialJumpColliderSize = jumpCollider.size;
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

    public override void SetVel(float obsVel)
    {
        base.SetVel(obsVel);

        Vector3 newSize = new Vector3(0, 0, 0); 
        Vector3 newPos = new Vector3(0, 0, 0);

        newPos = initialJumpColliderPos; newSize = initialJumpColliderSize;
        newPos.x -=  1.6f * 4.5f;
        newSize = initialJumpColliderSize;

        jumpCollider.size = newSize;
        jumpCollider.transform.position= newPos;
    }
}

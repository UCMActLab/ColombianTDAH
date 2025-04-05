using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Floatie : MovingObject
{
    List<int> collidedWith;  // Lista de identificadores de los delfines con los que ya ha colisionado 
    [SerializeField]
    BoxCollider jumpCollider; // Collider para triggear salto automático y que el delfín enceste en este flotador
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
        initialJumpColliderPos = jumpCollider.transform.position;
    }

    /// <summary>
    /// Solo sumará puntos o se podrá colocar si el delfín aun no ha pasado por este flotador
    /// </summary>
    /// <param name="id">Identificador (índice) del delfín</param>
    /// <returns>True on success</returns>
    public bool TryScore(int id)
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

        //Cálculo de la distancia del trigger de salto para el salto automático del delfín a este flotador
        Vector3 newPos = new Vector3(0, 0, 0);

        newPos = initialJumpColliderPos; 
        newPos.x -= 2.35f * 4.5f; // Cálculo aproximado (la animación aumenta xon la velocidad con los objetos)

        jumpCollider.transform.position= newPos;
    }
}

using UnityEngine;

public class Obstaculo : MovingObject
{
    private void Start()
    {
        base.Start();
        type = Box.Obstacle;
        collisionReaction = true;
    }

    private void OnCollisionEnter(Collision collision)
    {
        base.OnCollisionEnter(collision);

        if (collision.gameObject.GetComponent<DolphinController>() != null)
        {
            // Desactiva Velocidad aumentada
            DolphinLevelManager.Instance.DeactivateIncreasedSpeed();
        }
    }
}
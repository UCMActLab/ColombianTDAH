using UnityEngine;

public class Obstaculo : MovingObject
{
    [SerializeField]
    Box _type;

    private void Start()
    {
        base.Start();
        type = _type;
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

    public Box GetType()
    {
        return _type;
    }
}
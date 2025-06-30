using UnityEngine;

public class ObstacleDeadZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // Si un objeto coisiona o manda a destruir
        MovingObject movingComp = other.GetComponent<MovingObject>();
        if (movingComp != null) {
            movingComp.DestroyObstacle();
        }
        ObstaculoPelota obstaculoPelota = other.GetComponent<ObstaculoPelota>();
        if (obstaculoPelota != null)
        {
            obstaculoPelota.DestroyObstacle();
        }
    }
}

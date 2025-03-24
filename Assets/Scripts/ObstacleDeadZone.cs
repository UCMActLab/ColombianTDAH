using UnityEngine;

public class ObstacleDeadZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        MovingObject movingComp = other.GetComponentInParent<MovingObject>();
        if (movingComp != null) {
            movingComp.DestroyObstacle();
        }
    }
}

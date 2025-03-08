using UnityEngine;

public class ObstacleDeadZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Obstaculo obstacleComp = other.GetComponent<Obstaculo>();
        if (obstacleComp != null) {
            obstacleComp.DestroyObstacle();
        }
    }
}

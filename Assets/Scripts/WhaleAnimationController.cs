using UnityEngine;
using static DolphinController;

public class WhaleAnimationController : MonoBehaviour
{
    public void OnAnimationEnded(string action)
    {
        gameObject.SetActive(false);
        DolphinLevelManager.Instance.SetAllObstacleSpawning(true);
    }
}

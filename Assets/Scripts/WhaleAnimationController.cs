using UnityEngine;
using static DolphinController;

public class WhaleAnimationController : MonoBehaviour
{
    // Metodo llamado al finalizar la animacion de la ballena
    public void OnAnimationEnded(string action)
    {
        // Desactiva ballena y vuelve a activar spawner de objetos
        if(action=="DiveExit")
        {
            gameObject.SetActive(false);
            DolphinLevelManager.Instance.SetAllObstacleSpawning(true);
        }
    }
}

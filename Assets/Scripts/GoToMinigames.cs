using UnityEngine;

public class GoToMinigames : MonoBehaviour
{
    public void GoToMinigameScene(string minigameSceneName)
    {
        SceneLoader.LoadScene(minigameSceneName);
    }
}

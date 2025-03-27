using UnityEngine;

public class OptionsMenu : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameObject.SetActive(false);
    }

    public void Toggle()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        DolphinLevelManager.Instance.Pause(gameObject.activeSelf); // pausa el juego
    }
}

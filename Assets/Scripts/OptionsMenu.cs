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

        EventRegister.TipoJuego gameType = EventRegister.Instance.GetInfoSesion().nombreJuego;

        if (gameType == EventRegister.TipoJuego.Delfines)
            DolphinLevelManager.Instance.Pause(gameObject.activeSelf); // pausa el juego

        else if (gameType == EventRegister.TipoJuego.MisionColombia)
            MisionLevelManager.Instance.Pause(gameObject.activeSelf);

        else if (gameType == EventRegister.TipoJuego.Cocina)
            LevelKitchenManager.Instance.Pause(gameObject.activeSelf);
        
    }

    public void ToggleWithoutPause()
    {
        gameObject.SetActive(!gameObject.activeSelf);
    }
}

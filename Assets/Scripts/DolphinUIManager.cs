using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class DolphinUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _pointsText;
    [SerializeField]
    private TMP_Text _levelText;
    [SerializeField]
    GameObject _endLevel;
    [SerializeField]
    private Button _velButton;
    [SerializeField]
    GameObject _options;
    [SerializeField]
    private GameObject _configUIObject;
    
    void OnEnable()
    {
        // Desactivo UI configuracion
        _configUIObject.SetActive(false);
    }

    public void startLevelStats(int level, int points)
    {
        _pointsText.SetText("Puntos: " + level.ToString());
        _levelText.SetText("Nivel: "+ points.ToString());
    }
    public void updatePoints(int points)
    {
        _pointsText.SetText("Puntos: " + points.ToString());
    }

    public void updateLevel(int level)
    {
        _pointsText.SetText(level.ToString());
    }

    public void showWin()
    {
        _pointsText.enabled = false;
        _levelText.enabled = false;
        _options.SetActive(false);

        // Activamos opciones de fin de nivel
        _endLevel.SetActive(true);
    }

    public void VelocityClick()
    {
        SetVelButton(false);
        DolphinLevelManager.Instance.ActivateIncreasedSpeed();
    }

    public void SetVelButton(bool enable)
    {
        _velButton.interactable = enable;
    }
}

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
    private Slider _levelSlider;
    [SerializeField]
    GameObject _endLevel;
    [SerializeField]
    private Button _velButton;
    [SerializeField]
    GameObject _options;
    [SerializeField]
    private GameObject _configUIObject;

    // Texto puntos encima del delfín
    [SerializeField]
    public GameObject _pointsTextPrefab;
    [SerializeField]
    public float _pointsTextLifeTime;

    private int _maxPoints;

    void OnEnable()
    {
        // Desactivo UI configuracion
        _configUIObject.SetActive(false);
    }

    public void startLevelStats(int level, int points)
    {
        _pointsText.SetText("Puntos:\n0 / " + points.ToString());
        _maxPoints = points;
        _levelSlider.maxValue = _maxPoints;
        _levelSlider.value = 0f;
        _levelText.SetText("Nivel: "+ level.ToString());
    }
    public void updatePoints(int points)
    {
        _pointsText.SetText("Puntos: " + points.ToString() + "/" + _maxPoints.ToString());
        _levelSlider.value = points;
    }

    public void updateLevel(int level)
    {
        _pointsText.SetText(level.ToString());
    }

    public void showWin()
    {
        _pointsText.enabled = false;
        _levelText.enabled = false;
        _levelSlider.gameObject.SetActive(false);
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

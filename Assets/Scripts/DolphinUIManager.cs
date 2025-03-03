using UnityEngine;
using TMPro;
public class DolphinUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _pointsText;
    [SerializeField]
    private TMP_Text _levelText;
    [SerializeField]
    GameObject _endLevel;
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
        _pointsText.SetText("Points: " + level.ToString());
        _levelText.SetText("Level: "+ points.ToString());
    }
    public void updatePoints(int points)
    {
        _pointsText.SetText("Points: " + points.ToString());
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
        _endLevel.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

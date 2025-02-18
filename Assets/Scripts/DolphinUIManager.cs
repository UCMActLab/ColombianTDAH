using UnityEngine;
using TMPro;
public class DolphinUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text pointsText;
    //private GameObject points;
    [SerializeField]
    private TMP_Text levelText;
    //private GameObject level;


    [SerializeField]
    GameObject win;
    [SerializeField]
    GameObject nextLevelButton;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //pointsText = points.GetComponent<TMP_Text>();
        //levelText = level.GetComponent<TMP_Text>();
    }
    public void startLevelStats(int level, int points)
    {
        pointsText.SetText("Level: " + level.ToString());
        levelText.SetText("Points: " + points.ToString());
    }
    public void updatePoints(int points)
    {
        pointsText.SetText("Points: " + points.ToString());
    }

    public void updateLevel(int level)
    {
        pointsText.SetText(level.ToString());
    }

    public void showWin()
    {
        // points.SetActive(false);
        //level.SetActive(false);
        pointsText.enabled = false;
        levelText.enabled = false;

        win.SetActive(true);
        nextLevelButton.SetActive(true);
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

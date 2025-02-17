using UnityEngine;
using TMPro;
public class DolphinUIManager : MonoBehaviour
{
    [SerializeField]
    private TMP_Text pointsText;
    [SerializeField]
    private TMP_Text levelText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }
    public void startLevelStats(int level, int points)
    {
        pointsText.SetText(level.ToString());
        levelText.SetText(points.ToString());
    }
    public void updatePoints(int points)
    {
        pointsText.SetText(points.ToString());
    }

    public void updateLevel(int level)
    {
        pointsText.SetText(level.ToString());
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using UnityEngine.UI;

public class MisionUIManager : MonoBehaviour
{
    [SerializeField]
    GameObject _decisionGO;

    [SerializeField]
    Slider _timeSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _decisionGO.SetActive(false);

        float answerTime = MisionLevelManager.Instance.GetAnswerTime();

        InitSlider(0, answerTime);
    }

    // Update is called once per frame
    void Update()
    {

    }

    void InitSlider(float min, float max)
    {
        _timeSlider.maxValue = max;
        _timeSlider.minValue = min;
        _timeSlider.value = max;
    }

    public void ShowDecisionButtons()
    {
        _decisionGO.SetActive(true);
    }

    public void HideDecisionButtons()
    {
        _decisionGO.SetActive(false);
    }

    public void YesClicked()
    {
        Debug.Log("YEEES");
        Clicked();
    }

    public void NoClicked()
    {
        Debug.Log("NOOO");
        Clicked();
    }

    void Clicked()
    {
        _decisionGO.SetActive(false);
        MisionLevelManager.Instance.Answered();
    }

    public void UpdateSlider(float value)
    {
        _timeSlider.value = value;
    }
}

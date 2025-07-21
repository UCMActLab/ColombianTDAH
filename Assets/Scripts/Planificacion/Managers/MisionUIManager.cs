using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MisionUIManager : MonoBehaviour
{
    [SerializeField]
    GameObject _decisionGO;

    [SerializeField]
    Slider _timeSlider;

    [SerializeField]
    TextMeshProUGUI _time;

    [SerializeField]
    PlanificationList _planificationListComp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _decisionGO.SetActive(false);

        MisionLevelManager.Instance.RegisterUIManager(this);
        float answerTime = MisionLevelManager.Instance.GetAnswerTime();

        InitSlider(0, answerTime);
    }

    // Inicializa valores del slider
    void InitSlider(float min, float max)
    {
        _timeSlider.maxValue = max;
        _timeSlider.minValue = min;
        _timeSlider.value = max;
    }

    // Activa botones de respuesta
    public void ShowDecisionButtons()
    {
        _decisionGO.SetActive(true);
    }

    // Desactiva botones de respuesta
    public void HideDecisionButtons()
    {
        _decisionGO.SetActive(false);
    }

    // Respuesta si
    public void YesClicked()
    {
        Debug.Log("YEEES");
        Clicked();
    }

    // Respuesta no
    public void NoClicked()
    {
        Debug.Log("NOOO");
        Clicked();
    }

    // Desactiva pregunta y botones de respuesta
    void Clicked()
    {
        _decisionGO.SetActive(false);
        MisionLevelManager.Instance.Answered();
    }

    // Actualiza el valor del slider
    public void UpdateSlider(float value)
    {
        _timeSlider.value = value;
    }

    // Cambia tiempo de la hora
    public void ChangeTime(string newTime)
    {
        string[] separatedTime = newTime.Split(" ");

        if (separatedTime[1][0] == 'a')
            separatedTime[1] = "AM";
        else if(separatedTime[1][0] == 'p')
            separatedTime[1] = "PM";

        _time.text = separatedTime[0] + ":00 " + separatedTime[1];
    }

    public void SetStartTime(string newTime)
    {
        _planificationListComp.SetDepartureTime(newTime);

        ChangeTime(newTime);
    }

    public void SetStops(List<string> stops)
    {
        _planificationListComp.SetStops(stops);
    }

    public void SetSleepHours(List<string> sleepHours)
    {
        _planificationListComp.SetSleepHours(sleepHours);
    }

    public void SetLocationHours(List<string> locationHours)
    {
        _planificationListComp.SetLocationHours(locationHours);
    }
}

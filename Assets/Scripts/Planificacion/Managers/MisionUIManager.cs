using NUnit.Framework;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MisionUIManager : MonoBehaviour
{
    [SerializeField]
    GameObject _decisionGO;

    [SerializeField]
    TextMeshProUGUI _questionText;

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
        string mensaje = "Pregunta final, se ha contestado";
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.TerminaDecision, mensaje));
        EventRegister.Instance.EvntToJson();

        _decisionGO.SetActive(false);
        MisionLevelManager.Instance.Answered();
    }

    // Actualiza el valor del slider
    public void UpdateSlider(float value)
    {
        _timeSlider.value = value;
    }

    public void ChangeQuestion(string qText)
    {
        _questionText.text = qText;
    }

    // Cambia tiempo de la hora
    public void ChangeTime(HourMinSec newTime)
    {
        _time.text = newTime.GetHMString();
    }

    public void SetStartTime(HourMinSec newTime)
    {
        _planificationListComp.SetDepartureTime(newTime.GetHString());

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

    public void ExitLevel() {
        MisionLevelManager.Instance.ExitLevel();
    }
}

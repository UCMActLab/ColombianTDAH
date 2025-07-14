using NUnit.Framework;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class MapUIManager : MonoBehaviour
{
    [SerializeField]
    List<TextMeshProUGUI> rules;

    [SerializeField]
    GameObject _warningGO;

    [SerializeField]
    List<TMP_Dropdown> dropdowns;

    [SerializeField]
    TextMeshProUGUI _stopsExtraMins;

    [SerializeField]
    TextMeshProUGUI _sleepExtraHours;

    [SerializeField]
    TextMeshProUGUI _totalTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ConfirmPlanification()
    {
        SceneManager.LoadScene("MC_Level");
    }

    public void SetStops(int number, int minutes)
    {
        rules[0].text = "- " + number + " paradas de " + minutes + " minutos.";
    }

    public void SetSleepHours(int hours)
    {
        rules[1].text = "- Dormir " + hours + " horas.";

    }

    public void SetLocationFrec(int hours)
    {
        rules[2].text = "- Enviar ubicación cada " + hours + " horas.";
    }

    public void SetDepartureHours(List<string> optiondatas)
    {
        SetHours(0, optiondatas);
    }

    public void SetAllSleepHours(List<string> optiondatas)
    {
        SetHours(1, optiondatas);
    }

    public void SetLocationHours(List<string> optiondatas)
    {
        SetHours(2, optiondatas);
    }

    public void SetStopsNames(List<string> optiondatas)
    {
        SetHours(3, optiondatas);
    }

    private void SetHours(int index, List<string> optiondatas)
    {
        dropdowns[index].ClearOptions();
        dropdowns[index].AddOptions(optiondatas);
    }

    public void SetStopExtraMins(int extraMins)
    {
        _stopsExtraMins.text = "+" + extraMins + "'";
    }

    public void SetSleepExtraHours(int extraHours)
    {
        _sleepExtraHours.text = "+" + extraHours + "h";
    }

    private void SetTotalTime()
    {
        // Recalcla horas totales desde la hora de salida y con la suma extra
        // Si supera el maximo de horas que puede durar el viaje se pondra en rojo.
        // Get Total Time del Game Manager 
        _totalTime.text = "XXh YY'";
    }

    public void SetWarning(bool enabled)
    {
        _warningGO.SetActive(enabled);
    }
}

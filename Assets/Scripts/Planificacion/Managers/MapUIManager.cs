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

    // Actualiza paradas minimas en reglas UI
    public void SetStops(int number, int minutes)
    {
        rules[0].text = "- " + number + " paradas de " + minutes + " minutos.";
    }

    // Actualiza horas de suenyo minimas reglas UI
    public void SetSleepHours(int hours)
    {
        rules[1].text = "- Dormir " + hours + " horas.";

    }

    // Actualiza horas minimas entre mensaje de ubicacion en reglas UI
    public void SetLocationFrec(int hours)
    {
        rules[2].text = "- Enviar ubicación cada " + hours + " horas.";
    }

    // Actualiza opciones horas de salida en la UI
    public void SetDepartureHours(List<string> optiondatas)
    {
        SetHours(0, optiondatas);
    }

    // Actualiza opciones horas de dormir en la UI
    public void SetAllSleepHours(List<string> optiondatas)
    {
        SetHours(1, optiondatas);
    }

    // Actualiza opciones horas de ubicacion en la UI
    public void SetLocationHours(List<string> optiondatas)
    {
        SetHours(2, optiondatas);
    }

    // Actualiza opciones paradas en la UI
    public void SetStopsNames(List<string> optiondatas)
    {
        SetHours(3, optiondatas);
    }

    // Actualiza opciones del Dropbox
    private void SetHours(int index, List<string> optiondatas)
    {
        dropdowns[index].ClearOptions();
        dropdowns[index].AddOptions(optiondatas);
    }

    // Actualiza en la UI minutos extra por parada
    public void SetStopExtraMins(int extraMins)
    {
        _stopsExtraMins.text = "+" + extraMins + "'";
    }

    // Actualiza en la UI horas extra por dormir
    public void SetSleepExtraHours(int extraHours)
    {
        _sleepExtraHours.text = "+" + extraHours + "h";
    }

    // Actualiza duracion de viaje total en la UI
    public void SetTotalTime(int totalHours, int totalMins, bool correct)
    {
        _totalTime.text = totalHours + "h " + totalMins + "'";

        if (correct)
            _totalTime.color = Color.cyan;
        else 
            _totalTime.color = Color.red;
    }

    // Activa o desactiva mensaje de aviso
    public void SetWarning(bool enabled)
    {
        _warningGO.SetActive(enabled);
    }
}

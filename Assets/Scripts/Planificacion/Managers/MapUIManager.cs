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
    List<TMP_Dropdown> dropdowns;
    

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

    public void SetLocationHours(List<string> optiondatas)
    {
        SetHours(1, optiondatas);
    }


    private void SetHours(int index, List<string> optiondatas)
    {
        dropdowns[index].ClearOptions();
        dropdowns[index].AddOptions(optiondatas);
    }
}

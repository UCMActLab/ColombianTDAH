using NUnit.Framework;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using TMPro;

public class MapUIManager : MonoBehaviour
{
    [SerializeField]
    List<TextMeshProUGUI> rules;

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
}

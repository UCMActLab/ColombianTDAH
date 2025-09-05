using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static EventRegister;

public class ShowResumen : MonoBehaviour
{

    [SerializeField] private GameObject checkListGO;
    private List<Transform> checkElements = new List<Transform>();

    [SerializeField] private TextMeshProUGUI winLoseText;
    [SerializeField] private GameObject exitButton;

    [SerializeField] private AudioManagerResumen audioManager;

    private int numSleepHours;
    private string horaSalida;
    private int paradasCorrectas;
    private int ubicacionesCorrectas;

    bool win;
    public struct CheckInfo
    {
        public string Texto;
        public bool Conseguido;

        public CheckInfo(string texto, bool conseguido)
        {
            Texto = texto;
            Conseguido = conseguido;
        }
    }

    private List<CheckInfo> checksInfo;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        win = true;
        exitButton.SetActive(false);
        winLoseText.gameObject.SetActive(false);


        foreach (Transform child in checkListGO.transform)
        {
            checkElements.Add(child);

        }

        int numChecks = checkElements.Count;


        //para hacer pruebas
        numSleepHours = 8;
        horaSalida = "12AM";
        paradasCorrectas = 0;
        ubicacionesCorrectas = 3;
        int ubicacionesTotales = 10;
        int paradasTotales = 8;
        int numSleepHoursTotales = 8;
        //ponerlos de verdad
        if (MisionLevelManager.Instance != null)
        {
            MisionLevelManager.Instance.Pause(true);
            numSleepHours = MisionLevelManager.Instance.SleptHours;
            numSleepHoursTotales = MisionLevelManager.Instance.TotalSleepHours;

            horaSalida = MisionLevelManager.Instance.StartTime;
            paradasTotales = MisionLevelManager.Instance.SelectedInitialStops.Count;
            paradasCorrectas = paradasTotales - MisionLevelManager.Instance.SelectedStops.Count;

            ubicacionesCorrectas = MisionLevelManager.Instance.SelectedLocationHours.Count;
            ubicacionesTotales = MisionLevelManager.Instance.SelectedLocationHours.Count;
        }
        

        checksInfo = new List<CheckInfo>(numChecks);

        checksInfo.Add(new CheckInfo($"Horas de sueño..........{numSleepHours}/{numSleepHoursTotales}", numSleepHours == numSleepHoursTotales));
        checksInfo.Add(new CheckInfo($"Hora de salida...........{horaSalida}", true));
        checksInfo.Add(new CheckInfo($"Paradas correctas...{paradasCorrectas}/{paradasTotales}", paradasCorrectas == paradasTotales));
        checksInfo.Add(new CheckInfo($"Ubicación mandada...{ubicacionesCorrectas}/{ubicacionesTotales}", ubicacionesCorrectas == ubicacionesTotales));


        showChecklist();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    void showChecklist()
    {
        StartCoroutine(ShowChecklistCoroutine());
    }

    IEnumerator ShowChecklistCoroutine()
    {
        for (int i = 0; i < checkElements.Count; i++)
        {
            Debug.Log("count " + checkElements.Count);

            Debug.Log(i);
            audioManager.PlayChecklistSound(checksInfo[i].Conseguido);

            showCheckElement(i, checksInfo[i].Conseguido, checksInfo[i].Texto);
            yield return new WaitForSeconds(2f);
        }

        ActivateWinMessage();
    }

    //que por cada hijo de la checklist haya que ir metiendolos con su info correcta
    void showCheckElement(int childIndex, bool conseguido, string texto)
    {
        if (!conseguido) win = false; //si alguno de los checks no se cumple no se gana

        Debug.Log("Child number " + childIndex);
        Debug.Log("texto " + texto);

        //tiene que estar asi
        //checklist
        //--checkelement
        //-----text
        //-----tick
        //-----cross

        checkElements[childIndex].gameObject.SetActive(true);

        if (checkElements[childIndex].GetChild(0).gameObject.GetComponent<TextMeshProUGUI>() != null)
        {
            checkElements[childIndex].GetChild(0).gameObject.GetComponent<TextMeshProUGUI>().text = texto;

        }
        else
        {
            Debug.Log("text component nulo");

        }

        checkElements[childIndex].GetChild(1).gameObject.SetActive(conseguido);
        checkElements[childIndex].GetChild(2).gameObject.SetActive(!conseguido);
    }

    void ActivateWinMessage()
    {
        string winText = "NIVEL COMPLETADO\r\n     BIEN HECHO! :)";
        string loseText = "CASI LO TIENES\r\nPRUEBA OTRA VEZ";

        audioManager.PlayWinLoseSound(win);

        winLoseText.gameObject.SetActive(true);
        winLoseText.text = win ? winText : loseText;

        Color winColor = new Color(87f / 255f, 126f / 255f, 215f / 255f, 1f);
        Color loseColor = new Color(80f / 255f, 105f / 255f, 200f / 255f, 1f);

        winLoseText.color = win ? winColor : loseColor;

        exitButton.SetActive(true);

    }

    public void ExitButtonPress()
    {
        if (MisionLevelManager.Instance != null)
        {
            MisionLevelManager.Instance.ExitLevel();

        }
        else
        {
            Debug.Log("MisionLevelManager nulo");
        }
    }
}

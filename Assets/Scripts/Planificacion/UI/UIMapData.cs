using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIMapData : MonoBehaviour
{
    UIDocument _document;
    Button _acceptButton;

    // Reglas
    IntegerField _stopNumber;
    IntegerField _stopMins;
    IntegerField _sleepHours;
    IntegerField _location;
    IntegerField _duration;
    IntegerField _durationMax;

    // Planificacion
    Toggle[,] _depHours;
    Toggle[,] _locHours;
    Toggle[,] _allSleepHours;
    IntegerField _hourPerSleep;

    TextField _stopNames;

    // Ejecucion
    IntegerField _answerTime;


    [SerializeField]
    GameObject _map;

    [SerializeField]
    GameObject _dialogs;

    // Scriptable Object
    [SerializeField]
    MisionConfigurationData _config = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Guarda referencias
        _document = GetComponent<UIDocument>();

        if (_document != null)
        {
            // Reglas minimo
            _stopNumber = _document.rootVisualElement.Q<IntegerField>("NumeroParadas");
            _stopMins = _document.rootVisualElement.Q<IntegerField>("TiempoParadas");
            _sleepHours = _document.rootVisualElement.Q<IntegerField>("HorasSueno");
            _location = _document.rootVisualElement.Q<IntegerField>("Ubicacion");
            _duration = _document.rootVisualElement.Q<IntegerField>("Duracion");
            _durationMax = _document.rootVisualElement.Q<IntegerField>("DuracionMax");

            // Planificacion
            _depHours = new Toggle[12, 2];
            _locHours = new Toggle[12, 2];
            _allSleepHours = new Toggle[12, 2];
            _hourPerSleep = _document.rootVisualElement.Q<IntegerField>("CuantoDormir");
            _stopNames = _document.rootVisualElement.Q<TextField>("ParadasTextField");

            // Referencias Toggles horas Salida, Dormir y Ubicacion
            string name = "";
            string timeMode = "am";
            for (int i = 1; i < 3; i++)
            {
                for (int j = 1; j < 13; j++)
                {
                    name = j + timeMode;

                    _depHours[j - 1, i - 1] = _document.rootVisualElement.Q<Toggle>(name);
                    _locHours[j - 1, i - 1] = _document.rootVisualElement.Q<Toggle>(name + "L");
                    _allSleepHours[j - 1, i - 1] = _document.rootVisualElement.Q<Toggle>(name + "S");
                }

                timeMode = "pm";
            }

            // Ejecucion
            _answerTime = _document.rootVisualElement.Q<IntegerField>("TiempoRespuesta");

            // Callback boton
            _acceptButton = _document.rootVisualElement.Q("guardarYjugar") as Button;

            if (_acceptButton != null)
                _acceptButton.RegisterCallback<ClickEvent>(OnAcceptClick);
        }

        if (_map != null)
            _map.SetActive(false);

        if (_dialogs != null)
            _dialogs.SetActive(false);
    }

    private void OnDisable()
    {
        _acceptButton.UnregisterCallback<ClickEvent>(OnAcceptClick);
    }

    private void OnAcceptClick(ClickEvent ce)
    {
        SaveConfiguration();
        _document.enabled = false;
        _map.SetActive(true);
        _dialogs.SetActive(true);
    }

    private void SaveConfiguration()
    {
        // CONFIGURACION REGLAS
        // Paradas
        _config.NumStops = _stopNumber.value;
        _config.StopMins = _stopMins.value;

        // Dormir
        _config.SleepHours = _sleepHours.value;

        // Ubicacion
        _config.Location = _location.value;

        // Duracion
        _config.Duration = _duration.value;
        _config.DurationMax = _durationMax.value;

        // CONFIGURACION PLANIFICACION
        // Horas
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                // Horas salida
                _config.DepartureHours[j, i] = _depHours[j, i].value;
                // Horas ubicacion
                _config.LocationHours[j, i] = _locHours[j, i].value;
                //Horas dormir
                _config.AllSleepHours[j, i] = _allSleepHours[j, i].value;
            }
        }

        // Horas por cada parada de dormir
        _config.HoursPerSleep = _hourPerSleep.value;

        string auxString = _stopNames.value;

        _config.StopsNames = SeparateStopNames(auxString);

        // CONFIGURACION EJECUCION
        // Tiempo de respuesta en segundos
        _config.AnswerTime = _answerTime.value;

        // Game Manager
        MisionLevelManager.Instance.LoadConfiguration(_config);

    }

    // Separa un texto y devuelve lista de palabras
    private List<string> SeparateStopNames(string names)
    {
        char[] delimiterChars = {',', '.'};
        string[] words = names.Split(delimiterChars);

        List<string> wordsList = words.ToList();

        return wordsList;
    }
}

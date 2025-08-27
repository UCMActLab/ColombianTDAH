using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIMapData : MonoBehaviour
{
    UIDocument _document;
    Button _nextButton;

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
    GameObject uiQuestionDoc;

    // Scriptable Object
    [SerializeField]
    MisionConfigurationData _config = null;

    int levelId;
    string levelInfoPath = "";

    bool _editMode;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _editMode = SetModeConfig();
        if (_editMode)
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
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 12; j++)
                    {
                        if (j != 0)
                            name = j + timeMode;
                        else
                            name = 12 + timeMode;

                        _depHours[j, i] = _document.rootVisualElement.Q<Toggle>(name);
                        _locHours[j, i] = _document.rootVisualElement.Q<Toggle>(name + "L");
                        _allSleepHours[j, i] = _document.rootVisualElement.Q<Toggle>(name + "S");
                    }

                    timeMode = "pm";
                }

                // Ejecucion
                _answerTime = _document.rootVisualElement.Q<IntegerField>("TiempoRespuesta");

                // Callback boton
                _nextButton = _document.rootVisualElement.Q("Siguiente") as Button;

                if (_nextButton != null)
                    _nextButton.RegisterCallback<ClickEvent>(OnNextClick);
            }

            Debug.Log("getIsDefaultConfig " + SceneLoader.Instance.getIsDefaultConfig());

            if (!SceneLoader.Instance.getIsDefaultConfig()) SetUIFromJSON();
        }
    }

    private void OnDisable()
    {
        if (_editMode)
            _nextButton.UnregisterCallback<ClickEvent>(OnNextClick);
    }

    private void OnNextClick(ClickEvent ce)
    {
        SaveConfiguration();
        uiQuestionDoc.SetActive(true);
        uiQuestionDoc.GetComponent<UIConfigQuestion>().Init(_config.StopsNames, _config);
        gameObject.SetActive(false);
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
        int k = 0;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                // Horas salida
                _config.DepartureHours[k] = _depHours[j, i].value;
                // Horas ubicacion
                _config.LocationHours[k] = _locHours[j, i].value;
                //Horas dormir
                _config.AllSleepHours[k] = _allSleepHours[j, i].value;

                k++;
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
        char[] delimiterChars = { ',', '.' };
        string[] words = names.Split(delimiterChars);

        List<string> wordsList = words.ToList();

        return wordsList;
    }

    private bool SetModeConfig()
    {
        bool editMode = SceneLoader.Instance.getMode();
        levelId = SceneLoader.Instance.getCurrentLevelId(EventRegister.TipoJuego.MisionColombia);
        string writeDir = System.IO.Path.Combine(Application.persistentDataPath, "configInfoMC");

        if (editMode) //el usuario quiere editar el juego
        {
            if (!System.IO.Directory.Exists(writeDir))
            {
                System.IO.Directory.CreateDirectory(writeDir);
            }
            levelInfoPath = System.IO.Path.Combine(writeDir, "DefaultMisionConfigurationData" + levelId + ".json");

         

        }
        else //se carga el nivel por default
        {
            levelInfoPath = "Planificacion/DefaultLevels/DefaultMisionConfigurationData" + levelId;
            Debug.Log("cargando nivel default desde " + levelInfoPath);

            _config = Resources.Load<MisionConfigurationData>(levelInfoPath);

            // Los niveles por defecto están desbloqueados, pero se hace la comprobación por si acaso
            if (_config.Desbloqueado)
            {
                // Me desactivo
                MisionLevelManager.Instance.LoadConfiguration(_config);
                MisionLevelManager.Instance.LoadQuestions(_config.Questions.ToDictionary(), _config.DistractionStops);
                MisionLevelManager.Instance.ActivateGame();
                gameObject.SetActive(false);
            }

        }
        _config.configName = levelInfoPath;

        return editMode;
    }

    private void SetUIFromJSON()
    {
        Debug.Log("SetUIFromJSON MisionConfigurationData " + levelInfoPath);

        // Verificar si el archivo existe antes de leerlo
        if (!System.IO.File.Exists(levelInfoPath)) return;

        Debug.Log("SetUIFromJSON MisionConfigurationData after reading " + levelInfoPath);

        string levelInfo = System.IO.File.ReadAllText(levelInfoPath);
        JsonUtility.FromJsonOverwrite(levelInfo, _config);

        _stopNumber.value = _config.NumStops;
        _stopMins.value = _config.StopMins;

        _sleepHours.value = _config.SleepHours;
        _hourPerSleep.value = _config.HoursPerSleep;

        _location.value = _config.Location;
        _duration.value = _config.Duration;
        _durationMax.value = _config.DurationMax;

        // Paradas concatenamos en un string para mostrar en el TextField
        if (_config.StopsNames != null && _config.StopsNames.Count > 0)
            _stopNames.value = string.Join(",", _config.StopsNames);
        else
            _stopNames.value = "";

        // 24 HORAS (los toggle que están a 12x2)
        int k = 0;
        for (int i = 0; i < 2; i++)
        {
            for (int j = 0; j < 12; j++)
            {
                _depHours[j, i].value = _config.DepartureHours[k];
                _locHours[j, i].value = _config.LocationHours[k];
                _allSleepHours[j, i].value = _config.AllSleepHours[k];
                k++;
            }
        }

    }
}

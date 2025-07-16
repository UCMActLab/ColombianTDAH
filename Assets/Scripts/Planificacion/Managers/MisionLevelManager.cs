using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEngine.Rendering.STP;

public class MisionLevelManager : MonoBehaviour
{
    // Singleton
    static private MisionLevelManager _instance;
    public static MisionLevelManager Instance { get { return _instance; } }

    // UI
    [SerializeField]
    MisionUIManager _misionUIManager;
    [SerializeField]
    MapUIManager _mapUIManager;

    // Time
    [SerializeField]
    float _answerTime = 10; // Seconds
    float _timeCont = 0;
    bool _isAnswering = false;

    // Reglas
    int _stopsN; // Number
    int _stopMins; // Mins
    int _sleepHours; // Hours
    int _locationFrec; // Hours
    int _duration; // Hours
    int _durationMax; // Hours
    bool _rules;

    // Horas
    bool[,] _depHours;
    bool[,] _locHours;
    bool[,] _allSleepHours;
    int _hourPerSleep;
    int _totalDurationMins;
    int _totalSleepHours;

    // Lista de paradas
    List<string> _stops;

    // DECISIONES
    string _startTime;
    List<string> _selectedStops;
    List<string> _selectedSleepTimes;
    List<string> _selectedLocationHours;



    private void Awake()
    {
        // Si no hay instancia de esta clase ya creada se almacena
        if (_instance == null)
            _instance = this;
        // Si esta creada se destruye porque no necesitamos una mas
        else
            Destroy(this.gameObject);

        DontDestroyOnLoad(this.gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _timeCont = _answerTime;
        _totalDurationMins = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Actualiza contador tiempo
        if (_isAnswering) UpdateTime();
    }

    // Activa botones y slider tiempo
    public void ShowDecisionButtons()
    {
        // Show buttons UI
        _misionUIManager.ShowDecisionButtons();
        _isAnswering = true;
    }

    // Desactiva botones y slider tiempo
    void HideDecisionButtons()
    {
        Answered();

        _misionUIManager.HideDecisionButtons();
        _isAnswering = true;
    }

    // Actualiza tiempo
    void UpdateTime()
    {
        if (_timeCont > 0)
        {
            // Contador
            _timeCont -= Time.deltaTime;

            // Update Slider
            _misionUIManager.UpdateSlider(_timeCont);
        }
        else
            HideDecisionButtons();
    }

    // Reestablece contador
    public void Answered()
    {
        _isAnswering = false;
        _timeCont = _answerTime;
    }

    // Devuelve valor del tiempo de respuesta
    public float GetAnswerTime() { return _answerTime; }

    // Guarda referencia UI nivel
    public void RegisterUIManager(MisionUIManager misionUIManager)
    {
        _misionUIManager = misionUIManager;
        _misionUIManager.ChangeTime(_startTime);
    }

    // Carga configuracion escogida
    public void LoadConfiguration(MisionConfigurationData config)
    {
        // Reglas
        _stopsN = config.NumStops;
        _stopMins = config.StopMins;
        _sleepHours = config.SleepHours;
        _locationFrec = config.Location;
        _duration = config.Duration;
        _durationMax = config.DurationMax;

        SetUIRules();

        // Planificacion
        _depHours = config.DepartureHours;
        _locHours = config.LocationHours;
        _allSleepHours = config.AllSleepHours;

        _hourPerSleep = config.HoursPerSleep;

        _stops = config.StopsNames;

        SetUIPlanification();

        CheckRules();
    }

    // Cambia reglas UI
    private void SetUIRules()
    {
        _mapUIManager.SetStops(_stopsN, _stopMins);
        _mapUIManager.SetSleepHours(_sleepHours);
        _mapUIManager.SetLocationFrec(_locationFrec);
    }

    // Cambia posibles opciones mapa
    private void SetUIPlanification()
    {

        List<string> optiondatas = new List<string>();
        List<string> optiondatas2 = new List<string>();
        List<string> optiondatas3 = new List<string>();
        string auxString = "am";

        for (int i = 1; i <= _depHours.GetLength(1); i++)
        {
            for (int j = 1; j <= _depHours.GetLength(0); j++)
            {
                if (_depHours[j - 1, i - 1])
                {
                    optiondatas.Add(j + " " + auxString);
                }

                if (_locHours[j - 1, i - 1])
                {
                    optiondatas2.Add(j + " " + auxString);
                }

                if (_allSleepHours[j - 1, i - 1])
                {
                    optiondatas3.Add(j + " " + auxString);
                }
            }

            auxString = "pm";
        }

        _mapUIManager.SetDepartureHours(optiondatas);
        _mapUIManager.SetLocationHours(optiondatas2);
        _mapUIManager.SetAllSleepHours(optiondatas3);

        _mapUIManager.SetStopsNames(_stops);
    }

    public void SetStartTime(string newTime)
    {
        _startTime = newTime;
    }
    public void SetSelectedStops(List<string> newSelectedStops)
    {
        _selectedStops = newSelectedStops;
        _mapUIManager.SetStopExtraMins(_selectedStops.Count * _stopMins);
    }

    public void SetSelectedSleepTime(List<string> newSelectedSleepTime)
    {
        _totalSleepHours = 0;
        _selectedSleepTimes = newSelectedSleepTime;

        for (int i = 0; i < _selectedSleepTimes.Count; i++)
        {
            if (i != (_selectedSleepTimes.Count - 1))
            {
                int hBetween = GetHoursInBetween(_selectedSleepTimes[i], _selectedSleepTimes[i + 1]);
                if (hBetween < _hourPerSleep)
                    _totalSleepHours += hBetween;
                else
                    _totalSleepHours += _hourPerSleep;
            }
            else
                _totalSleepHours += _hourPerSleep;
        }
        _mapUIManager.SetSleepExtraHours(_totalSleepHours);
    }

    public void SetSelectedLocationHours(List<string> newSelectedLocationHours)
    {
        _selectedLocationHours = newSelectedLocationHours;
    }

    public void CheckRules()
    {
        if (!(_selectedStops == null || _selectedSleepTimes == null || _selectedLocationHours == null))
        {
            // Calculo tiempos totales
            int stopsDuration = _selectedStops.Count * _stopMins; // minutos
            //int sleepDuration = _selectedSleepTimes.Count * _hourPerSleep; // horas
            _totalDurationMins = _duration * 60 + stopsDuration + _totalSleepHours * 60; // minutos
            bool totalTimeCorrect = (_totalDurationMins / 60) < _durationMax;

            // Duracion total en UI
            _mapUIManager.SetTotalTime((_totalDurationMins / 60), (_totalDurationMins % 60), totalTimeCorrect);

            // Comprobacion reglas
            _rules = _stopsN <= _selectedStops.Count && _sleepHours <= _totalSleepHours && totalTimeCorrect;

            // Mensaje aviso reglas UI
            _mapUIManager.SetWarning(!_rules);
        }
    }

    private int GetHoursInBetween(string h1, string h2)
    {
        int diff;
        string[] h1Split = h1.Split(' ');
        string[] h2Split = h2.Split(' ');
        int num1 = int.Parse(h1Split[0]);
        int num2 = int.Parse(h2Split[0]);
        // 1 am 2 am 3 am 4 am 5 am 6 am 7 am 8 am 9 am 10 am 11 am 12 am
        // 1 pm 2 pm 3 pm 4 pm 5 pm 6 pm 7 pm 8 pm 9 pm 10 pm 11 pm 12 pm

        if (h1Split[1] == "am" && h2Split[1] == "pm")
        {
            if (h1Split[0] != h2Split[0])
            {
                diff = 12 - num1 + num2;
            }
            else
                diff = 12;
        }
        else
            diff = num2 - num1;

        Debug.Log("Diff " + h1 + " y " + h2 + ": " + diff);

        return diff;
    }
}

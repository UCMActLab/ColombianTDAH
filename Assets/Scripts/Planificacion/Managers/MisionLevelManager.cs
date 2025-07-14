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
    float _answerTime = 10;
    float _timeCont = 0;
    bool _isAnswering = false;

    // Reglas
    int _stopsN;
    int _stopMins;
    int _sleepHours;
    int _locationFrec;
    bool _rules;

    // Horas
    bool[,] _depHours;
    bool[,] _locHours;
    bool[,] _allSleepHours;
    int _hourPerSleep;

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

        SetUIRules();

        // Planificacion
        _depHours = config.DepartureHours;
        _locHours = config.LocationHours;
        _allSleepHours = config.AllSleepHours;

        _hourPerSleep = config.HoursPerSleep;

        _stops = config.StopsNames;

        SetUIPlanification();
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
        _selectedSleepTimes = newSelectedSleepTime;
        _mapUIManager.SetSleepExtraHours(_selectedSleepTimes.Count * _hourPerSleep);
    }

    public void SetSelectedLocationHours(List<string> newSelectedLocationHours)
    {
        _selectedLocationHours = newSelectedLocationHours;
    }

    public void CheckRules()
    {
        Debug.Log("Checkeando");
        // si tiene seleccionadas paradas minimas
        if (!(_selectedStops == null || _selectedSleepTimes == null || _selectedLocationHours == null))
        {
            _rules = _stopsN <= _selectedStops.Count && _sleepHours <= (_selectedSleepTimes.Count * _hourPerSleep);
            Debug.Log("Checkeado" +_rules);
            _mapUIManager.SetWarning(_rules);
        }

    }

}

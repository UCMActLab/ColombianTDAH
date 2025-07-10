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

    // Horas
    bool [,] _depHours;
    bool[,] _locHours;

    private void Awake()
    {
        // Si no hay instancia de esta clase ya creada se almacena
        if (_instance == null)
            _instance = this;
        // Si esta creada se destruye porque no necesitamos una mas
        else
            Destroy(this.gameObject);
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
    public float GetAnswerTime() {  return _answerTime; }

    // Carga configuracion escogida
    public void LoadConfiguration(MisionConfigurationData config)
    {
         _stopsN = config.NumStops;
         _stopMins = config.StopMins;
         _sleepHours = config.SleepHours;
         _locationFrec = config.Location;

        SetUIRules();

        _depHours = config.DepartureHours;
        _locHours = config.LocationHours;

        SetUIPlanification();
    }

    // Cambia reglas UI
    private void SetUIRules()
    {
        _mapUIManager.SetStops(_stopsN, _stopMins);
        _mapUIManager.SetSleepHours(_sleepHours);
        _mapUIManager.SetLocationFrec(_locationFrec);
    }

    private void SetUIPlanification()
    {

        List<string> optiondatas = new List<string>();
        List<string> optiondatas2 = new List<string>();
        string auxString = "am";

        for (int i = 1; i <= _depHours.GetLength(1); i++)
        {
            for (int j = 1; j <= _depHours.GetLength(0); j++)
            {
                if (_depHours[j - 1, i - 1])
                {
                    optiondatas.Add(j + " " + auxString);
                }

                if (_locHours[j - 1, i - 1]){
                    optiondatas2.Add(j + " " + auxString);
                }
            }

            auxString = "pm";
        }

        _mapUIManager.SetDepartureHours(optiondatas);
        _mapUIManager.SetLocationHours(optiondatas2);
    }
}

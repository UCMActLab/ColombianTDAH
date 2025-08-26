using System;
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

    // Audio
    [SerializeField]
    SoundManager _soundManager;

    // Time
    int _freeTime; // Tiempo que no esta durmiendo 
    float _playTimeCont = 0;
    float _auxCont = 0; // Contador aparicion paradas en ejecucion
    float _sleepFade = 2;
    float _sleepAuxCont = 0;
    float _realSegsPerStop = 7.5f;
    HourMinSec _gameClock;
    HourMinSec _auxGameCont;

    // Estados
    bool _paused = true;
    bool _sleeping = false;

    // Ask Time
    float _answerTime = 10; // Seconds
    float _timeCont = 0;
    bool _isAnswering = false;
    int _questionFrecMins;

    // Reglas
    int _stopsN; // Number
    int _stopMins; // Mins
    int _sleepHours; // Hours
    int _locationFrec; // Hours
    int _duration; // Hours
    int _durationMax; // Hours
    bool _rules;

    // Horas
    bool[] _depHours;
    bool[] _locHours;
    bool[] _allSleepHours;
    int _hourPerSleep;
    int _totalDurationMins;
    int _totalSleepHours;
    bool _locMessageCorrect;

    // Lista de paradas
    List<string> _stops;

    // DECISIONES
    string _startTime;
    List<string> _selectedStops;
    List<string> _selectedSleepTimes;
    List<HourMinSec> selectedSleepTimes;
    List<string> _selectedLocationHours;
    List<HourMinSec> selectedLocationHours;

    // Preguntas
    Dictionary<string, string> _questions = new Dictionary<string, string>();

    // Game Objects
    [SerializeField]
    GameObject _map;

    [SerializeField]
    GameObject _dialogs;

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
        _auxGameCont = new HourMinSec();

        // Listas
        selectedLocationHours = new List<HourMinSec>();
        selectedSleepTimes = new List<HourMinSec>();

        // Cambia imagen del mapa dependiendo del nivel
        SetMapImage(SceneLoader.Instance.getCurrentLevelId(EventRegister.TipoJuego.MisionColombia));

        // Settea juego
        EventRegister.InfoSesion infoSesion = EventRegister.Instance.GetInfoSesion();
        infoSesion.nombreJuego = EventRegister.TipoJuego.MisionColombia;
        EventRegister.Instance.SetInfoSesion(infoSesion);

    }

    // Update is called once per frame
    void Update()
    {

        if (!_paused)
        {
            // Actualiza contador tiempo
            if (_isAnswering) UpdateTime();

            UpdateClock();

        }
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
        string mensaje = "Pregunta final, no se ha contestado";
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.TerminaDecision, mensaje));
        EventRegister.Instance.EvntToJson();

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

    void UpdateClock()
    {
        // Si no est durmiendo
        if (!_sleeping)
        {

            _playTimeCont += Time.deltaTime;

            if (_auxCont >= _realSegsPerStop)
            {

                _gameClock += new HourMinSec(0, _stopMins, 0); // Reloj juego
                int gameMins = _auxGameCont.Minutes + _stopMins;
                _auxGameCont = new HourMinSec(0, gameMins, 0); // Aumento contador
                Debug.Log(_auxGameCont.Minutes);
                _misionUIManager.ChangeTime(_gameClock); // Cambio en UI

                _auxCont = 0; // Reinicio contador

                // Actualizo dormir
                UpdateSleep();

                if (!_sleeping)
                {

                    if (_auxGameCont.Minutes >= _questionFrecMins)
                    {
                        Question(); // Activa pregunta buena

                        _auxGameCont = new HourMinSec(); // Reinicio contador
                    }
                }
            }
            else
                _auxCont += Time.deltaTime;
        }
        // Si duerme
        else
        {
            if (_sleepAuxCont >= _sleepFade)
            {
                Sleep(false);
                _sleepAuxCont = 0;
            }
            else
            {
                _sleepAuxCont += Time.deltaTime;
                _misionUIManager.SetSleepImageAlpha(255 * _sleepAuxCont / _sleepFade);
            }
        }
    }

    void UpdateSleep()
    {
        // si la hora en la que estamos esta en la lista de dormir pongo a true booleano dormir

        for (int i = 0; i < selectedSleepTimes.Count && !_sleeping; i++)
        {
            _sleeping = (selectedSleepTimes[i].Hours == _gameClock.Hours);
        }

        if (_sleeping) Sleep(true);
    }

    void Sleep(bool enabled)
    {
        _sleeping = enabled;

        if (enabled)
        {
            _gameClock += new HourMinSec(_hourPerSleep, 0, 0);
            Debug.Log("Activo dormir");
        }
        else
        {

            Debug.Log("Desactivo dormir");
        }
        _misionUIManager.SetSleepImageAlpha(0);
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
        _misionUIManager.SetStartTime(_gameClock);
        _misionUIManager.SetStops(_selectedStops);
        _misionUIManager.SetSleepHours(_selectedSleepTimes);
        _misionUIManager.SetLocationHours(_selectedLocationHours);
    }

    // Carga las preguntas de las paradas
    public void LoadQuestions(Dictionary<string, string> q)
    {
        _questions = q;
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

        // Ejecucion
        _answerTime = config.AnswerTime;
        _timeCont = _answerTime;
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

        for (int i = 0; i < _depHours.Length; i++)
        {

            HourMinSec auxTime = new HourMinSec(i, 0, 0);
            string auxString = auxTime.GetHString();

            if (_depHours[i])
            {

                optiondatas.Add(auxString);
            }

            if (_locHours[i])
            {

                optiondatas2.Add(auxString);
            }

            if (_allSleepHours[i])
            {
                optiondatas3.Add(auxString);

            }

        }

        _mapUIManager.SetDepartureHours(optiondatas);
        _mapUIManager.SetLocationHours(optiondatas2);
        _mapUIManager.SetAllSleepHours(optiondatas3);

        _mapUIManager.SetStopsNames(_stops);
    }

    // Set Time. "X am/X pm"
    public void SetStartTime(string newTime)
    {
        _startTime = newTime;
        string[] timeSplit = newTime.Split(' ');
        int aux = 0;
        if (timeSplit[1] == "pm")
            aux = 12;
        string auxString = timeSplit[0];
        _gameClock = new HourMinSec(int.Parse(auxString) + aux, 0, 0);
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

        selectedSleepTimes.Clear();
        for (int i = 0; i < newSelectedSleepTime.Count; i++)
        {
            selectedSleepTimes.Add(new HourMinSec(newSelectedSleepTime[i]));
        }


        CalculateSleepHours();

        _mapUIManager.SetSleepExtraHours(_totalSleepHours);
    }

    public void SetSelectedLocationHours(List<string> newSelectedLocationHours)
    {
        _selectedLocationHours = newSelectedLocationHours;

        selectedLocationHours.Clear();
        for (int i = 0; i < newSelectedLocationHours.Count; i++)
        {
            selectedLocationHours.Add(new HourMinSec(newSelectedLocationHours[i]));
        }
    }

    public void CheckRules()
    {
        if (!(_selectedStops == null || _selectedSleepTimes == null || _selectedLocationHours == null))
        {
            // Calculo tiempos totales
            int stopsDuration = _selectedStops.Count * _stopMins; // minutos
            _totalDurationMins = _duration * 60 + stopsDuration + _totalSleepHours * 60; // minutos
            bool totalTimeCorrect = (_totalDurationMins / 60) < _durationMax;

            // Aviso ubicacion cumple con la hora de salida y la duracion
            bool totalLocMessCorrect = CheckLocationRules();

            // Duracion total en UI
            _mapUIManager.SetTotalTime((_totalDurationMins / 60), (_totalDurationMins % 60), totalTimeCorrect);

            // Comprobacion reglas
            _rules = _stopsN <= _selectedStops.Count && _sleepHours <= _totalSleepHours && totalTimeCorrect && totalLocMessCorrect;

            // Mensaje aviso reglas UI
            _mapUIManager.SetWarning(!_rules);
        }
    }

    // Comprueba reglas de mensaje de ubicacion
    private bool CheckLocationRules()
    {
        _locMessageCorrect = true;
        int i = 0;
        while (i < selectedLocationHours.Count - 1 && _locMessageCorrect)
        {
            int hBetween = selectedLocationHours[i].GetHoursInBetween(selectedLocationHours[i + 1].Hours);
            if (hBetween > _locationFrec)
                _locMessageCorrect = false;

            i++;
        }

        // Comprueba horas de inicio y final tambien
        bool totalLocMessCorrect = _locMessageCorrect;

        if (_locMessageCorrect)
        {
            int nElem = selectedLocationHours.Count;
            if (0 < nElem)
            {
                // True si de la hora de inicio hasta el primer aviso de ubicacion y desde el ultimo aviso hasta el final hay menos de la frecuencia de aviso
                HourMinSec startTime = new HourMinSec(_startTime);
                totalLocMessCorrect = (startTime.GetHoursInBetween(selectedLocationHours[0].Hours)) <= _locationFrec && (_totalDurationMins - 60 * (startTime.GetHoursInBetween(selectedLocationHours[nElem - 1].Hours)) <= (_locationFrec * 60));

            }
            else
                totalLocMessCorrect = _totalDurationMins <= _locationFrec * 60;
        }

        return totalLocMessCorrect;
    }

    // Calcula el tiempo total escogido para dormir
    private void CalculateSleepHours()
    {
        for (int i = 0; i < selectedSleepTimes.Count; i++)
        {
            if (i != (selectedSleepTimes.Count - 1))
            {
                int hBetween = selectedSleepTimes[i].GetHoursInBetween(selectedSleepTimes[i + 1].Hours);
                if (hBetween < _hourPerSleep)
                    _totalSleepHours += hBetween;
                else
                    _totalSleepHours += _hourPerSleep;
            }
            else
                _totalSleepHours += _hourPerSleep;
        }

    }

    public void AcceptPlanning()
    {
        _paused = false;
        _soundManager.Click();
        CalculateQuestionFrec();
    }

    public void ActivateGame()
    {
        EventRegister.Instance.AddInitialEvent(EventRegister.EventosInfo.Inicio, "nivel " + SceneLoader.Instance.getCurrentLevelId(EventRegister.TipoJuego.MisionColombia).ToString("00"), EventRegister.TipoJuego.MisionColombia);
        Debug.Log("se pudo iniciar el evento Inicio en MisionLevelManager.");

        _map.SetActive(true);
        _dialogs.SetActive(true);
    }

    // Cambia imagen del mapa
    public void SetMapImage(int index)
    {
        _mapUIManager.SetMapImage(index - 1);
    }

    public void Pause(bool enabled)
    {
        Debug.Log("Pauso que voy ardiendooo~");

        _paused = enabled;
    }

    public void ExitLevel()
    {
        Destroy(gameObject);
        _instance = null;
    }

    // Calcula cada cuanto deber aparecer una pregunta
    void CalculateQuestionFrec()
    {
        _freeTime = _duration - _totalSleepHours; // Tiempo que no esta dormido
        _questionFrecMins = (_freeTime / _questions.Count) * 60;

        Debug.Log("Pregunta buena cada: " + _questionFrecMins);
    }

    // Hace que aparezca una pregunta en pantalla
    void Question()
    {
        string mensaje = "Pregunta inicio";
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.EmpiezaDecision, mensaje));
        EventRegister.Instance.EvntToJson();
        // Cambio texto de pregunta

        // Aparece pregunta con botones de decision
        ShowDecisionButtons();
        Debug.Log("Aparece pregunta buena para responder"); // Tiene que parar
    }
}

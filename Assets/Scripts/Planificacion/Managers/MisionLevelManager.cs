using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.InputSystem;
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
    float _sleepFade = 5;
    float _sleepAuxCont = 0;
    float _realSegsPerStop = 1.5f;
    HourMinSec _gameClock;
    HourMinSec _auxGameCont;
    int _auxHourClock;

    // Estados
    bool _paused = true;
    bool _shouldSleep = false;
    bool _sleeping = false;
    bool _anyQuestionsLeft = true;
    bool _isSelectedStopQuestion;

    // Ask Time
    float _answerTime = 10; // Seconds
    float _timeCont = 0;
    bool _isAnswering = false;
    int _questionFrecMins;
    int _goodStopAnswers = 0;
    int _badStopAnswers = 0;
    int _goodSentLocatiton = 0;
    int _indexUIQuestion;

    // Reglas
    int _stopsN; // Number
    int _stopMins; // Mins
    int _sleepHours; // Hours
    int _locationFrec; // Hours
    int _duration; // Hours
    int _durationMax; // Hours
    bool _rules;
    HourMinSec _lastSentHour;
    bool _isLocationSentGood = true;

    // Horas
    bool[] _depHours;
    bool[] _locHours;
    bool[] _allSleepHours;
    int _hourPerSleep;
    int _totalDurationMins;
    int _totalSleepHours; //horas que pusiste en la planificacion
    int _sleptHours; //horas que llevas dormidas
    bool _locMessageCorrect;
    HashSet<int> _askedSleepTimes = new HashSet<int>(); //para no repetir la pregunta de sue�o en horas que ya se han hecho

    // Lista de paradas
    List<string> _stops;

    // DECISIONES
    string _startTime;
    List<string> _selectedInitialStops; // Paradas seleccionadas por el jugador al inicio
    List<string> _selectedStops; // Paradas seleccionadas por el jugador
    List<string> _selectableStops; // Paradasa que puede seleccionar el jugador
    List<string> _distractionStops; // Paradas para distraer
    List<string> _selectedSleepTimes;
    List<HourMinSec> selectedSleepTimes;
    List<string> _selectedLocationHours;
    List<HourMinSec> selectedLocationHours;
    List<HourMinSec> _locHoursList;

    // Preguntas
    Dictionary<string, string> _questions = new Dictionary<string, string>();

    // Game Objects
    [SerializeField]
    GameObject _map;

    [SerializeField]
    GameObject _dialogs;

    // Random num
    System.Random rnd = new System.Random();


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
            else UpdateClock();

            GoToResumenScreen();

            isLevelFinished();
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

        FinishAnswer(); // Sin contestar
        BadAnswer();

        _misionUIManager.HideDecisionButtons();
        _isAnswering = false;
    }

    void SetVignette()
    {
        _sleptHours -= _hourPerSleep; //si no duerme se restan horas de suenyo

        float sleepRatio = (float)_sleptHours / (float)_totalSleepHours;
        float vignetteAlpha = 1f - sleepRatio;
        _misionUIManager.SetSleepVignette(vignetteAlpha);
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
        if (!_shouldSleep && !_sleeping)
        {

            _playTimeCont += Time.deltaTime;

            if (_auxCont >= _realSegsPerStop)
            {

                _gameClock += new HourMinSec(0, _stopMins, 0); // Reloj juego
                int gameMins = _auxGameCont.Minutes + _stopMins;
                _auxGameCont = new HourMinSec(0, gameMins, 0); // Aumento contador
                //Debug.Log(_auxGameCont.Minutes);
                _misionUIManager.ChangeTime(_gameClock); // Cambio en UI

                _auxCont = 0; // Reinicio contador

                // Actualizo dormir
                UpdateSleep();

                if (!_shouldSleep && !_sleeping && _anyQuestionsLeft)
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

            UpdateLocation();
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
                // Fade in / Fade Out
                if (_sleepAuxCont >= _sleepFade * 2 / 3)
                    _misionUIManager.SetSleepImageAlpha((_sleepFade - _sleepAuxCont) / (_sleepFade * 2 / 3));
                else if (_sleepAuxCont < _sleepFade / 3)
                    _misionUIManager.SetSleepImageAlpha(_sleepAuxCont / (_sleepFade / 3));
                else
                    _misionUIManager.ChangeTime(_gameClock); // Cambio en UI

                _sleepAuxCont += Time.deltaTime;
            }
        }
    }

    void UpdateLocation()
    {
        // Si ha cambiado la hora
        if (_auxHourClock != _gameClock.Hours)
        {
            // Comprueba si se cumple la regla
            CheckLocationSent();

            // Si es igual o menor sinifica q se ha saltado la hora de mandar ubicacion
            while (_locHoursList.Count != 0 && _auxHourClock >= _locHoursList[0].Hours)
            {
                // Pongo cross en UI
                Debug.Log("Aux Hour Clock: " + _auxHourClock + " Comprobadno y eliminando: " + _locHoursList[0].GetHString());
                _misionUIManager.SetLocationTick(_selectedLocationHours.IndexOf(_locHoursList[0].GetHString()), false);

                // Elimino hora de la lista
                _locHoursList.RemoveAt(0);
            }

            // Actualizo e igualo auxde reloj
            _auxHourClock = _gameClock.Hours;
        }
    }

    void UpdateSleep()
    {
        // si la hora en la que estamos esta en la lista de dormir pongo a true booleano dormir
        if (_askedSleepTimes.Contains(_gameClock.Hours))
        {
            return;
        }

        for (int i = 0; i < selectedSleepTimes.Count && !_shouldSleep; i++)
        {
            _shouldSleep = (selectedSleepTimes[i].Hours == _gameClock.Hours);

        }

        if (_shouldSleep)
        {
            _askedSleepTimes.Add(_gameClock.Hours);
            SleepQuestion();
        }

    }

    void Sleep(bool enabled)
    {
        Debug.Log("Sleeping enabled " + enabled);
        _sleeping = enabled;


        if (enabled)
        {
            int index = _selectedSleepTimes.IndexOf(_gameClock.GetHString());

            Debug.Log("Index sleep: " + _selectedSleepTimes[index]);
            _misionUIManager.SetSleepTick(index, true);

            // Sumo horas dormidas
            _gameClock += new HourMinSec(_hourPerSleep, 0, 0);
            Debug.Log("Activo dormir");
        }
        else
        {

            Debug.Log("Desactivo dormir");
            _misionUIManager.SetSleepImageAlpha(0);
        }
    }
    void SleepQuestion()
    {
        string mensaje = "Pregunta inicio";
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.EmpiezaDecision, mensaje));
        EventRegister.Instance.EvntToJson();


        int randomNum = 0;
        // Busco pregunta en seleccionadas
        if (randomNum == 0)
        {

            _misionUIManager.ChangeQuestion($"Quieres dormir {_hourPerSleep} horas?");
        }

        ShowDecisionButtons();
        Debug.Log("Aparece pregunta buena para responder dormir"); // Tiene que parar
    }

    // Reestablece contador
    public void Answered(bool yes)
    {
        FinishAnswer();

        if (_shouldSleep) //si esta _shouldSleep es que es una pregunta de dormir si o no
        {

            if (yes)
                _misionUIManager.SetSleepVignette(0); //se resetea 
            else
                BadAnswer();

            Sleep(yes);
            _shouldSleep = false;
        }
        else
        {
            if (yes == _isSelectedStopQuestion)
            {
                if (yes)
                    _goodStopAnswers++;

                GoodAnswer();
            }
            else
                BadAnswer();
        }

    }

    private void FinishAnswer()
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
        _selectedInitialStops = new List<string>(_selectedStops);
        _misionUIManager.SetStops(_selectedStops);
        _misionUIManager.SetStartTime(_gameClock);
        _misionUIManager.SetSleepHours(_selectedSleepTimes);
        _misionUIManager.SetLocationHours(_selectedLocationHours);
        _locHoursList = new List<HourMinSec>(selectedLocationHours);
    }

    // Carga las preguntas de las paradas
    public void LoadQuestions(Dictionary<string, string> q, List<string> distractionStops, List<string> selectableStops)
    {
        _questions = q;
        _distractionStops = distractionStops;
        _selectableStops = selectableStops;

        _mapUIManager.SetStopsNames(_selectableStops);
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
        _lastSentHour = new HourMinSec(_gameClock.Hours, _gameClock.Minutes, _gameClock.Seconds);
        _auxHourClock = _gameClock.Hours;
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

        _sleptHours = _totalSleepHours; //empiezan siendo las totales y luego se restan si no duermes

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

        int randomNum;
        if (_selectedStops.Count > 0 && _distractionStops.Count > 0)
        {
            randomNum = rnd.Next(0, 2);
        }
        // Si no quedan de las paradas seleccionadas
        else if (_selectedStops.Count == 0)
        {
            randomNum = 1;
        }
        // Si no quedan de las paradas distractoras
        else
        {
            randomNum = 0;
        }

        // Busco pregunta en seleccionadas
        if (randomNum == 0)
        {
            _isSelectedStopQuestion = true;
            randomNum = rnd.Next(0, _selectedStops.Count);

            // Cambio texto de pregunta
            _misionUIManager.ChangeQuestion(_questions[_selectedStops[randomNum]]);
            _indexUIQuestion = _selectedInitialStops.IndexOf(_selectedStops[randomNum]); // index
            _selectedStops.Remove(_selectedStops[randomNum]);

        }
        // Busco pregunta en distractoras
        else
        {
            _isSelectedStopQuestion = false;
            randomNum = rnd.Next(0, _distractionStops.Count);

            // Cambio texto de pregunta
            _misionUIManager.ChangeQuestion(_questions[_distractionStops[randomNum]]);

            // Borro parada realizada
            _distractionStops.Remove(_distractionStops[randomNum]);
        }

        // Compuebo si no quedan preguntas
        if (_selectedStops.Count == 0 && _distractionStops.Count == 0)
        {
            _anyQuestionsLeft = false;
            Debug.Log("Ya no hay m�s preguntas");
        }

        // Aparece pregunta con botones de decision
        ShowDecisionButtons();
        Debug.Log("Aparece pregunta buena para responder"); // Tiene que parar
    }

    private void GoodAnswer()
    {
        if (_isSelectedStopQuestion)
            _misionUIManager.SetStopTick(_indexUIQuestion, true);

        Debug.Log("Good answer");
    }

    private void BadAnswer()
    {
        if (_shouldSleep)
        {
            SetVignette();
            int index = _selectedSleepTimes.IndexOf(_gameClock.GetHString());
            _misionUIManager.SetSleepTick(index, false);
            _shouldSleep = false;
        }
        else
        {
            _badStopAnswers++;

            if (_isSelectedStopQuestion)
                _misionUIManager.SetStopTick(_indexUIQuestion, false);
        }
        Debug.Log("Bad answer");
    }

    public void SendLocation()
    {
        // Comprueba si se cumple la regla
        CheckLocationSent();

        // Guarda ultima hora de envio
        _lastSentHour.Hours = _gameClock.Hours;
        _lastSentHour.Minutes = _gameClock.Minutes;
        _lastSentHour.Seconds = _gameClock.Seconds;

        // Actualiza UI
        if (_locHoursList.Count != 0 && _gameClock.Hours == _locHoursList[0].Hours)
        {
            // Pongo tick en UI
            _misionUIManager.SetLocationTick(_selectedLocationHours.IndexOf(_locHoursList[0].GetHString()), true);
            _locHoursList.RemoveAt(0);
            _goodSentLocatiton++;
        }
    }

    void CheckLocationSent()
    {
        // Guarda ultima hora de envio
        int hourDiff = _lastSentHour.GetHoursInBetween(_gameClock.Hours);

        // Mira si se cumple la regla
        if (hourDiff > _locationFrec)
            _isLocationSentGood = false;
    }

    void GoToResumenScreen()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneLoader.LoadScene("MC_Resumen");
        }
    }
    void isLevelFinished()
    {
        if (_gameClock.Hours == 0)
        {
            Debug.Log("terminando nivel");
            SceneLoader.LoadScene("MC_Resumen");
        }
    }

    public bool Win()
    {
        string[] timeSplit = _startTime.Split(' ');
        int aux = 0;
        if (timeSplit[1] == "pm")
            aux = 12;
        string auxString = timeSplit[0];

        int realDuration = new HourMinSec(int.Parse(auxString) + aux, 0, 0).GetHoursInBetween(_gameClock.Hours);

        return _goodStopAnswers >= _stopsN && _sleptHours >= _totalSleepHours && _isLocationSentGood && realDuration < _durationMax;
        // Si no se pasa del tiempo maximo
    }


    public int HourPerSleep => _hourPerSleep;
    public int SleptHours => _sleptHours;
    public int TotalSleepHours => _totalSleepHours;
    public string StartTime => _startTime;
    public List<string> SelectedInitialStops => _selectedInitialStops;
    public List<string> SelectedStops => _selectedStops;
    public int GoodStopAnswers => _goodStopAnswers;
    public List<string> SelectedLocationHours => _selectedLocationHours;
    public int GoodSentLocation => _goodSentLocatiton;


}

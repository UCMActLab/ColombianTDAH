using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MisionConfigurationData", menuName = "Scriptable Objects/MisionConfigurationData")]
[System.Serializable]
public class MisionConfigurationData : ScriptableObject
{
    //ID
    public string configName;

    // REGLAS
    // Paradas
    [SerializeField]
    private int _nStops;
    public int NumStops { get => _nStops; set => _nStops = value; }

    [SerializeField]
    private int _stopMins;
    public int StopMins { get => _stopMins; set => _stopMins = value; }

    // Dormir
    [SerializeField]
    private int _sleepHours;
    public int SleepHours { get => _sleepHours; set => _sleepHours = value; }

    // Ubicacion
    [SerializeField]
    private int _location;
    public int Location { get => _location; set => _location = value; }

    // Duracion
    [SerializeField]
    private int _duration;
    public int Duration { get => _duration; set => _duration = value; }

    [SerializeField]
    private int _durationMax;
    public int DurationMax { get => _durationMax; set => _durationMax = value; }

    // PLANIFICACION
    // Matriz horas salida
    [SerializeField]
    private bool[] _departureHours = new bool[24];
    public bool[] DepartureHours { get => _departureHours; set => _departureHours = value; }

    // Matriz horas ubicacion
    [SerializeField]
    private bool[] _locationHours = new bool[24];
    public bool[] LocationHours { get => _locationHours; set => _locationHours = value; }

    // Matriz horas dormir
    [SerializeField]
    private bool[] _allSleepHours = new bool[24];
    public bool[] AllSleepHours { get => _allSleepHours; set => _allSleepHours = value; }

    [SerializeField]
    private int _hoursPerSleep;
    public int HoursPerSleep { get => _hoursPerSleep; set => _hoursPerSleep = value; }

    // Paradas lista
    [SerializeField]
    List<string> _stopsNames;
    public List<string> StopsNames { get => _stopsNames; set => _stopsNames = value; }

    // EJECUCION
    // Tiempo respuesta
    [SerializeField]
    private int _answerTime;
    public int AnswerTime { get => _answerTime; set => _answerTime = value; }

    // Preguntas
    public SerializableDictionary<string, string> Questions = new SerializableDictionary<string, string>();

    [SerializeField]
    List<string> _distractionStops = new List<string>();
    public List<string> DistractionStops { get => _distractionStops; set => _distractionStops = value; }


    // Desbloqueo
    [SerializeField]
    private bool _desbloqueado;
    public bool Desbloqueado { get => _desbloqueado; set => _desbloqueado = value; }
}

using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "MisionConfigurationData", menuName = "Scriptable Objects/MisionConfigurationData")]
public class MisionConfigurationData : ScriptableObject
{
    //ID
    public string configName;

    // REGLAS
    // Paradas
    private int _nStops;
    public int NumStops { get => _nStops; set => _nStops = value; }

    private int _stopMins;
    public int StopMins { get => _stopMins; set => _stopMins = value; }
 
    // Dormir
    private int _sleepHours;
    public int SleepHours { get => _sleepHours; set => _sleepHours = value; }

    // Ubicacion
    private int _location;
    public int Location { get => _location; set => _location = value; }

    // Duracion
    private int _duration;
    public int Duration { get => _duration; set => _duration = value; }

    private int _durationMax;
    public int DurationMax { get => _durationMax; set => _durationMax = value; }

    // PLANIFICACION
    // Matriz horas salida
    private bool[,] _departureHours = new bool[12, 2];
    public bool[,] DepartureHours { get => _departureHours; set => _departureHours = value; }

    // Matriz horas ubicacion
    private bool[,] _locationHours = new bool[12, 2];
    public bool[,] LocationHours { get => _locationHours; set => _locationHours = value; }

    // Matriz horas dormir
    private bool[,] _allSleepHours = new bool[12, 2];
    public bool[,] AllSleepHours { get => _allSleepHours; set => _allSleepHours = value; }

    private int _hoursPerSleep;
    public int HoursPerSleep { get => _hoursPerSleep; set => _hoursPerSleep = value; }

    // Paradas lista
    List<string> _stopsNames;
    public List<string> StopsNames { get => _stopsNames; set => _stopsNames = value; }

    // EJECUCION
    // Tiempo respuesta
    private int _answerTime;
    public int AnswerTime { get => _answerTime; set => _answerTime = value; }


    //[Header("Level unblocked:")]
    //[SerializeField]
    //private bool _desbloqueado;
    //public bool Desbloqueado { get => _desbloqueado; set => _desbloqueado = value; }
}

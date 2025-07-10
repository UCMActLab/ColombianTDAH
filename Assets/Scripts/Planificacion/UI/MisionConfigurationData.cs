using UnityEngine;

[CreateAssetMenu(fileName = "MisionConfigurationData", menuName = "Scriptable Objects/MisionConfigurationData")]
public class MisionConfigurationData : ScriptableObject
{
    //ID
    public string configName;

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

    // Matriz horas salida
    bool[,] _departureHours = new bool[12, 2];
    public bool[,] DepartureHours { get => _departureHours; set => _departureHours = value; }

    // Matriz horas ubicacion
    bool[,] _locationHours = new bool[12, 2];
    public bool[,] LocationHours { get => _locationHours; set => _locationHours = value; }


    //[Header("Level unblocked:")]
    //[SerializeField]
    //private bool _desbloqueado;
    //public bool Desbloqueado { get => _desbloqueado; set => _desbloqueado = value; }
}

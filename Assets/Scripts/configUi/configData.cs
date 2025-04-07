using JetBrains.Annotations;
using UnityEngine;

[CreateAssetMenu(fileName = "configData", menuName = "Scriptable Objects/configData")]

[System.Serializable]
public class configData : ScriptableObject
{
    //ID
    public string configName;


    //RIVER CONFIG
    [Header("River Configuration")]
    [Range(1, 6)]
    [SerializeField, Tooltip("River rails")]
    private int _nCarriles; 
    public int NumCarriles { get => _nCarriles; set => _nCarriles = value; }

    [Range(1, 10)]
    [SerializeField, Tooltip("Number of dolphins")]
    private int _nDelfines; 
    public int NumDelfines { get => _nDelfines; set => _nDelfines = value; }

    [SerializeField, Tooltip("Positions of dolphins in the river (length, depth)")]
    private Vector2[] _posDelfines;
    public Vector2[] PosDelfines { get => _posDelfines; set => _posDelfines = value; }


    //LEVEL PARAM CONFIG
    [Header("Obstacles Spawning")]
    [SerializeField]
    private bool _obstaclesEnabled;
    public bool ObstaclesEnabled { get => _obstaclesEnabled; set => _obstaclesEnabled = value; }
    [SerializeField, Tooltip("Time for object spawning")]
    private float _minObstacleSpawn;
    public float MinObstacleSpawn { get => _minObstacleSpawn; set => _minObstacleSpawn = value; }

    [SerializeField]
    private float _maxObstacleSpawn;
    public float MaxObstacleSpawn { get => _maxObstacleSpawn; set => _maxObstacleSpawn = value; }

    [Range(1, 100)]
    [SerializeField, Tooltip("Speed for object spawning")]
    private float _obstacleSpeed;
    public float ObstacleSpeed { get => _obstacleSpeed; set => _obstacleSpeed = value; }

    [Range(0.1f, 100)]
    [SerializeField, Tooltip("Increased speed sactor")]
    private float _increasedSpeedFactor;
    public float IncreasedSpeedFactor { get => _increasedSpeedFactor; set => _increasedSpeedFactor = value; }

    [SerializeField, Tooltip("Whale apearing right guests")]
    private int _whaleApearingGuests;
    public int WhaleApearingGuests { get => _whaleApearingGuests; set => _whaleApearingGuests = value; }

    [Header("Floats Spawning")]
    [SerializeField]
    private bool _floatsEnabled;
    public bool FloatsEnabled { get => _floatsEnabled; set => _floatsEnabled = value; }


    [Header("Jumping Details")]
    [SerializeField, Tooltip("Time between jumps")] 
    private float _minTimeBetweenJumps;
    public float MinTimeBetweenJumps { get => _minTimeBetweenJumps; set => _minTimeBetweenJumps = value; }

    [SerializeField]
    private float _maxTimeBetweenJumps;
    public float MaxTimeBetweenJumps { get => _maxTimeBetweenJumps; set => _maxTimeBetweenJumps = value; }

    [SerializeField, Tooltip("Can dolphins special jump at the same time?")]
    public bool canSpecialJumpSimultaneously;

    [SerializeField, Tooltip("Count between special jumps")] 
    private float _minCountBetweenSpecialJumps;
    public float MinCountBetweenSpecialJumps { get => _minCountBetweenSpecialJumps; set => _minCountBetweenSpecialJumps = value; }

    [SerializeField]
    private float _maxCountBetweenSpecialJumps;
    public float MaxCountBetweenSpecialJumps { get => _maxCountBetweenSpecialJumps; set => _maxCountBetweenSpecialJumps = value; }



    [Header("Points")]
    [SerializeField, Tooltip("Necessary points to end level")]
    private float _levelPoints;

    public float LevelPoints { get => _levelPoints; set => _levelPoints = value; }
    
    [SerializeField, Tooltip("Points to add per right special jump guess")]
    private float _rightGuessPoints; //acertar pirueta
    public float RightGuessPoints { get => _rightGuessPoints; set => _rightGuessPoints = value; }

    [SerializeField, Tooltip("Points to add per right special jump guess with increased velocity")]
    private float _rightGuessPointsVel; //acertar pirueta con velocidad aumentada
    public float RightGuessPointsVel { get => _rightGuessPointsVel; set => _rightGuessPointsVel = value; }


    [SerializeField, Tooltip("Points to add per floatie")]
    private float _floatiePoints; //acertar en flotador
    public float FloatiePoints { get => _floatiePoints; set => _floatiePoints = value; }


    [SerializeField, Tooltip("Points to add per floatie with increased velocity")]
    private float _floatiePointsVel; //acertar en flotador con velocidad aumentada
    public float FloatiePointsVel { get => _floatiePointsVel; set => _floatiePointsVel = value; }

    [SerializeField, Tooltip("Points to substract per wrong special jump guess")]
    private float _wrongGuessPoints;
    public float WrongGuessPoints { get => _wrongGuessPoints; set => _wrongGuessPoints = value; }

    [SerializeField, Tooltip("Points to substract when a dolphin hits an obstacle")]
    private float _hitObstaclePoints; //chocarse con obstáculo
    public float HitObstaclePoints { get => _hitObstaclePoints; set => _hitObstaclePoints = value; }

}

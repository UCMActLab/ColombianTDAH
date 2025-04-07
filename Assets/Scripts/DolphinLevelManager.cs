using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Box { Empty, Dolphin, Obstacle, Floatie } // Tipos de objetos representados en la matriz del río

public class DolphinLevelManager : MonoBehaviour
{
    // Singleton
    static private DolphinLevelManager _instance;
    public static DolphinLevelManager Instance { get { return _instance; } }

    // RIVER VARIABLES
    //  Numero de carriles y columnas del rio
    [SerializeField, Tooltip("Carriles del r�o")]
    int railNumber = 3;
    //[SerializeField]
    int colsNumber = 9;
    float floatingPlaneHeight = 0;

    //quizá sobre lol
    int initialDolphins;
    Vector2[] posDolphins;

    // Matrices
    Box[,] occupationMatrix;
    GameObject[,] cubesMatrix;

    // Sizes
    [SerializeField, Tooltip("River size")]
    Vector3 _riverSize;
    Vector3 _cubeSize;

    // Offset (drag)
    [SerializeField, Tooltip("Offset for dolphin drag")]
    Vector3 _offset;

    // GAME LEVEL VARIABLES
    // Points
    [Header("Level variables")]
    [SerializeField, Tooltip("Necessary points to end level")]
    int _winPoints;
    int _currentPoints;
    [SerializeField, Tooltip("Points to add per right special jump guess")]
    int _specialJumpPoints;
    int _specialJumpIncreasedVelPoints;
    [SerializeField, Tooltip("Points to add per wrong jump guess")]
    int _wrongSpecialJumpPoints;
    [SerializeField, Tooltip("Points to substact per collision with obstacle")]
    int _hitObstaclePoints;

    // Velocity
    [SerializeField]
    float _increaseVelFactor = 3;
    bool _increasedVelocity = false;
    [SerializeField, Tooltip("Points to add when getting through a floatie")]
    int _floatiePoints = 50;
    int _floatieIncreasedVelPoints = 100;

    // DolphinTimes
    [SerializeField, Tooltip("Min time between jumps")]
    float minJumpTime;
    [SerializeField, Tooltip("Max time between jumps")]
    float maxJumpTime;
    float currTime;
    [SerializeField, Tooltip("Min time between special jumps")]
    int minSpecialJumpCount;
    [SerializeField, Tooltip("Max time between special jumps")]
    int maxSpecialJumpCount;
    [SerializeField, Tooltip("More than one special jump")]
    bool piruetasSimult;

    // Para obstaculos
    [SerializeField]
    RandomObjectSpawner randomObjectSpawner;
    [SerializeField, Tooltip("Time for object spawning")]
    float minSpawnTime;
    float maxSpawnTime;
    float nextSpawnTime;
    bool _obstacleSpawning = true;
    float pauseSpawningTime = 5.0f;
    float _obstacleSpeed;

    // Para flotadores
    bool _floatieSpawning = true;

    // Managers (queremos instanciar prefabs o hacer un find?)
    [SerializeField]
    DolphinUIManager _UIManager;
    [SerializeField]
    DolphinManager _dolphinManager;

    // BackGround
    [SerializeField]
    GameObject _background;
    enviroMov _backgroundMovementComp;

    // Ballena 
    [SerializeField, Tooltip("Whale prefab")]
    GameObject _whale;
    [SerializeField]
    int _whaleSpawnNum = 3;
    int _whaleTryCont = 0;

    [SerializeField]
    List<GameObject> _animals;

    //SavedfromUI
    [SerializeField]
    configData levelData;
    private void Awake()
    {
        // Si no hay instancia de esta clase ya creada se almacena
        if (_instance == null)
            _instance = this;
        // Si est� creada se destruyee porque no necesitamos una mas
        else
            Destroy(this.gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_obstacleSpawning) nextSpawnTime = maxSpawnTime;
    }

    private void OnEnable()
    {
        _backgroundMovementComp = _background.GetComponent<enviroMov>();
    }
    // Update is called once per frame
    void Update()
    {
        if (_obstacleSpawning || _floatieSpawning)
        {
            currTime += Time.deltaTime;
            if (currTime >= nextSpawnTime)
            {
                nextSpawnTime = UnityEngine.Random.Range(minSpawnTime, maxSpawnTime);
                currTime = 0;
                randomObjectSpawner.Spawn();
            }
        }
    }


    /// <summary>
    /// Método que inicializa el nivel
    /// </summary>
    /// <param name="config">Archivo de configuración de nivel</param>
    public void InitLevel(configData config)
    {
        // Carga de configuración
        bool loaded = LoadConfiguration(config);

        // Inicialización río / matriz de ocupación
        InitialiseMatrixes();

        // Inicialización Delfines
        List<Vector2> dolphinXYPositions = new List<Vector2>(); // Posiciones lógicas en la matriz

        if (!loaded) //default
        {
            initialDolphins = 2;
            dolphinXYPositions.Clear();
            dolphinXYPositions.Add(new Vector2(0, 0));
            dolphinXYPositions.Add(new Vector2(1, 3));
        }

        List<Vector3> dolphinRealPositions = new List<Vector3>();
        int divingDolphins = 0; // Delfines que no estarán en la superfície en el comienzo del nivel

        for (int i = 0; i < initialDolphins; i++)
        {
            if (posDolphins[i].x == -1) divingDolphins++; 
            else
            {
                dolphinXYPositions.Add(posDolphins[i]);
                dolphinRealPositions.Add(GetWorldPositionFromCube((int)posDolphins[i].y, (int)posDolphins[i].x));
                occupationMatrix[(int)posDolphins[i].y, (int)posDolphins[i].x] = Box.Dolphin;
            }
        }

        _dolphinManager.Init(initialDolphins, divingDolphins, dolphinRealPositions, dolphinXYPositions, minJumpTime, maxJumpTime, 10, true, minSpecialJumpCount, maxSpecialJumpCount);

        // Velocidad y habilitación de objetos instanciados (obstáculos, flotadores)
        randomObjectSpawner.SetVel(_obstacleSpeed);
        randomObjectSpawner.EnableObstacles(_obstacleSpawning);
        randomObjectSpawner.EnableFloats(_floatieSpawning);

        // Velocidad del fondo
        _backgroundMovementComp.SetVelocity(_obstacleSpeed / 50);  
        _dolphinManager.DeactivateIncreasedSpeed(8);
        _dolphinManager.ActivateIncreasedSpeed(_obstacleSpeed);

        // Inicialización UI del nivel
        _UIManager.startLevelStats(0, 0);

        // Inicialización del Event Register Manager
        EventRegister.Instance.WriteStart();
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.Inicio, "nivel X"));
        EventRegister.Instance.EvntToJson();
    }

    /// <summary>
    /// Inicialización de matriz de ocupación según carriles
    /// </summary>
    public void InitialiseMatrixes()
    {
        // Calculo tamanyos
        _cubeSize = new Vector3(_riverSize.x / colsNumber, 0.5f, _riverSize.z / railNumber); // cube size
        _offset = _offset + new Vector3(-_riverSize.x / 2, 0.0f, _riverSize.z / 2); // coloca centrado;

        // Inicializo matrices
        occupationMatrix = new Box[colsNumber, railNumber];
        cubesMatrix = new GameObject[colsNumber, railNumber];

        randomObjectSpawner.Init(railNumber);

        // Creacion de casillas en la escena
        for (int i = 0; i < railNumber; i++) // i -> y
        {
            for (int j = 0; j < colsNumber; j++) // j -> x
            {
                // Relleno matrices
                occupationMatrix[j, i] = Box.Empty;
                cubesMatrix[j, i] = CreateCube(j, i);
            }
        }

        _offset = _offset - new Vector3(-_riverSize.x / 2, 0.0f, _riverSize.z / 2);
    }

    private bool CheckLevelWinCondition()
    {
        if (_currentPoints >= _winPoints)
        {
            SetAllObstacleSpawning(false);
            EndLevel();
            return true;
        }
        return false;
    }
    private void EndLevel()
    {
        _UIManager.showWin();
        _dolphinManager.DeactivateDolphins();
    }

    /// <summary>
    /// Puntos añadidos por acierto al elegir salto
    /// </summary>
    public int RightGuess()
    {
        int pointsToAdd = _specialJumpPoints;

        if (_increasedVelocity)
            pointsToAdd = _specialJumpIncreasedVelPoints;

        _currentPoints += pointsToAdd;
        _UIManager.updatePoints(_currentPoints);

        // Aparicion ballena con "_whaleSpawnNum" numero de aciertos
        _whaleTryCont++;
        if (_whaleSpawnNum <= _whaleTryCont)
        {
            // Animacion ballena
            _whale.SetActive(true);
            SetAllObstacleSpawning(false);

            _whaleTryCont = 0;
        }

        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.NPuntos, _currentPoints.ToString("00")));
        EventRegister.Instance.EvntToJson();

        CheckLevelWinCondition();

        return pointsToAdd;
    }

    /// <summary>
    /// Puntos restados por equivocación al elegir salto
    /// </summary>
    public int WrongGuess()
    {
        _currentPoints += _wrongSpecialJumpPoints;
        if (_currentPoints < 0)
            _currentPoints = 0;
        _UIManager.updatePoints(_currentPoints);
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.NPuntos, _currentPoints.ToString("00")));
        EventRegister.Instance.EvntToJson();
        return _wrongSpecialJumpPoints;
    }

    /// <summary>
    /// Puntos restados por colisión con obstáculo
    /// </summary>
    public int HitObstacle()
    {
        _currentPoints += _hitObstaclePoints;
        if (_currentPoints < 0)
            _currentPoints = 0;
        _UIManager.updatePoints(_currentPoints);
        return _hitObstaclePoints;
    }

    /// <summary>
    /// Puntos añadidos por saltar en flotador
    /// </summary>
    public int FloatHit()
    {
        int pointsToAdd = _floatiePoints;

        if (_increasedVelocity)
            pointsToAdd = _floatieIncreasedVelPoints;

        _currentPoints += pointsToAdd;
        _UIManager.updatePoints(_currentPoints);

        CheckLevelWinCondition();

        return pointsToAdd;
    }

    // Crea una casilla en la posicion indicada x,y
    private GameObject CreateCube(int x, int y)
    {
        // Creacion cubo dependiendo de las medidas
        GameObject _cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

        _cubeObject.transform.localScale = _cubeSize; // escala
        _cubeObject.transform.position = new Vector3(x * _cubeSize.x + _cubeSize.x / 2, 0.0f, -y * _cubeSize.z - _cubeSize.z / 2) + _offset; // position

        if (x == 0)
        { //en la primera casilla registra el carril en el spawner
            randomObjectSpawner.SetCenterPos(y, _cubeObject.transform.position.z);
        }

        _cubeObject.GetComponent<MeshRenderer>().enabled = false; // Invisible
        _cubeObject.GetComponent<Collider>().isTrigger = true;
        MatrixCubeInfo cubeMatrixInfo = _cubeObject.AddComponent<MatrixCubeInfo>();
        cubeMatrixInfo.SetXY(x, y);
        _cubeObject.layer = 8;

        return _cubeObject;
    }

    /// <summary>
    /// Devuelve GameObject de la posicion de la matriz indicada
    /// </summary>
    public GameObject GetCubeFromMatrix(int x, int y)
    {
        return cubesMatrix[x, y];
    }

    /// <summary>
    /// Posición en el mundo real de la casilla
    /// </summary>
    public Vector3 GetWorldPositionFromCube(int x, int y)
    {
        return GetCubeFromMatrix(x, y).GetComponent<Transform>().position;
    }
    /// <summary>
    /// Devuelve Enumerado de si esta ocupado en la posicion de la matriz indicada
    /// </summary>
    public Box GetOccupationFromMatrix(int x, int y)
    {
        return occupationMatrix[x, y];
    }

    /// <summary>
    /// Marca posición de la matriz como ocupada con el tipo de objeto indicado
    /// </summary>
    public void SetOccupation(int x, int y, Box occupation)
    {
        occupationMatrix[x, y] = occupation;
    }

    public Vector3 GetRiverSize()
    {
        return _riverSize;
    }

    public Vector3 GetRiverOffset()
    {
        return _offset;
    }

    public float GetRiverFloatingHeight()
    {
        return floatingPlaneHeight;
    }

    /* Sin usar actualmente, pero podía usarse para calcular con más precisión el carril por el que salir
    protected struct InfoRail
    {
        public int railNumber;
        public int nObstacles;
        public int distToFirstObs;
        public int freeSpots;

        public InfoRail(int railN, int nObs, int dist, int spots)
        {
            railNumber = railN;
            nObstacles = nObs;
            distToFirstObs = dist;
            freeSpots = spots;
        }
    }
    //Return num objs, distance a primer obs 
    private InfoRail GetRailOccupancy(int xPos, int rail)
    {
        InfoRail occupancy = new InfoRail(rail, 0, 0, 0);
        int i = xPos;
        while (i < cubesMatrix.GetLength(0))
        {
            Box ocup = GetOccupationFromMatrix(i, rail);

            if (ocup == Box.Obstacle)
            {
                if (occupancy.nObstacles == 0) occupancy.distToFirstObs = i;
                occupancy.nObstacles++;
            }
            else if (ocup == Box.Empty)
            {
                occupancy.freeSpots++;
            }
            i++;
        }

        if (occupancy.nObstacles == 0) occupancy.distToFirstObs = 1000;
        return occupancy;
    }
    */

    /// <summary>
    /// Método que devueleve el punto de la matriz libre más cercano libre
    /// y potencialmente lo ocupa con el objeto indicado
    /// </summary>
    /// <param name="pos">Posición actual del delfín</param>
    /// <param name="setOcuppation">Si se quiere settear la posición en la matriz automáticamente</param>
    /// <param name="type">Tipo de objeto a colocar</param>
    /// <returns></returns>
    public Vector2 GetNextAvailableMatrixSpot(Vector3 pos, bool setOcuppation = true, Box type = Box.Dolphin)
    {
        Vector2 nextPos = new Vector3(0, 1);
        Vector2 dolphinMatrixPos = GetUpperCubeXYfromDivePos(pos);

        //bool success = false;
        int x = (int)dolphinMatrixPos.x;
        int y = (int)dolphinMatrixPos.y;

        //int minDistanceToObs = 4;

        nextPos = GetMatrixXFreePos(x, y, setOcuppation, type);
     
        StartCoroutine(PauseObstaclesInRail((int)nextPos.y));
        return nextPos;
    }

    //Coge la primera posición libre hacia la derecha desde su posición actual
    private Vector2 GetMatrixXFreePos(int x, int y, bool setOcuppation, Box type)
    {
        bool success = false;
        Vector2 pos = new Vector3(0, 1);

        while (!success)
        {
            if (GetOccupationFromMatrix(x, y) == Box.Empty)
            {
                pos = new Vector2(x, y);
                success = true;
                if (setOcuppation) SetOccupation(x, y, type);
            }
            else
            {
                x++;
            }
        }
        return pos;
    }

   /// <summary>
   /// Método para traducir posicion de buceo a posicion en matriz en la superfície del río (en cuanto a x, z)
   /// </summary>
    public Vector2 GetUpperCubeXYfromDivePos(Vector3 pos) //generalizar a dir 
    {
        int layer_mask = LayerMask.GetMask("Matrix");
        bool hasHit = Physics.Raycast(pos, Vector3.up, out RaycastHit hit, Mathf.Infinity, layer_mask);
        Debug.DrawRay(pos, Vector3.up, Color.green, 4.0f);

        if (hasHit)
        {
            return hit.collider.GetComponent<MatrixCubeInfo>().GetXY();
        }

        else return new Vector2(0, 1);
    }

    /// <summary>
    /// Activa o Desactiva el spawner de obstaculos
    /// </summary>
    public void SetAllObstacleSpawning(bool enabled)
    {
        _obstacleSpawning = enabled;
    }

    /// <summary>
    /// Activa o Desactiva el spawner de obstaculos en el carril indicado en railNum
    /// </summary>
    public void SetObstacleSpawnerInRail(int railNum, bool enabled)
    {
        randomObjectSpawner.SetRailObstacleSpawner(railNum, enabled);
    }

    /// <summary>
    /// Pausa la instanciación de obstáculos en el raíl
    /// </summary>
    IEnumerator PauseObstaclesInRail(int rail)
    {
        SetObstacleSpawnerInRail(rail, false); 
        yield return new WaitForSeconds(pauseSpawningTime);
        SetObstacleSpawnerInRail(rail, true);
    }

    /// <summary>
    /// Metodo que guarda los datos de la configuracion en las variables privadas de la clase
    /// </summary>
    /// <param name="config">Archivo de configuración de nivel</param>
    /// <returns></returns>
    bool LoadConfiguration(configData config)
    {
        levelData = config;
        if (levelData == null) return false; //fail

        railNumber = levelData.NumCarriles;
        initialDolphins = levelData.NumDelfines;
        posDolphins = levelData.PosDelfines;

        _floatieSpawning = levelData.FloatsEnabled;
        _obstacleSpawning = levelData.ObstaclesEnabled;

        minSpawnTime = levelData.MinObstacleSpawn;
        maxSpawnTime = levelData.MaxObstacleSpawn;
        _obstacleSpeed = levelData.ObstacleSpeed;

        minJumpTime = levelData.MinTimeBetweenJumps;
        maxJumpTime = levelData.MaxTimeBetweenJumps;
        minSpecialJumpCount = (int)levelData.MinCountBetweenSpecialJumps;
        maxSpecialJumpCount = (int)levelData.MaxCountBetweenSpecialJumps;
        piruetasSimult = levelData.canSpecialJumpSimultaneously;

        _winPoints = (int)levelData.LevelPoints;
        _specialJumpPoints = (int)levelData.RightGuessPoints;
        _specialJumpIncreasedVelPoints = (int)levelData.RightGuessPointsVel;
        _floatiePoints = (int)levelData.FloatiePoints;
        _floatieIncreasedVelPoints = (int)levelData.FloatiePointsVel;
        _wrongSpecialJumpPoints = (int)levelData.WrongGuessPoints;
        _hitObstaclePoints = (int)levelData.HitObstaclePoints;

        _whaleSpawnNum = levelData.WhaleApearingGuests;
        _increaseVelFactor = levelData.IncreasedSpeedFactor;

        return true;
    }

    /// <summary>
    /// Activa el aumento de velocidad de los objetos, delfines y fondo
    /// </summary>
    public void ActivateIncreasedSpeed()
    {
        _increasedVelocity = true;

        // Background
        float backgroundVel = _backgroundMovementComp.GetVelocity();
        backgroundVel *= _increaseVelFactor;
        _backgroundMovementComp.SetVelocity(backgroundVel);

        // Obstaculos y flotador
        float objectsVel = randomObjectSpawner.GetVel();
        objectsVel *= _increaseVelFactor;
        randomObjectSpawner.SetVel(objectsVel);
        randomObjectSpawner.ChangeAllVelocities(objectsVel);

        // Animacion delfines
        _dolphinManager.ActivateIncreasedSpeed(_increaseVelFactor);
    }

    /// <summary>
    /// Desactiva el aumento de velocidad de los objetos, delfines y fondo
    /// </summary>
    public void DeactivateIncreasedSpeed()
    {
        if (_increasedVelocity)
        {
            _increasedVelocity = false;
            _UIManager.SetVelButton(true);

            // Background
            float backgroundVel = _backgroundMovementComp.GetVelocity();
            backgroundVel /= _increaseVelFactor;
            _backgroundMovementComp.SetVelocity(backgroundVel);

            // Obstaculos y flotador
            float objectsVel = randomObjectSpawner.GetVel();
            objectsVel /= _increaseVelFactor;
            randomObjectSpawner.SetVel(objectsVel);
            randomObjectSpawner.ChangeAllVelocities(objectsVel);

            // Animacion delfines
            _dolphinManager.DeactivateIncreasedSpeed(_increaseVelFactor);
        }
    }

    public void DeregisterObject(GameObject obj)
    {
        randomObjectSpawner.DeregisterObject(obj);
    }

    /// <summary>
    /// Pausa elementos visuales del juego
    /// </summary>
    public void Pause(bool pause)
    {
        SetAllObstacleSpawning(!pause); // no spawnea obstaculos
        randomObjectSpawner.PauseObjects(pause); // pausa objetos
        _dolphinManager.PauseDolphins(pause); // pausa delfines
        _dolphinManager.enabled = !pause; // para manager delfines
        _backgroundMovementComp.enabled = !pause; // pausa fondo

        // Pausa animaciones de animales
        for (int i = 0;i < _animals.Count; i++) {
            _animals[i].GetComponent<Animator>().enabled = !pause;
        }
    }
}

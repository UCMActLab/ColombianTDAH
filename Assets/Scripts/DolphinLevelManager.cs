using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Box { Empty, Dolphin, Obstacle, Floatie }

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
    [SerializeField, Tooltip("Points to add per wrong jump guess")]
    int _wrongSpecialJumpPoints;
    [SerializeField, Tooltip("Points to substact per collision with obstacle")]
    int _hitObstaclePoints;
    [SerializeField]
    int _increasedVelPointsFactor = 2;

    // Velocity
    [SerializeField]
    float _increaseVelFactor = 3;
    bool _increasedVelocity = false;
    [SerializeField, Tooltip("Points to add when getting through a floatie")]
    int _floatiePoints = 50;

    // DolphinTimes
    [SerializeField, Tooltip("Min time between jumps")]
    protected float minJumpTime;
    [SerializeField, Tooltip("Max time between jumps")]
    protected float maxJumpTime;
    protected float currTime;
    [SerializeField, Tooltip("Min time between special jumps")]
    protected float minSpecialJumpCount;
    [SerializeField, Tooltip("Max time between special jumps")]
    protected float maxSpecialJumpCount;

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

    [SerializeField, Tooltip("Whale prefab")]
    GameObject _whale;
    [SerializeField]
    int _whaleSpawnNum = 3;
    int _whaleTryCont = 0;

    //SavedfromUI
    [SerializeField]
    configData levelData;

    public void InitLevel(configData config)
    {
        bool loaded = LoadConfiguration(config);
        InitialiseMatrixes();

        List<Vector2> dolphinXYPositions = new List<Vector2>();


        if (!loaded)
        {
            initialDolphins = 2;
            dolphinXYPositions.Add(new Vector2(0, 0));
            dolphinXYPositions.Add(new Vector2(1, 3));
        }

        List<Vector3> dolphinRealPositions = new List<Vector3>();
        int divingDolphins = 0;

        for (int i = 0; i < initialDolphins; i++)
        {
            if (posDolphins[i].x == -1) divingDolphins++;
            else
            {
                dolphinXYPositions.Add(posDolphins[i]);
                dolphinRealPositions.Add(GetWorldPositionFromCube((int)posDolphins[i].y, (int)posDolphins[i].x));//----------------------------------
            }
        }

        _dolphinManager.Init(initialDolphins, divingDolphins, dolphinRealPositions, dolphinXYPositions, minJumpTime, maxJumpTime, 10);

        // Obstacles Velocity
        randomObjectSpawner.SetVel(_obstacleSpeed);

        //Enable Floats
        randomObjectSpawner.EnableObstacles(_obstacleSpawning); 
        randomObjectSpawner.EnableFloats(_floatieSpawning);

        // Background Velocity
        _backgroundMovementComp.SetVelocity(_obstacleSpeed/50);    // same as obstacles in game

        // Init Level UIs
        _UIManager.startLevelStats(0, 0);

        //Init Event Register Manager
        EventRegister.Instance.WriteStart();
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.Inicio, "nivel X"));
        EventRegister.Instance.EvntToJson();
    }

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


    private void EndGame()
    {
        _UIManager.showWin();
        _dolphinManager.DeactivateDolphins();
        //freeze gam/disable input
    }

    public int RightGuess()
    {
        int pointsToAdd = _specialJumpPoints;

        if (_increasedVelocity)
            pointsToAdd *= _increasedVelPointsFactor;

        _currentPoints += pointsToAdd;
        _UIManager.updatePoints(_currentPoints);

        // Aparicion ballena con "_whaleSpawnNum" numero de aciertos
        _whaleTryCont++;
        if(_whaleSpawnNum <= _whaleTryCont)
        {
            // Animacion ballena
            _whale.SetActive(true);
            SetAllObstacleSpawning(false);

            _whaleTryCont = 0;
        }


        //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.NPuntos, _currentPoints.ToString("00")));
        //EventRegister.Instance.EvntToJson();
        if (_currentPoints >= _winPoints)
        {
            SetAllObstacleSpawning(false);
            EndGame();
        }
        return pointsToAdd;
    }

    public int WrongGuess()
    {
        _currentPoints += _wrongSpecialJumpPoints;
        if (_currentPoints < 0)
            _currentPoints = 0;
        _UIManager.updatePoints(_currentPoints);
        //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.NPuntos, _currentPoints.ToString("00")));
        //EventRegister.Instance.EvntToJson();
        return _wrongSpecialJumpPoints;
    }

    public int HitObstacle()
    {
        _currentPoints += _hitObstaclePoints;
        if (_currentPoints < 0)
            _currentPoints = 0;
        _UIManager.updatePoints(_currentPoints);
        return _hitObstaclePoints;
    }
    public int FloatHit()
    {
        int pointsToAdd = _floatiePoints;

        if (_increasedVelocity)
            pointsToAdd *= _increasedVelPointsFactor;

        _currentPoints += pointsToAdd;
        _UIManager.updatePoints(_currentPoints);
        return pointsToAdd;
    }
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
        if (_obstacleSpawning)
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
    /// M�todos de caculo de matrices/rio
    /// </summary>

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

    // Devuelve GameObject de la posicion de la matriz indicada
    public GameObject GetCubeFromMatrix(int x, int y)
    {
        return cubesMatrix[x, y];
    }

    // Devuelve Enumerado de si esta ocupado en la posicion de la matriz indicada
    public Box GetOccupationFromMatrix(int x, int y)
    {
        return occupationMatrix[x, y];
    }

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

    //public int XLaneOccupancyDistance(int xLargo, int yCarril) //distance between pos xLargo and next obj in line alongside the axis x of river
    //{
    //    int i = xLargo;
    //    int dist = 0;
    //    bool found = false;
    //    int nObstacules = 0;
    //    int nDolphins = 0;
    //    while (i < cubesMatrix.GetLength(0))
    //    {
    //        Box ocup = GetOccupationFromMatrix(i, yCarril);

    //        if (ocup == Box.Obstacle)
    //        {
    //            found = true; nObstacules++;
    //        }
    //        else if (ocup == Box.Dolphin) { nDolphins++; }
    //        if (!found) dist++;
    //        i++;
    //    }
    //    if (!found) return 100;
    //    return dist;
    //}

    //return num objs, distance a primer obs 
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


    //cogemos el punto en la matriz m�s libre (respecto a un punto hacia su derecha, por donde aparecen los obst�culos(?))
    public Vector2 GetNextAvailableMatrixSpot(Vector3 pos, bool setOcuppation = true, Box type = Box.Dolphin)
    {
        Vector2 nextPos = new Vector3(0, 1);
        Vector2 dolphinMatrixPos = GetUpperCubeXYfromDivePos(pos);
        //Debug.Log(dolphinMatrixPos.x + " " + dolphinMatrixPos.y);

        bool success = false;
        int x = (int)dolphinMatrixPos.x;
        int y = (int)dolphinMatrixPos.y;

        int minDistanceToObs = 4;

        nextPos = GetMatrixXFreePos(x, y, setOcuppation, type);
        //THRID TRY LOL
        //InfoRail railToCheck = new InfoRail(y, 0, 0, 0);
        //int rail = y;
        //railToCheck = GetRailOccupancy(x, rail);

        //if (railToCheck.distToFirstObs < minDistanceToObs) return GetMatrixXFreePos(x, rail, setOcuppation, type);

        //int i = 1;
        //int leftRail = y, rightRail = y;

        //Debug.Log("hiiii");
        //InfoRail railToCheckUp = new InfoRail(y, 0, 0, 0);
        //InfoRail railToCheckDown = new InfoRail(y, 0, 0, 0);

        //Debug.Log("rails : " + railNumber);
        //while (i < railNumber / 2)
        //{

        //    if (leftRail - i >= 0)
        //    {
        //        leftRail -= 1;
        //        railToCheckUp = GetRailOccupancy(x, leftRail);
        //    }
        //    if (rightRail + i < railNumber)
        //    {
        //        rightRail = rail += 1;
        //        railToCheckDown = GetRailOccupancy(x, rightRail);
        //    }

        //    if (railToCheckUp.distToFirstObs > railToCheckDown.distToFirstObs || (railToCheckUp.distToFirstObs == railToCheckDown.distToFirstObs && railToCheckUp.freeSpots > railToCheckDown.freeSpots))
        //    {
        //        railToCheck = railToCheckUp;
        //    }
        //    else if (railToCheckDown.distToFirstObs > railToCheckUp.distToFirstObs || (railToCheckDown.distToFirstObs == railToCheckUp.distToFirstObs && railToCheckDown.freeSpots > railToCheckUp.freeSpots))
        //    {
        //        railToCheck = railToCheckDown;
        //    }

        //    if (railToCheck.distToFirstObs < minDistanceToObs)
        //    {
        //       nextPos = GetMatrixXFreePos((int)dolphinMatrixPos.x, railToCheck.railNumber, setOcuppation, type); break;
        //    }

        //    i++;
        //}

        //for (int i = 0; i < railNumber / 2; i++)
        //{
        //    if (y - 1 >= 0) y--;
        //    {

        //        railToCheck = GetRailOccupancy(x, y - 1);
        //        if (railToCheck.distToFirstObs < minDistanceToObs) return new Vector2(x, rail);
        //    }
        //    if (y + 1 < railNumber) y++;
        //    {

        //    }

        //}
        //while (!success)
        //{
        //    if (GetOccupationFromMatrix(x, y) == Box.Empty)
        //    {
        //        nextPos = new Vector2(x, y);
        //        success = true;
        //        if (setOcuppation) SetOccupation(x, y, type);
        //    }
        //    else
        //    {
        //        x++;
        //    }
        //}

        //cosas en las que estoy cookeando las prioridades xd
        ////neccessary space to spawn = algo //distance, lane
        //int currentYLane = (int)dolphinMatrixPos.y;

        //var distancesList = new List<Tuple<int, int>>();


        //int dist = XLaneOccupancyDistance((int)pos.x, (int)pos.y);
        //distancesList.Add(new Tuple<int, int>(dist, (int)pos.y));

        //distancesList.Sort((x, y) => y.Item1.CompareTo(x.Item1));


        ////tambien puede ponerse despues de un obstaculo, antes quiza sea problematico
        //nextPos = new Vector3(pos.x, distancesList[0].Item2, pos.y);

        //return nextPos;
        //Debug.Log("New position: " + nextPos);


        StartCoroutine(PauseObstaclesInRail((int)nextPos.y));
        return nextPos;
    }

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
    public Vector3 GetWorldPositionFromCube(int x, int y)
    {
        return GetCubeFromMatrix(x, y).GetComponent<Transform>().position;
    }

    //metodo para traducir posicion de diving a posicion en matriz (en cuanto a x, z)
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

    // Activa o Desactiva el spawner de obstaculos
    public void SetAllObstacleSpawning(bool enabled)
    {
        _obstacleSpawning = enabled;
    }

    // Activa o Desactiva el spawner de obstaculos en el carril indicado en railNum
    public void SetObstacleSpawnerInRail(int railNum, bool enabled)
    {
        randomObjectSpawner.SetRailObstacleSpawner(railNum, enabled);
    }
    IEnumerator PauseObstaclesInRail(int rail)
    {
        SetObstacleSpawnerInRail(rail, false); //Debug.Log(rail + "I stoppeddddd");
        yield return new WaitForSeconds(pauseSpawningTime);
        SetObstacleSpawnerInRail(rail, true); //Debug.Log(rail + "I returnedddd");
    }

    // Metodo que guarda los datos de la configuracion en las variables privadas de la clase
    bool LoadConfiguration(configData config)
    {
        levelData = config;

        if (levelData == null) return false;


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
        minSpecialJumpCount = levelData.MinCountBetweenSpecialJumps;
        maxSpecialJumpCount = levelData.MaxCountBetweenSpecialJumps;

        _winPoints = (int)levelData.LevelPoints; 
        _specialJumpPoints = (int)levelData.RightGuessPoints;
        _wrongSpecialJumpPoints = (int)levelData.WrongGuessPoints;
        _hitObstaclePoints = (int)levelData.HitObstaclePoints;


        return true;
    }

    public void ActivateIncreasedSpeed()
    {
        _increasedVelocity = true;

        // Background
        float backgroundVel = _backgroundMovementComp.GetVelocity();
        backgroundVel *= _increaseVelFactor;
        _backgroundMovementComp.SetVelocity(backgroundVel);

        // Obstaculos
        float objectsVel = randomObjectSpawner.GetVel();
        objectsVel *= _increaseVelFactor;
        randomObjectSpawner.SetVel(objectsVel);
        randomObjectSpawner.ChangeAllVelocities(objectsVel);

    }

    public void DeactivateIncreasedSpeed()
    {
        if (_increasedVelocity) {
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
        }
    }

    public void DeregisterObject(GameObject obj)
    {
        randomObjectSpawner.DeregisterObject(obj);
    }
}

using UnityEngine;

public enum Box { Empty, Dolphin, Obstacle }

public class DolphinLevelManager : MonoBehaviour
{
    // Singleton
    static private DolphinLevelManager _instance;
    public static DolphinLevelManager Instance { get { return _instance; } }


    [SerializeField]
    int railNumber = 3;

    [SerializeField]
    int colsNumber = 8;

    // Matrices
    Box[,] occupationMatrix;
    GameObject[,] cubesMatrix;

    // Sizes
    [SerializeField]
    Vector3 _riverSize;
    Vector3 _cubeSize;

    // Offset
    [SerializeField]
    Vector3 _offset;

    //GAME VARIABLES
    [Header("Level variables")]
    [SerializeField, Tooltip("Necessary points to end level")]
    int _winPoints;
    int _currentPoints;
    [SerializeField, Tooltip("Points per right special jump guess")]
    int _specialJumpPoints;


    [SerializeField]
    DolphinUIManager _UIManager; //quizá mejor con un find o singleton, o con un prefab de ui de nivel a instanciar

    // Para obstaculos
    RandomObjectSpawner randomObjectSpawner;
    [SerializeField]
    float spawnTime;
    float currTime;

    public int rightGuess()
    {
        _currentPoints += _specialJumpPoints;
        _UIManager.updatePoints(_currentPoints);
        if (_currentPoints >= _winPoints) endGame();
        return _specialJumpPoints;
    }

    private void endGame()
    {
        _UIManager.showWin();
    }

    private void Awake()
    {
        // Si no hay instancia de esta clase ya creada se almacena
        if (_instance == null)
            _instance = this;
        // Si está creada se destruyee porque no necesitamos una mas
        else
            Destroy(this.gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        randomObjectSpawner = GetComponent<RandomObjectSpawner>();

        // Calculo tamanyos
        _cubeSize = new Vector3(_riverSize.x / colsNumber, 0.5f, _riverSize.z / railNumber); // cube size
        _offset = _offset + new Vector3(-_riverSize.x / 2, 0.0f, _riverSize.z / 2); // coloca centrado;

        // Inicializo matrices
        occupationMatrix = new Box[colsNumber, railNumber];
        cubesMatrix = new GameObject[colsNumber, railNumber];

        randomObjectSpawner.carrilCenetrs = new float[railNumber];

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
        _UIManager.startLevelStats(0, 0);
    }

    // Update is called once per frame
    void Update()
    {
        currTime += Time.deltaTime;
        if (currTime >= spawnTime)
        {
            currTime = 0;
            randomObjectSpawner.Spawn();
        }
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
            randomObjectSpawner.carrilCenetrs[y] = _cubeObject.transform.position.z;
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
}

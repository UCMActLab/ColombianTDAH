using JetBrains.Annotations;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.PlayerSettings;

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
    int floatingPlaneY = 0; //esto lo sabe el drop también

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
    [SerializeField, Tooltip("Points to add per right special jump guess")]
    int _specialJumpPoints;
    [SerializeField, Tooltip("Points to substact per collision with obstacle")] //al final quitábamos puntos? o solo los mandábamos abajo
    int _hitObstaclePoints;


    [SerializeField]
    DolphinUIManager _UIManager; //quizá mejor con un find o singleton, o con un prefab de ui de nivel a instanciar

    // Para obstaculos
    RandomObjectSpawner randomObjectSpawner;
    [SerializeField]
    float spawnTime;
    float currTime;

    [SerializeField]
    GameObject _whale;

    public int RightGuess()
    {
        _currentPoints += _specialJumpPoints;
        _UIManager.updatePoints(_currentPoints);
        if (_currentPoints >= _winPoints)
        {
            // Animacion ballena
            _whale.SetActive(true);

            EndGame();
        }
        return _specialJumpPoints;
    }

    private void EndGame()
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

    //cogemos el punto en la matriz más libre (respecto a un punto hacia su derecha, por donde aparecen los obstáculos(?))
    public Vector2 GetNextAvailableMatrixSpot(Vector3 pos, bool setOcuppation = true, Box type = Box.Dolphin) 
    {
        Vector2 nextPos = new Vector3(0, 1);
        Vector2 dolphinMatrixPos = GetUpperCubeXYfromDivePos(pos);
        //Debug.Log(dolphinMatrixPos.x + " " + dolphinMatrixPos.y);

        bool success = false;
        int x = (int)dolphinMatrixPos.x;
        int y = (int)dolphinMatrixPos.y;

        while (!success)
        {
            if (GetOccupationFromMatrix(x, y) == Box.Empty)
            {
                nextPos = new Vector2(x, y);
                success = true;
                if(setOcuppation) SetOccupation(x, y, type);
            }
            else
            {
                x++;
            }
        }

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
        return nextPos;
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
}

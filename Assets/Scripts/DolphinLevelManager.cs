using UnityEngine;

public enum Box { Empty, Dolphin, Obstacle }

public class DolphinLevelManager : MonoBehaviour
{
    [SerializeField]
    int railNumber = 3;

    [SerializeField]
    int colsNumber = 8;

    // Matrices
    Box[,] occupationMatrix;
    GameObject[,] cubesMatrix;

    // Sizes
    Vector3 _cubeSize;
    Vector3 _riverSize;

    // Offset
    Vector3 _offset;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Calculo tamanyos
        _riverSize = new Vector3(40.0f, 0.5f, 12.0f); // river size
        _cubeSize = new Vector3(_riverSize.x / colsNumber, 0.5f, _riverSize.z / railNumber); // cube size
        _offset = new Vector3(-_riverSize.x/2, 0.0f, _riverSize.z /2); // coloca centrado

        // Inicializo matrices
        occupationMatrix = new Box[colsNumber, railNumber];
        cubesMatrix = new GameObject[colsNumber, railNumber];

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
    }

    // Update is called once per frame
    void Update()
    {

    }

    // Crea una casilla en la posicion indicada x,y
    private GameObject CreateCube(int x, int y)
    {
        // Creacion cubo dependiendo de las medidas
        GameObject _cubeObject = GameObject.CreatePrimitive(PrimitiveType.Cube);

        _cubeObject.transform.localScale = _cubeSize; // escala
        _cubeObject.transform.position = new Vector3(x * _cubeSize.x + _cubeSize.x/2, 0.0f, -y * _cubeSize.z) + _offset; // position


        _cubeObject.GetComponent<MeshRenderer>().enabled = false; // Invisible
        _cubeObject.GetComponent<Collider>().isTrigger = true;
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
}

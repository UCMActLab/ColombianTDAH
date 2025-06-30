using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.Splines;

public class DolphinManager : MonoBehaviour
{
    // Delfines
    [SerializeField]
    protected GameObject dolphinPrefab;
    [SerializeField]
    protected GameObject defaultSpawnPos;
    [SerializeField]
    protected List<GameObject> dolphins; // Lista de todos los delfines (buceando y en superfície)
    protected List<GameObject> dolphinsToCheck; // Lista auxiliar para realizar orden de salto
    protected List<GameObject> surfaceDolphinsList; // Delfines en la superfície
    protected List<GameObject> divingDolphinsList; // Delfines buceando

    // Saltos 
    protected float minJumpTime;
    protected float maxJumpTime;
    protected float nextJumpingTime;
    protected float currTime;
    protected int minSpecialJumpCount;
    protected int maxSpecialJumpCount;

    // Flotar
    protected float floatTime; //tiempo de floating back 
    protected float currFloatTime;

    // Piruetas
    [SerializeField]
    protected bool piruetasSimult; //Varios delfines realizan un salto especial simultáneamente
    int nextPirueta;
    int jumpCont;

    /// <summary>
    /// Intenta manda saltar a un delfin random entre los disponibles en el frame
    /// </summary>
    /// <returns>True si el deflín seleccionado ha saltado</returns>
    private bool Jump()
    {
        int jumpingDolphin = Random.Range(0, dolphinsToCheck.Count); 
        DolphinController dolphinCont = dolphinsToCheck[jumpingDolphin].GetComponent<DolphinController>();
        bool success = false;

        if (jumpCont == nextPirueta) // Si toca salto especial 
        {
            success = dolphinCont.SpecialJump();
            if (success)
            {
                jumpCont = -1;
                GenerateNextSpecialJumpCont();
            }
        }
        else // Salto normal 
        {
            success = dolphinCont.Jump();
        }

        if (!success)
        {
            dolphinsToCheck.Remove(dolphinsToCheck[jumpingDolphin]); // Si el delfín seleccionado no consigue saltar se retira de la lista auxiliar de delfines a contemplar
            return false;
        }

        jumpCont++;

        return true;
    }

    /// <summary>
    /// Intenta mandar un delfín a la superfície de entre los disponibles en el frame
    /// </summary>
    /// <returns>True ha flotado un delfín</returns>
    private bool Float()
    {
        if (divingDolphinsList.Count > 0)
        {
            int toFloatDolphin = Random.Range(0, divingDolphinsList.Count);
            DolphinController dolphinCont = divingDolphinsList[toFloatDolphin].GetComponent<DolphinController>();
            bool success = false;

            success = dolphinCont.Float();

            surfaceDolphinsList.Add(divingDolphinsList[toFloatDolphin]); // Añadimos a lista de delfines en la superfície
            divingDolphinsList.Remove(divingDolphinsList[toFloatDolphin]);
            return true;
        }
        else return false;
    }


    // Métodos intermedios que llama el delfín para avisar de cosas y saber los puntos correspondientes (al convertirse en un singleton level manager inncesarios)
    public int RightGuess()
    {
        int plusPoints = DolphinLevelManager.Instance.RightGuess();
        return plusPoints;
    }
    public int WrongGuess()
    {
        int lessPoints = DolphinLevelManager.Instance.WrongGuess();
        return lessPoints;
    }

    public int HitObstacle(Box type)
    {
        int hitPoints = DolphinLevelManager.Instance.HitObstacle(type);
        return hitPoints;
    }

    public int FloatHit()
    {
        int floatPoints = DolphinLevelManager.Instance.FloatHit();
        return floatPoints;
    }

    /// <summary>
    /// Método de inicialización de delfines en el río
    /// </summary>
    public void Init(int numberDolphins, int divingDolphins, List<Vector3> dolphinPositions, List<Vector2> dolphinXYPositions, float minJumpingTime, float maxJumpingTime, float floatingTime, bool simultSpecialJump, int minSpecialJC, int maxSpecialJC)
    {
        //Creamos en la matriz e instanciamos en la posición correspondiente los delfines colocados
        for (int i = 0; i < numberDolphins - divingDolphins; i++)
        {
            GameObject dolphin = GameObject.Instantiate(dolphinPrefab, dolphinPositions[i], Quaternion.identity);
            dolphins.Add(dolphin);
            surfaceDolphinsList.Add(dolphin);
            dolphin.GetComponent<Drag>().SetIndex(i);
            dolphin.GetComponent<DolphinController>().SetIndex(i);
            dolphins[i].GetComponent<DolphinController>().RegisterDolphinManager(this);
            dolphin.GetComponent<MatrixCubeInfo>().SetXY((int)dolphinXYPositions[i].y, (int)dolphinXYPositions[i].x); 

        }
        //Los delfines buceadores los mandamos a nadar
        for (int i = numberDolphins - divingDolphins; i < numberDolphins; i++)
        {
            GameObject dolphin = GameObject.Instantiate(dolphinPrefab, new Vector3(0, -5, 0), Quaternion.identity);
            dolphins.Add(dolphin);
            dolphin.GetComponent<Drag>().SetIndex(i);
            dolphin.GetComponent<DolphinController>().SetIndex(i);
            dolphins[i].GetComponent<DolphinController>().RegisterDolphinManager(this);
            dolphins[i].GetComponent<DolphinController>().SetStartingState(DolphinController.DolphinStates.DIVING);
        }

        minJumpTime = minJumpingTime;
        maxJumpTime = maxJumpingTime;
        floatTime = floatingTime;
        piruetasSimult = simultSpecialJump;
        minSpecialJumpCount = minSpecialJC;
        maxSpecialJumpCount = maxSpecialJC;

        GenerateNextJumpingTime(); // Primer salto
    }

    /// <summary>
    /// Genera el próximo tiempo de salto (normal/especial)
    /// </summary>
    private void GenerateNextJumpingTime()
    {
        nextJumpingTime = Random.Range(minJumpTime, maxJumpTime);

    }

    /// <summary>
    /// Genera el próximo número de salto en que se realizará una pirueta especial
    /// </summary>
    private void GenerateNextSpecialJumpCont()
    {
        nextPirueta = Random.Range(minSpecialJumpCount, maxSpecialJumpCount);
    }

    /// <summary>
    /// Añadir delfín a grupo de delfines sumergidos
    /// </summary>
    public void AddDivingDolphin(GameObject dolphin)
    {
        divingDolphinsList.Add(dolphin);
        surfaceDolphinsList.Remove(dolphin);
    }
    private void Awake()
    {
        divingDolphinsList = new List<GameObject>();
        surfaceDolphinsList = new List<GameObject>();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (dolphins.Count == 0)
        {
            dolphins = new List<GameObject>();
        }

        currTime = 0;
        currFloatTime = 0;
        jumpCont = 0;

        // Si hay piruetas simultáneas
        if (piruetasSimult) minSpecialJumpCount = 0;
        else if (!piruetasSimult && minSpecialJumpCount < 2) { minSpecialJumpCount = 2; }

        // Generamos el primer salto y cuándo será pirueta especial
        GenerateNextJumpingTime();
        GenerateNextSpecialJumpCont();
    }

    // Update is called once per frame
    void Update()
    {
        currTime += Time.deltaTime;
        if (currTime >= nextJumpingTime)
        {
            currTime = 0;
            GenerateNextJumpingTime();
            dolphinsToCheck = new List<GameObject>(surfaceDolphinsList);
            while (dolphinsToCheck.Count != 0 && !Jump()); // Intentamos que salte alguno de los delfines disponibles
        }

        currFloatTime += Time.deltaTime;
        if (currFloatTime >= floatTime)
        {
            currFloatTime = 0;
            Float();
        }
    }

    // Desactiva todos los delfines
    public void DeactivateDolphins()
    {
        dolphins.ForEach(d => d.SetActive(false));
    }

    // Aumenta la velocidad de la animacion de los delfines
    public void ActivateIncreasedSpeed(float velFactor)
    {
        for (int i = 0; i < dolphins.Count; i++)
        {
            dolphins[i].GetComponent<Animator>().speed *= velFactor;
        }
    }

    // Animacion de los delfines vuelve a velocidad normal
    public void DeactivateIncreasedSpeed(float velFactor)
    {

        for (int i = 0; i < dolphins.Count; i++)
        {
            dolphins[i].GetComponent<Animator>().speed /= velFactor;
        }
    }

    // Pausa animación de todos los delfines
    public void PauseDolphins(bool pause)
    {
        for (int i = 0; i < dolphins.Count; i++)
        {
            dolphins[i].GetComponent<Animator>().enabled = !pause; // pausa animacion
            dolphins[i].GetComponent<Buceo>().enabled = !pause; // pausa movimiento buceo
            dolphins[i].GetComponent<DolphinController>().enabled = !pause; // pausa controlador delfin
            dolphins[i].GetComponent<SplineAnimate>().enabled = !pause; // pausa animacion buceo
        }
    }
}

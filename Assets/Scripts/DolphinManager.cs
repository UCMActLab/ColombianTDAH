using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;
using UnityEngine.Splines;

public class DolphinManager : MonoBehaviour
{
    //lista de delfines, pos
    [SerializeField]
    protected GameObject dolphinPrefab;
    [SerializeField]
    protected GameObject defaultSpawnPos;
    [SerializeField]
    protected List<GameObject> dolphins; //Deberia ser una lista de controllers? (probablemente)
    protected List<GameObject> dolphinsToCheck;
    protected List<GameObject> dolphinsToFloatCheck;

    //times
    protected float minJumpTime;
    protected float maxJumpTime;
    protected float nextJumpingTime;
    protected float currTime;
    protected int minSpecialJumpCount;
    protected int maxSpecialJumpCount;
    protected float floatTime; //tiempo de floating back 
    protected float currFloatTime;

    [SerializeField]
    protected bool piruetasSimult; //Varios delfines realizan un salto especial simultáneamente
    int nextPirueta;
    int jumpCont;

    //methods
    private bool Jump()
    {
        int jumpingDolphin = Random.Range(0, dolphinsToCheck.Count); //idea de siguiente delfin disp: copiar lista y quitar no disponible para sig random 
        DolphinController dolphinCont = dolphinsToCheck[jumpingDolphin].GetComponent<DolphinController>();
        bool success = false;

        if (jumpCont == nextPirueta)
        {
            success = dolphinCont.SpecialJump();
            if (success)
            {
                jumpCont = -1;
                GenerateNextSpecialJumpCont();
            }
        }
        else
        {
            success = dolphinCont.Jump();
        }

        if (!success)
        {
            dolphinsToCheck.Remove(dolphinsToCheck[jumpingDolphin]);
            return false;
        }

        jumpCont++;

        //if(jumpCont == dolphins.Count - 1)
        //{
        //    jumpCont = 0;
        //    GenerateNextSpecialJumpCont(); //esto valdría junto con una lista de delfines por saltar para que saltaran en orden y solo una vez por ronda
        // //deshabilitar jumpCont= 0 de jumpCOnt == nextpirueta si se quiere usar 
        //}

        return true;
    }

    private bool Float()
    {
        int toFloatDolphin = Random.Range(0, dolphinsToFloatCheck.Count);
        DolphinController dolphinCont = dolphinsToFloatCheck[toFloatDolphin].GetComponent<DolphinController>();
        bool success = false;

        success = dolphinCont.Float();

        if (!success)
        {
            dolphinsToFloatCheck.Remove(dolphinsToFloatCheck[toFloatDolphin]);
            return false;
        }

        return true;
    }


    //que llama el delfín para avisar de cosas y saber los puntos correspondientes
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

    public int HitObstacle()
    {
        int hitPoints = DolphinLevelManager.Instance.HitObstacle();
        return hitPoints;
    }

    public int FloatHit()
    {
        int floatPoints = DolphinLevelManager.Instance.FloatHit();
        return floatPoints;
    }
    public void Init(int numberDolphins, int divingDolphins, List<Vector3> dolphinPositions, List<Vector2> dolphinXYPositions, float minJumpingTime, float maxJumpingTime, float floatingTime, bool simultSpecialJump, int minSpecialJC, int maxSpecialJC)
    {
        //Creamos en la matriz e instanciamos en la posición correspondiente los delfines colocados
        for (int i = 0; i < numberDolphins - divingDolphins; i++)
        {
            GameObject dolphin = GameObject.Instantiate(dolphinPrefab, dolphinPositions[i], Quaternion.identity);
            dolphins.Add(dolphin);
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
        GenerateNextJumpingTime();
    }

    private void GenerateNextJumpingTime()
    {
        nextJumpingTime = Random.Range(minJumpTime, maxJumpTime);

    }
    private void GenerateNextSpecialJumpCont()
    {
        nextPirueta = Random.Range(minSpecialJumpCount, maxSpecialJumpCount);
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

        //Si hay piruetas simultáneas
        if (piruetasSimult) minSpecialJumpCount = 0;
        else if (!piruetasSimult && minSpecialJumpCount < 2) { minSpecialJumpCount = 2; }

        //Generamos el primer salto y cuándo será pirueta especial
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
            dolphinsToCheck = new List<GameObject>(dolphins);
            while (dolphinsToCheck.Count != 0 && !Jump()) ;
        }

        currFloatTime += Time.deltaTime;
        if (currFloatTime >= floatTime)
        {
            currFloatTime = 0;
            dolphinsToFloatCheck = new List<GameObject>(dolphins);
            while (dolphinsToFloatCheck.Count != 0 && !Float()) ;
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
        int i = 0;
        for (; i < dolphins.Count; i++)
        {
            dolphins[i].GetComponent<Animator>().enabled = !pause;
            dolphins[i].GetComponent<Buceo>().enabled = !pause;
            dolphins[i].GetComponent<DolphinController>().enabled = !pause;
            dolphins[i].GetComponent<SplineAnimate>().enabled = !pause;
        }
    }
}

using UnityEngine;
using System.Collections.Generic;
using Random = UnityEngine.Random;

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
    protected float jumpTime;
    protected float currTime;
    protected float floatTime;
    protected float currFloatTime;

    [SerializeField]
    protected bool singlePirueta; //realmente esto dependera mucho de nivel 
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

        if(jumpCont == dolphins.Count - 1)
        {
            jumpCont = 0;
            nextPirueta = Random.Range(0, dolphins.Count);
        }
        return true;

    }

    private bool Float()
    {
        int toFloatDolphin = Random.Range(0, dolphinsToFloatCheck.Count); //idea de siguiente delfin disp: copiar lista y quitar no disponible para sig random 
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


    //que llama el delfín para avisar de cosas
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

    public void Init(int numberDolphins, List<Vector3> dolphinPositions, List<Vector2> dolphinXYPositions, float jumpingTime, float floatingTime)
    {
        for (int i = 0; i<numberDolphins; i++) //pos (?)
        {
            GameObject dolphin = GameObject.Instantiate(dolphinPrefab, dolphinPositions[i], Quaternion.identity);
            dolphins.Add(dolphin);
            dolphin.GetComponent<Drag>().SetIndex(i);
            dolphins[i].GetComponent<DolphinController>().registerDolphinManager(this);
            dolphin.GetComponent<MatrixCubeInfo>().SetXY((int)dolphinXYPositions[i].x, (int)dolphinXYPositions[i].y);

        }

        jumpTime = jumpingTime;
        floatTime = floatingTime;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(dolphins.Count == 0)
        {
            dolphins = new List<GameObject>();
        }

        currTime = 0;
        currFloatTime = 0;

        jumpCont = 0;
        nextPirueta = Random.Range(0, dolphins.Count);
    }

    // Update is called once per frame
    void Update()
    {
        currTime += Time.deltaTime;
        if (currTime >= jumpTime)
        {
            currTime = 0;
            dolphinsToCheck = new List<GameObject>(dolphins);
            while (dolphinsToCheck.Count!=0&&!Jump());
        }

        currFloatTime += Time.deltaTime;
        if (currFloatTime >= floatTime)
        {
            currFloatTime = 0;
            dolphinsToFloatCheck = new List<GameObject>(dolphins);
            while (dolphinsToFloatCheck.Count != 0 && !Float());
        }
    }

    public void DeactivateDolphins()
    {
        dolphins.ForEach(d => d.SetActive(false));
    }
}

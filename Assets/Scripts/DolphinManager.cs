using UnityEngine;
using System.Collections.Generic;
using Unity.Mathematics;
using Random = UnityEngine.Random;
using UnityEditor.DeviceSimulation;

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
    [SerializeField]
    protected float jumpTime;
    protected float currTime;
    [SerializeField]
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
    public  int RightGuess()
    {
        int plusPoints = DolphinLevelManager.Instance.RightGuess();
        return plusPoints; 
    }
    //wrong guess si quisieramos o juntarlo 
    //DEMO
    public void startDemo()
    {
        for(int i = 0; i < 5; i++)
        {
            dolphins[i].GetComponent<DolphinController>().Dive();
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        //si no hemos colocado ninguno en el editor que se genere uno para hacer pruebas (lo ideal sería que los niveles fueran serializables/scrìptables  y se leyeran/gestonaran desde game por ej
        if(dolphins.Count == 0)
        {
            dolphins = new List<GameObject>();
            GameObject defaultDolphin = GameObject.Instantiate(dolphinPrefab, defaultSpawnPos.GetComponent<Transform>().position , Quaternion.identity);
            dolphins.Add(defaultDolphin);   
        }

        //registramos el manager para los delfines para que me puedan avisar de cosas/eventos
        for(int i = 0; i<dolphins.Count; i++)
        {
            dolphins[i].GetComponent<DolphinController>().registerDolphinManager(this);
        }

        currTime = 0;
        currFloatTime = 0;

        jumpCont = 0;
        nextPirueta = Random.Range(0, dolphins.Count);

        //Invoke("startDemo", 4);
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
}

using UnityEngine;
using System.Collections.Generic;

public class DolphinManager : MonoBehaviour
{
    //lista de delfines, pos
    [SerializeField]
    protected GameObject dolphinPrefab;
    [SerializeField]
    protected GameObject defaultSpawnPos;
    [SerializeField]
    protected List<GameObject> dolphins; //Deberia ser una lista de controllers? (probablemente)
    [SerializeField]
    DolphinLevelManager levelManager; //Esto a revisar

    //times
    [SerializeField]
    protected float jumpTime;
    protected float currTime;

    [SerializeField]
    protected bool singlePirueta; //realmente esto dependera mucho de nivel 
    int nextPirueta;
    int jumpCont;

    //methods
    private bool Jump()
    {
        int jumpingDolphin = Random.Range(0, dolphins.Count); //idea de siguiente delfin disp: copiar lista y quitar no disponible para sig random 
        DolphinController dolphinCont = dolphins[jumpingDolphin].GetComponent<DolphinController>();
        bool success = false;

        if (jumpCont == nextPirueta)
        {
           success = dolphinCont.SpecialJump();
        }
        else
        {
            success = dolphinCont.Jump();
        }

        jumpCont++;

        if(jumpCont == dolphins.Count - 1)
        {
            jumpCont = 0;
            nextPirueta = Random.Range(0, dolphins.Count);
        }

        return success;
    }

    //que llama el delfín para avisar de cosas
    public  int rightGuess()
    {
        int plusPoints = levelManager.rightGuess();
        return plusPoints; 
    }
    //wrong guess si quisieramos o juntarlo 

    //DEMO
    public void startDemo()
    {
        for(int i = 0; i < 3; i++)
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

        jumpCont = 0;
        nextPirueta = Random.Range(0, dolphins.Count);

        Invoke("startDemo", 4);
    }

    // Update is called once per frame
    void Update()
    {
        currTime += Time.deltaTime;
        if (currTime >= jumpTime)
        {
            currTime = 0;
            while(!Jump());
        }
    }
}

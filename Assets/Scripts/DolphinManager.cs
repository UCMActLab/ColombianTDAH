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
    protected List<GameObject> dolphins;

    //times
    [SerializeField]
    protected float jumpTime;
    protected float currTime;

    [SerializeField]
    protected bool singlePirueta; //realmente esto dependera mucho de nivel 
    int nextPirueta;
    int jumpCont;

    //methods
    public void Jump()
    {
        int jumpingDolphin = Random.Range(0, dolphins.Count);

        if (jumpCont == nextPirueta)
        {
            dolphins[jumpingDolphin].GetComponent<DolphinController>().SpecialJump();
        }
        else
        {
            dolphins[jumpingDolphin].GetComponent<DolphinController>().Jump();
        }

        jumpCont++;

        if(jumpCont == dolphins.Count - 1)
        {
            jumpCont = 0;
            nextPirueta = Random.Range(0, dolphins.Count);
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

        currTime = 0;

        jumpCont = 0;
        nextPirueta = Random.Range(0, dolphins.Count);
    }

    // Update is called once per frame
    void Update()
    {
        currTime += Time.deltaTime;
        if (currTime > jumpTime)
        {
            currTime = 0;
            Jump();
        }
    }
}

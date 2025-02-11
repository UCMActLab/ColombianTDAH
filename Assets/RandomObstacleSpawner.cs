using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectSpawner : MonoBehaviour
{

    public GameObject[] myObjects;
    [SerializeField]
    public Vector3 limitMin;
    [SerializeField]
    public Vector3 limitMax;
    [SerializeField]
    public float velMax;
    public float[] carrilCenetrs;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            int randomIndex = Random.Range(0, myObjects.Length);            
            int randomIdPos = Random.Range(0, carrilCenetrs.Length);
            Vector3 randomSpawnPosition = new Vector3(Random.Range(limitMin.x, limitMax.x), Random.Range(limitMin.y, limitMax.y), carrilCenetrs[randomIdPos]);
            print(randomSpawnPosition);
            GameObject instantiated = Instantiate(myObjects[randomIndex], randomSpawnPosition, Quaternion.identity);
            Destroy(instantiated, 3.0f);
            //float vel = Random.Range(0, velMax);
            //print(vel);
            instantiated.GetComponent<Obstaculo>().m_Vel = 10;
        }

    }
}

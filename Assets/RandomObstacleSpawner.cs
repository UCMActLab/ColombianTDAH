using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomObjectSpawner : MonoBehaviour
{

    public GameObject[] myObjects;
    public float[] carrilCenetrs;

    void Start()
    {

    }

    public void Spawn()
    {
        int randomIndex = Random.Range(0, myObjects.Length);
        int randomIdPos = Random.Range(0, carrilCenetrs.Length);
        Vector3 randomSpawnPosition = new Vector3(20.0f, 0.0f, carrilCenetrs[randomIdPos]);
        GameObject instantiated = Instantiate(myObjects[randomIndex], randomSpawnPosition, Quaternion.identity);
        Destroy(instantiated, 10.0f);
    }
}

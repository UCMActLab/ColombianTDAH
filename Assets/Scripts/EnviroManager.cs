using UnityEngine;

public class EnviroManager : MonoBehaviour
{
    public GameObject[] myRocks;
    public GameObject[] myTrees;
    public GameObject[] myAnimals;
    float spawnTime;
    float currTime;

    void Start()
    {
        spawnTime = 0.2f;
        currTime = 0.0f;
    }

    public void Spawn()
    {
        Debug.Log("Spawn fondo");
        int randomIndex = Random.Range(0, myRocks.Length);
        Vector3 randomSpawnPosition = new Vector3(20.0f, 0.0f, 16.0f);
        GameObject instantiated = Instantiate(myRocks[randomIndex], randomSpawnPosition, Quaternion.identity);
        instantiated.transform.parent = transform;
        Destroy(instantiated, 3.0f);
    }

    void Update()
    {
        currTime += Time.deltaTime;
        if (currTime >= spawnTime)
        {
            currTime = 0;
            Spawn();
        }
    }
}

using UnityEngine;

public class RandomObjectSpawner : MonoBehaviour
{

    struct CarrilInfo
    {
        public bool active;
        public float CenterPosZ;
    }

    [SerializeField]
    float _posX = 20.0f;

    [SerializeField]
    GameObject[] _obstacles;
    CarrilInfo[] _carrilCenetrs;

    float _obsVel;

    public void Init(int nRail)
    {
        _carrilCenetrs = new CarrilInfo[nRail];
    }

    public void SetVel(float speed)
    {
        _obsVel = speed;
    }

    public void SetCenterPos(int index, float zCenter)
    {
        CarrilInfo carrilAux;
        carrilAux.active = true;
        carrilAux.CenterPosZ = zCenter;
        _carrilCenetrs[index] = carrilAux;
    }

    public void Spawn()
    {
        int randomIdPos = Random.Range(0, _carrilCenetrs.Length);

        if (_carrilCenetrs[randomIdPos].active)
        {
            Vector3 randomSpawnPosition = new Vector3(_posX, 0.0f, _carrilCenetrs[randomIdPos].CenterPosZ);

            randomIdPos = Random.Range(0, _obstacles.Length);
            GameObject instantiated = Instantiate(_obstacles[randomIdPos], randomSpawnPosition, Quaternion.identity);
            instantiated.GetComponent<Obstaculo>().SetVel(_obsVel);
        }
    }

    public void SetRailObstacleSpawner(int railNum, bool enabled)
    {
        _carrilCenetrs[railNum].active = enabled;
    }
}

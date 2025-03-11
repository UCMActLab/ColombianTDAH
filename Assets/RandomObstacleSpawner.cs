using UnityEngine;
using System.Collections.Generic;

public class RandomObjectSpawner : MonoBehaviour
{

    struct CarrilInfo
    {
        public bool active;
        public float CenterPosZ;
    }

    [SerializeField]
    float _posX = 14.0f;

    [SerializeField]
    GameObject[] _obstacles;
    CarrilInfo[] _carrilCenetrs;

    List<GameObject> _spawnedObjects;

    float _obsVel;

    private void Start()
    {
        _spawnedObjects = new List<GameObject>();
    }
    public void Init(int nRail)
    {
        _carrilCenetrs = new CarrilInfo[nRail];
    }

    public void SetVel(float speed)
    {
        _obsVel = speed;
    }

    public float GetVel()
    {
        return _obsVel;
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
        int randomCarril = UnityEngine.Random.Range(0, _carrilCenetrs.Length);

        if (_carrilCenetrs[randomCarril].active)
        {
            Vector3 randomSpawnPosition = new Vector3(_posX, 0.0f, _carrilCenetrs[randomCarril].CenterPosZ);

            int randomIdPos = UnityEngine.Random.Range(0, _obstacles.Length);
            GameObject instantiated = Instantiate(_obstacles[randomIdPos], randomSpawnPosition, Quaternion.identity);
            instantiated.transform.Rotate(90,0,0);
            instantiated.GetComponent<MovingObject>().SetVel(_obsVel);
            Debug.Log("Obstaculo: " + instantiated);
            _spawnedObjects.Add(instantiated);
            if (instantiated.GetComponent<Obstaculo>())
            {
                //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.OEntraPantalla, "Carril " + randomCarril.ToString()));
            }
            else
            {
                //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.FEntraPantalla, "Carril " + randomCarril.ToString()));
            }
                //EventRegister.Instance.EvntToJson();
        }

    }

    public void SetRailObstacleSpawner(int railNum, bool enabled)
    {
        _carrilCenetrs[railNum].active = enabled;
    }

    public void DeregisterObject(GameObject obj)
    {
        _spawnedObjects.Remove(obj);
    }

    public void ChangeAllVelocities(float newVel)
    {
        for (int i = 0; i < _spawnedObjects.Count; i++)
        {
            MovingObject movingObjComp = _spawnedObjects[i].GetComponent<MovingObject>();
            if (movingObjComp != null)
            {
                movingObjComp.SetVel(newVel);
            }
        }
    }
}

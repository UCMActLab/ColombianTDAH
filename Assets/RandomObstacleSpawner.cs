using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Rendering;

public class RandomObjectSpawner : MonoBehaviour
{

    struct CarrilInfo
    {
        public bool active;
        public float CenterPosZ;
    }

    [SerializeField]
    float _posX = 14.0f;

    //Listas de prefabs disponibles
    [SerializeField]
    GameObject[] _obstacles;
    [SerializeField]
    GameObject _float;

    //Lista de objetos de entre los cuales instanciar
    List<GameObject> _objects;

    CarrilInfo[] _carrilCenetrs;

    List<GameObject> _spawnedObjects;

    float _obsVel;


    private void Awake()
    {
        _spawnedObjects = new List<GameObject>();
        _objects = new List<GameObject>();
    }
    public void Init(int nRail)
    {
        _carrilCenetrs = new CarrilInfo[nRail];
    }

    public void SetVel(float speed)
    {
        _obsVel = speed;
    }

    public void EnableFloats(bool enable)
    {
        if(enable) _objects.Add(_float);
        else
        {
            foreach (GameObject obj in _objects) //por si hubiera más de un tipo de flotador
            {
                if (obj.GetComponent<Floatie>() != null)
                {
                    _objects.Remove(obj);
                }
            }
        }
    }

    public void EnableObstacles(bool enable)
    {
        if (enable) _objects.AddRange(_obstacles);
        else
        {
            foreach (GameObject obj in _objects) //por si hubiera más de un tipo de flotador
            {
                if (obj.GetComponent<Obstaculo>() != null)
                {
                    _objects.Remove(obj);
                }
            }
        }
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

            int randomIdPos = UnityEngine.Random.Range(0, _objects.Count);
            GameObject instantiated = Instantiate(_objects[randomIdPos], randomSpawnPosition, Quaternion.identity);
            instantiated.transform.Rotate(90,0,0);
            instantiated.GetComponent<MovingObject>().SetVel(_obsVel);
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

using UnityEngine;
using System.Collections.Generic;
using System;

public class RandomObjectSpawner : MonoBehaviour
{

    struct CarrilInfo
    {
        public bool active;
        public float CenterPosZ;
    }

    [SerializeField]
    float _posX = 12.0f;

    //Listas de prefabs disponibles
    [SerializeField]
    GameObject[] _troncos;
    [SerializeField]
    GameObject _float;
    [SerializeField]
    GameObject _boat;

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
        if (enable) _objects.Add(_float);
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

    public void EnableBoats(bool enable)
    {
        if (enable)
        {
            _objects.Add(_boat);
            Debug.Log("Barcos habilitados");
        }
        else
        {
            foreach (GameObject obj in _objects) //por si hubiera más de un tipo de barco
            {
                /* AHORA MISMO SOLO FUNC ENABLE = TRUE
                 * 
                 * NO SIRVE POR Q BARCOS TMBN SON OBSTACULOS
                if (obj.GetComponent<Barco>() != null)
                {
                    _objects.Remove(obj);
                }
                */
            }
        }
    }

    //CUANDO TRONCOS ESTO NO , LISTA TRONCOS NUEVA Y LISTA OBJ PRIVADA?
    public void EnableTroncos(bool enable)
    {
        if (enable) _objects.AddRange(_troncos);
        else
        {
            // ELIMINA TANTO LOS TRONCOS COMO LAS BARCAS
            foreach (GameObject obj in _objects)
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

        if (_carrilCenetrs[randomCarril].active && _objects.Count>0)
        {
            float posY = 0.4f;
            int randomIdPos = UnityEngine.Random.Range(0, _objects.Count);
            if (_objects[randomIdPos].name.StartsWith("Canoa"))
                posY = 1.0f;
            Vector3 randomSpawnPosition = new Vector3(_posX, posY, _carrilCenetrs[randomCarril].CenterPosZ);
            GameObject instantiated = Instantiate(_objects[randomIdPos], randomSpawnPosition, Quaternion.identity);
            if (instantiated.name.StartsWith("tronco"))
                instantiated.transform.Rotate(-90, 0, 0);
            instantiated.GetComponent<MovingObject>().SetVel(_obsVel);
            _spawnedObjects.Add(instantiated);
            if (instantiated.GetComponent<Obstaculo>())
            {
                EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.OEntraPantalla, "Carril " + randomCarril.ToString()));
            }
            else
            {
                instantiated.GetComponent<Floatie>().SetVel(_obsVel);
                EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.FEntraPantalla, "Carril " + randomCarril.ToString()));
            }

            EventRegister.Instance.EvntToJson();
        }

    }

    // Enable specific rail
    public void SetRailObstacleSpawner(int railNum, bool enabled)
    {
        _carrilCenetrs[railNum].active = enabled;
    }

    // Quita objeto de la lista
    public void DeregisterObject(GameObject obj)
    {
        _spawnedObjects.Remove(obj);
    }

    // Cambia la velocidad de todo objeto que este en el rio
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

    // Quita las fisicas de los objetos para pausarlos
    public void PauseObjects(bool pause)
    {
        for (int i = 0; i < _spawnedObjects.Count; i++)
        {
            _spawnedObjects[i].GetComponent<Rigidbody>().isKinematic = pause;
        }
    }
}

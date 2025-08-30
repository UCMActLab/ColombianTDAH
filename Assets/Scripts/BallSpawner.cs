using UnityEngine;
using System.Collections.Generic;
using System;

public class BallSpawner : MonoBehaviour
{
    [SerializeField]
    GameObject _ball;

    List<GameObject> _spawnedObjects;

    float _obsVel;
    bool _enabled = false;

    [SerializeField]
    Vector2 _minZone;
    [SerializeField]
    Vector2 _maxZone;



    //velocidades de los objetos (solo para la pausa)
    private Dictionary<Rigidbody, Vector3> _linearVelocities = new Dictionary<Rigidbody, Vector3>();
    private Dictionary<Rigidbody, Vector3> _angularVelocities = new Dictionary<Rigidbody, Vector3>();


    private void Awake()
    {
        _spawnedObjects = new List<GameObject>();
    }


    public void SetVel(float speed)
    {
        _obsVel = speed;
    }

    public void EnableBalls(bool enable)
    {
        _enabled = enable;
    }

    public float GetVel()
    {
        return _obsVel;
    }

    public void Spawn()
    {
        if (_enabled)
        {
            Vector3 randomSpawnPosition = new Vector3(UnityEngine.Random.Range(_minZone.x, _maxZone.x), 10.0f, UnityEngine.Random.Range(_minZone.y, _maxZone.y));

            GameObject instantiated = Instantiate(_ball, randomSpawnPosition, Quaternion.identity);
            
            Vector3 vel = UnityEngine.Random.onUnitSphere;
            vel.y = -2;
            vel.Normalize();
            vel *= 3;

            instantiated.GetComponent<ObstaculoPelota>().SetVelocity(vel);
            _spawnedObjects.Add(instantiated);

            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.BEntraPantalla, ""));
            EventRegister.Instance.EvntToJson();
        }

    }

    // Quita las fisicas de los objetos para pausarlos y guarda la velocidad que tuvieran para ponersela al despausar
    public void PauseObjects(bool pause)
    {
        for (int i = 0; i < _spawnedObjects.Count; i++)
        {
            GameObject obj = _spawnedObjects[i];
            if (obj == null) continue; //siguiente iteracion

            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb == null) continue; 

            if (pause)
            {
                // guardar velocidades
                if (!_linearVelocities.ContainsKey(rb))
                {
                    _linearVelocities[rb] = rb.linearVelocity;
                    _angularVelocities[rb] = rb.angularVelocity;
                }

                // quitamos fisicas
                rb.isKinematic = true;
            }
            else //si no esta pausado
            {
                // volvemos a usar fisicas
                rb.isKinematic = false;

                // restaurar velocidades si estaban guardadas
                if (_linearVelocities.TryGetValue(rb, out Vector3 vel))
                {
                    rb.linearVelocity = vel;
                }

                if (_angularVelocities.TryGetValue(rb, out Vector3 angVel))
                {
                    rb.angularVelocity = angVel;
                }
            }
        }

        // si se esta continuando el juego hacemos clear de la lista porque ya no sirve hasta la proxima pausa
        if (!pause)
        {
            _linearVelocities.Clear();
            _angularVelocities.Clear();
        }
    }

}

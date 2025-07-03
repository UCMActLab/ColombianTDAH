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

}

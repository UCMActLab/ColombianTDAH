using UnityEngine;

public class RoadSpawn : MonoBehaviour
{
    [SerializeField]
    GameObject _deadZoneObj;
    float _deadZone;
    [SerializeField]
    GameObject _spawnZoneObj;
    float _spawnZone;

    Transform _myTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _myTransform = transform;
        _deadZone = _deadZoneObj.transform.position.z;
        _spawnZone = _spawnZoneObj.transform.position.z;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = _myTransform.position;
        if(pos.z <= _deadZone)
        {
            _myTransform.position = new Vector3(pos.x, pos.y, _spawnZone);
        }
    }
}

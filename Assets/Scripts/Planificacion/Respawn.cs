using Unity.VisualScripting;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField]
    GameObject _spawnZone;

    Vector3 _spawnPosition;

    Transform _myTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _spawnPosition = _spawnZone.transform.position;
        _myTransform = transform;
    }


    public void Spawn()
    {
        Vector3 pos = _myTransform.position;
        _myTransform.position = new Vector3(pos.x, pos.y, _spawnPosition.z);
    }
}

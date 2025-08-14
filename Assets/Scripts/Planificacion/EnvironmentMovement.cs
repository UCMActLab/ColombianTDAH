using UnityEngine;

public class EnvironmentMovement : MonoBehaviour
{
    [SerializeField]
    float _velocity = 100;

    Transform _myTransform;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _myTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        _myTransform.Translate((-_velocity) * Time.deltaTime * Vector3.forward);
    }
}

using UnityEngine;

public class enviroMov : MonoBehaviour
{
    Transform _myTransform;
    [SerializeField]
    float _vel = 0.2f;

    void Start()
    {
        _myTransform = GetComponent<Transform>();
    }

    void FixedUpdate()
    {
        _myTransform.Rotate(0.0f, _vel*1.0f, 0.0f, Space.Self);
    }

    public void SetVelocity(float newVel)
    {
        _vel = newVel;
    }

    public float GetVelocity() {
    
        return _vel;
    }
}
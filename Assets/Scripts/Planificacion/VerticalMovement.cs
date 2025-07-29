using UnityEngine;

public class VerticalMovement : MonoBehaviour
{
    Transform _myTransform;
    Vector3 _targetCameraPos;
    float _springFactor = 1.0f;

    [SerializeField]
    Vector3 _offset;

    // Timer
    [SerializeField]
    float _dirTime = 2.0f;
    float _timeCont = 0.0f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _myTransform = transform;
        _targetCameraPos = _myTransform.position + _offset;
        _timeCont = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {

        if (_timeCont < _dirTime)
        {
            _timeCont += Time.deltaTime;
        }
        else
        {
            _offset = -_offset;
            _targetCameraPos = _myTransform.position + _offset;
            _timeCont = 0.0f;
        }
    }

    private void LateUpdate()
    {
        _myTransform.position = Vector3.Lerp(_myTransform.position, _targetCameraPos, _springFactor * Time.deltaTime);
    }
}

using TMPro;
using UnityEngine;

public class BuildingMouseInteraction : MonoBehaviour
{
    Material _mat;
    Vector3 _initialScale;
    Color _initialColor;
    private void Start()
    {
        _initialScale = transform.localScale;
        _mat = GetComponent<MeshRenderer>().material;
        _initialColor = _mat.color;
    }

    public void OnStartHovering()
    {
        _mat.color = Color.yellow;
        transform.localScale = _initialScale * 1.1f;
    }
    
    public void OnStopHovering()
    {
        _mat.color = _initialColor;
        transform.localScale = _initialScale;
    }
}

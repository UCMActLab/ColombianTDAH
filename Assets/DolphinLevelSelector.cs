using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class DolphinLevelSelector : MonoBehaviour
{
    [SerializeField]
    DolphinLevelSelector _nextLevel = null;

    [Header("Line Renderer")]
    [SerializeField]
    float _offsetY = 0.5f;

    [Header("Texto - ")]

    [SerializeField]
    TextMeshPro _text;

    [SerializeField]
    string _levelNum;

    [SerializeField]
    MeshRenderer _sphereMesh;

    [Header("Colores")]
    [SerializeField]
    static Color _lockedColor = Color.gray;

    [SerializeField]
    static Color _unlockedColor = Color.yellow;

    [SerializeField]
    private bool _isUnlocked = false;

    private bool _isLastUnlockedLevel = false;

    public void ChargeLevel()
    {
        if(_isUnlocked) SceneLoader.LoadScene("DolphinLevel_" + _levelNum);
    }

    public bool IsUnlocked() => _isUnlocked;

    public void UnlockLevel()
    {
        _isUnlocked = true;
        _sphereMesh.material.color = _unlockedColor;
        SetIsLastUnlockedLevel(true);
    }

    public void SetIsLastUnlockedLevel(bool locked) 
    { 
        _isLastUnlockedLevel = locked;
        ActivateLine(!_isLastUnlockedLevel && _isUnlocked);
    }

    public void LockLevel()
    {
        _isUnlocked = false;
        _sphereMesh.material.color = _lockedColor;
        SetIsLastUnlockedLevel(false);
    }

    private void ActivateLine(bool locked)
    {
        LineRenderer _rend = GetComponent<LineRenderer>();
        _rend.enabled = locked;
        if (_rend.enabled)
        {
            ChangeLine();
            _lastPosition = transform.position;
            // ESTA FUNCIÓN ES PROVISIONAL PARA HACER PRUEBAS!!!
            StartCoroutine(ChangeLineTick());
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ActivateLine(_isUnlocked && !_isLastUnlockedLevel);

        if (_text) 
            _text.text = _levelNum;
    }

    // ESTA FUNCIÓN ES PROVISIONAL PARA HACER PRUEBAS!!! se debería borrar
    // ESTA FUNCIÓN ES PROVISIONAL PARA HACER PRUEBAS!!! se debería borrar
    // ESTA FUNCIÓN ES PROVISIONAL PARA HACER PRUEBAS!!! se debería borrar
    // ESTA FUNCIÓN ES PROVISIONAL PARA HACER PRUEBAS!!! se debería borrar
    bool ChangeLine()
    {
        LineRenderer _rend = GetComponent<LineRenderer>();
        if (_rend.enabled = _nextLevel != null && _rend.enabled && !_isLastUnlockedLevel)
        {
            _rend.positionCount = 2;
            _rend.SetPosition(0, this.transform.position + Vector3.up * _offsetY);
            _rend.SetPosition(1, _nextLevel.transform.position + Vector3.up * _offsetY);
            _rend.useWorldSpace = true;
            _rend.textureMode = LineTextureMode.Tile;
        }

        return _rend.enabled;
    }

    Vector3 _lastPosition;
    bool PositionChanged()
    {
        return _lastPosition != transform.position;
    }

    IEnumerator ChangeLineTick()
    {
        while(true)
        {
            yield return new WaitForSeconds(0.05f);
            if (PositionChanged()) 
                ChangeLine();
        }
    }
}

using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;

// El GameObject que tenga este componente puede ser arrastrado
public class Drag : MonoBehaviour
{
    [SerializeField]
    LayerMask _layerMask;

    [SerializeField]
    int _index;

    [SerializeField]
    float _raycastDistance = 10.0f;
    [SerializeField]
    float _delayDragTime = 0.6f;
    float clickTime = 0.0f;

    static bool _isDragging = false;
    bool imDragging = false;
    Camera cam = null;
    Transform _myTransform;
    bool _draggedDolphin;
    Drop _clickedObjectDrop = null;


    float scalerFactor = 1.3f;

    void Start()
    {
        cam = Camera.main;
        _myTransform = transform;
    }

    // Update is called once per frame
    void Update()
    {
        // Click izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            clickTime = Time.time;
        }
        if (Input.GetMouseButton(0) && !_isDragging)
        {
            if (Time.time - clickTime > _delayDragTime)
            {
                _draggedDolphin = DragObject();
            }
        }
        if (Input.GetMouseButtonUp(0) && _isDragging && imDragging)
        {
            if (_draggedDolphin)
            {
                if (_clickedObjectDrop != null)
                    _clickedObjectDrop.DropObject(_index);
                _isDragging = false;
                imDragging = false;
            }
        }
        if (imDragging)
        {
            if (_clickedObjectDrop != null)
                _clickedObjectDrop.PreparingToDrop(_index);
        }
    }

    private bool DragObject()
    {
        Vector2 mousePos = new Vector2();

        mousePos.x = Input.mousePosition.x;
        mousePos.y = Input.mousePosition.y;

        Vector3 point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _raycastDistance));
        Vector3 dir = point - cam.transform.position;
        bool hasHit = Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, Mathf.Infinity, _layerMask);


        // Si hay Objeto que se pueda mover
        if (hasHit && (hit.collider.GetComponent<Drag>().GetIndex() == _index))
        {
            _clickedObjectDrop = hit.collider.gameObject.GetComponent<Drop>();
            _clickedObjectDrop.ObjectClick(_myTransform.position.y);
            _myTransform.localScale = _myTransform.localScale * scalerFactor; // Escala
            _isDragging = true;
            imDragging = true;
        }

        return hasHit;
    }

    public int GetIndex()
    {
        return _index;
    }

    public void DeactivateDrag()
    {
        _isDragging = false;
        imDragging = false;
        GetComponent<Drop>().Belittle();
    }
}

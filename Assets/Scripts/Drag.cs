using System.Dynamic;
using UnityEngine;
using UnityEngine.UI;

// El GameObject que tenga este componente puede ser arrastrado
public class Drag : MonoBehaviour
{
    LayerMask _layerMask;

    int _index;

    [SerializeField]
    float _raycastDistance = 10.0f;
    [SerializeField]
    float _delayDragTime = 0.05f;
    float clickTime = 0.0f;

    static bool _isDragging = false;
    bool imDragging = false;
    Camera cam = null;
    Transform _myTransform;
    bool _dolphinClicked = false;
    Drop _clickedObjectDrop = null;


    float scalerFactor = 1.3f;

    void Start()
    {
        cam = Camera.main;
        _myTransform = transform;
        _layerMask = LayerMask.GetMask("Dolphins");
    }

    // Update is called once per frame
    void Update()
    {
        // Click izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            // Si hay Objeto que se pueda mover
            if (IsDolphinHit())
            {
                clickTime = Time.time;
                _dolphinClicked = true;
            }
        }
        if (Input.GetMouseButton(0) && !_isDragging && _dolphinClicked)
        {
            // Si mantiene pulsado encima del delfin
            if (IsDolphinHit())
            {
                // Si pasa el tiempo del delay arrastra
                if (Time.time - clickTime > _delayDragTime)
                {
                    DragObject();
                }
            }
            // Si no mantiene pulsado sobre el delfin el contador se reinicia
            else
                _dolphinClicked = false;

        }
        if (Input.GetMouseButtonUp(0) && _isDragging && imDragging)
        {

            if (_clickedObjectDrop != null)
                _clickedObjectDrop.DropObject(_index);
            _isDragging = false;
            imDragging = false;
            _dolphinClicked = false;
        }
        if (imDragging)
        {
            if (_clickedObjectDrop != null)
                _clickedObjectDrop.PreparingToDrop(_index);
        }
    }

    private void DragObject()
    {
        _clickedObjectDrop.ObjectClick(_myTransform.position.y);
        _myTransform.localScale = _myTransform.localScale * scalerFactor; // Escala
        _isDragging = true;
        imDragging = true;
    }

    private bool IsDolphinHit()
    {
        Vector2 mousePos = new Vector2();

        mousePos.x = Input.mousePosition.x;
        mousePos.y = Input.mousePosition.y;

        Vector3 point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _raycastDistance));
        Vector3 dir = point - cam.transform.position;
        bool hasHit = Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, Mathf.Infinity, _layerMask);

        if (hasHit)
            _clickedObjectDrop = hit.collider.gameObject.GetComponent<Drop>();

        // Si hay Objeto que se pueda mover
        return (hasHit && (hit.collider.GetComponent<Drag>().GetIndex() == _index));
    }

    public int GetIndex()
    {
        return _index;
    }
    public void SetIndex(int index)
    {
        _index = index;
    }
    public bool AmIBeingDragged()
    {
        return imDragging;
    }
    public void DeactivateDrag()
    {
        // Si el delfin que estaba arrastrando va a bucear lo suelto
        if (imDragging)
        {
            _isDragging = false;
            imDragging = false;
            _dolphinClicked = false;
            GetComponent<Drop>().Belittle();
        }
        //GetComponent<DolphinController>().ClearMatrixOccupation(); //vaciamos lugar en matriz
    }
}

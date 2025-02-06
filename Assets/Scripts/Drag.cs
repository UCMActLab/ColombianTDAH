using UnityEngine;
using UnityEngine.UI;

// El GameObject que tenga este componente puede ser arrastrado
public class Drag : MonoBehaviour
{
    [SerializeField]
    LayerMask _layerMask;
    LayerMask _dropLayer;

    [SerializeField]
    float _raycastDistance = 10.0f;

    bool _isDragging = false;
    Camera cam = null;
    GameObject _dropPlane = null;
    Transform _myTransform;

    void Start()
    {
        cam = Camera.main;
        _myTransform = transform;
        _dropLayer = LayerMask.GetMask("Drop");

        // Plano
        _dropPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        _dropPlane.transform.position = Vector3.zero;
        _dropPlane.transform.localScale = new Vector3(3,3,3);
        _dropPlane.layer = 7; // Layer de drop
        _dropPlane.GetComponent<MeshRenderer>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Click izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            DragObject();
        }
        if (Input.GetMouseButtonUp(0) && _isDragging)
        {
            DropObject();
        }
    }

    private void DragObject()
    {
        Vector2 mousePos = new Vector2();

        mousePos.x = Input.mousePosition.x;
        mousePos.y = Input.mousePosition.y;

        Vector3 point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _raycastDistance));
        Vector3 dir = point - cam.transform.position;
        bool hasHit = Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, Mathf.Infinity, _layerMask);

        // Si hay Objeto que se pueda mover
        if (hasHit)
        {
            Debug.Log("Ha clicado el delfinn");

            // Cambio posicion.y del plano para saber la altura a la que esta cuando clica sobre el delfin
            Vector3 planePos = _dropPlane.transform.position;
            _dropPlane.transform.position = new Vector3(planePos.x, _myTransform.position.y, planePos.z);
            _isDragging = true;
        }
    }

    private void DropObject()
    {
        Debug.Log("He dejado el delfin");
        Vector2 mousePos = new Vector2();

        mousePos.x = Input.mousePosition.x;
        mousePos.y = Input.mousePosition.y;

        Vector3 point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _raycastDistance));
        Vector3 dir = point - cam.transform.position;
        bool hasHit = Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, Mathf.Infinity, _dropLayer);

        _myTransform.position = hit.point;
        _isDragging = false;
    }
}

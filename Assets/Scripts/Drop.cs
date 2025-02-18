using UnityEngine;

public class Drop : MonoBehaviour
{

    Material _material;
    Transform _myTransform;
    Camera _camera;

    [SerializeField]
    float _raycastDistance = 10.0f;

    // Drop
    GameObject _dropPlane = null;
    LayerMask _dropLayer;
    LayerMask _matrixLayer;

    // DragComp
    Drag _dragComponent = null;
    int _index;

    // Sizes
    Vector3 _initialScale;
    [SerializeField]
    Vector3 riverDropSize;
    [SerializeField]
    Vector3 riverDropOffset;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _myTransform = transform;
        _camera = Camera.main;
        _dragComponent = GetComponent<Drag>();
        _index = _dragComponent.GetIndex();

        // Layers
        _dropLayer = LayerMask.GetMask("Drop");
        _matrixLayer = LayerMask.GetMask("Matrix");

        //Plano
        _dropPlane = GameObject.CreatePrimitive(PrimitiveType.Cube);
        _dropPlane.transform.position = riverDropOffset;
        _dropPlane.transform.localScale = riverDropSize;
        _dropPlane.layer = 7; // Layer de drop
        _dropPlane.name = _index.ToString();
        _dropPlane.GetComponent<MeshRenderer>().enabled = false; // Invisible
        _dropPlane.GetComponent<Collider>().isTrigger = true;
        _initialScale = _myTransform.localScale;
    }

    public void DropObject(int ind)
    {
        Belittle();

        // Centra posicion
        Vector3 dir = _myTransform.position - _camera.transform.position;
        bool hasHit = Physics.Raycast(_camera.transform.position, dir, out RaycastHit hit, Mathf.Infinity, _matrixLayer);

        if (hasHit) {
            _myTransform.position = new Vector3(hit.transform.position.x, _dropPlane.transform.position.y, hit.transform.position.z);
        }
    }

    public void PreparingToDrop(int ind)
    {
        Vector2 mousePos = new Vector2();

        mousePos.x = Input.mousePosition.x;
        mousePos.y = Input.mousePosition.y;

        Vector3 point = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _raycastDistance));
        Vector3 dir = point - _camera.transform.position;

        RaycastHit[] hits;
        hits = Physics.RaycastAll(_camera.transform.position, dir, Mathf.Infinity, _dropLayer);

        // De todos los planos miro cual es el suyo
        RaycastHit hit = new RaycastHit();
        string name = "";
        for (int i = 0; i < hits.Length && (name != ind.ToString()); i++)
        {
            hit = hits[i];
            name = hit.collider.name;
        }

        if (name == ind.ToString())
        {
            //  Coloco delfin en la posicion a la del plano
            _myTransform.position = new Vector3(hit.point.x, _dropPlane.transform.position.y, hit.point.z);
        }
    }

    public void ObjectClick(float dropHigh)
    {
        // Cambio alttura del plano a la altura del delfin
        Vector3 planePos = _dropPlane.transform.position;
        _dropPlane.transform.position = new Vector3(planePos.x, _myTransform.position.y, planePos.z);
    }

    public void Belittle()
    {
        _myTransform.localScale = _initialScale;
    }
}

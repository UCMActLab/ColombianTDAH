using UnityEngine;

public class Drop : MonoBehaviour
{

    Material _material;
    Transform _myTransform;
    Camera _camera;

    [SerializeField]
    float _raycastDistance = 10.0f;

    [SerializeField]
    int _index;

    LayerMask _dropLayer;
    GameObject _dropPlane = null;

    Vector3 _initialScale;
    Color _initialColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _material = GetComponent<MeshRenderer>().material;
        _myTransform = transform;
        _camera = Camera.main;

        _dropLayer = LayerMask.GetMask("Drop");

        //Plano
        _dropPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        _dropPlane.transform.position = Vector3.zero;
        _dropPlane.transform.localScale = new Vector3(3, 3, 3);
        _dropPlane.layer = 7; // Layer de drop
        _dropPlane.name = _index.ToString();
        _dropPlane.GetComponent<MeshRenderer>().enabled = false; // Invisible
        _initialColor = _material.color;
        _initialScale = _myTransform.localScale;
    }

    public void DropObject(int ind)
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

        //  Coloco delfin en la posicion del plano
        _myTransform.position = hit.point;
        _material.color = _initialColor;
        _myTransform.localScale = _initialScale;
    }

    public void ObjectClick(float dropHigh)
    {
        // Cambio alttura del plano a la altura del delfin
        Vector3 planePos = _dropPlane.transform.position;
        _dropPlane.transform.position = new Vector3(planePos.x, _myTransform.position.y, planePos.z);

        _material.color = Color.blue; // Color
        _myTransform.localScale = _myTransform.localScale * 1.2f; // Escala
    }
}

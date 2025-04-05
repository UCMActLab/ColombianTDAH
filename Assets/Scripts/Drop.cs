using JetBrains.Annotations;
using System.Collections;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;

public class Drop : MonoBehaviour
{
    [SerializeField]
    GameObject _clickCollider;
    Transform _myTransform;
    Camera _camera;
    Vector3 _initialPosition;
    MatrixCubeInfo _matrixCubeInfo;

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
    [SerializeField]
    Vector2 margin;
    Vector3 _initialScale;
    Vector3 riverDropSize;
    Vector3 riverDropOffset;

    float _dolphinHighOffset = 0.3f;

    //Resposition info dolphin
    bool _isBeingRepositioned;
    Vector3 _rePos;
    [SerializeField]
    float rePosSpeed = 0.3f;
    float startReposTime = 0;
    float journeyLength = 0;



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _myTransform = transform;
        SetInitialPosition();
        _camera = Camera.main;
        _dragComponent = GetComponent<Drag>();
        _index = _dragComponent.GetIndex();
        _matrixCubeInfo = GetComponent<MatrixCubeInfo>();
        riverDropSize = DolphinLevelManager.Instance.GetRiverSize() - new Vector3(margin.x, 0, margin.y);
        riverDropOffset = DolphinLevelManager.Instance.GetRiverOffset();

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

        //Reposition after forced drop
        _rePos = Vector3.zero;
        _isBeingRepositioned = false;
       
    }
    private void Update()
    {
        if(_isBeingRepositioned) // Reposicionamento progresivo tras drop en flotador al centro de la casilla
        {
            float distCovered = (Time.time - startReposTime) * rePosSpeed;
            float fractionOfJourney = distCovered / journeyLength;
            transform.position = Vector3.Lerp(transform.position, _rePos, fractionOfJourney);

            if (_rePos.x - _myTransform.position.x == 0)
            {
                _isBeingRepositioned = false;
                _myTransform.position = _rePos;
            }
        }
    }

    public void DropObject(int ind)
    {
        Belittle();

        Vector3 mousePos = Input.mousePosition;

        Vector3 point = _camera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _raycastDistance));
        Vector3 dir = point - _camera.transform.position;

        // Centra posicion
        bool hasHit = Physics.Raycast(_camera.transform.position, dir, out RaycastHit hit, Mathf.Infinity, _matrixLayer);

        // Si ha dejado fuera de la zona drop el delfin se mira pos de delfin en vez de raton
        if (!hasHit)
        {
            dir = _myTransform.position - _camera.transform.position;
            Physics.Raycast(_camera.transform.position, dir, out RaycastHit hitInfo, Mathf.Infinity, _matrixLayer);
            hit = hitInfo;
        }

        // Si esta vacia o hay un flotador en la casilla cambio posicion y ocupo casilla
        Vector2 cubePosInMatrix = hit.collider.GetComponent<MatrixCubeInfo>().GetXY();
        Box type = DolphinLevelManager.Instance.GetOccupationFromMatrix((int)cubePosInMatrix.x, (int)cubePosInMatrix.y);
        if (type == Box.Empty || type == Box.Floatie)
        {
            //if (type == Box.Floatie) // El flotador liberar� su hueco para el delf�n y tomar� ya XY cuando entre en el trigger del siguiente hueco
            //{
            //    GameObject obj = DolphinLevelManager.Instance.GetCubeFromMatrix((int)cubePosInMatrix.x, (int)cubePosInMatrix.y);
            //    obj.GetComponent<MatrixCubeInfo>().SetXY(-1, -1);
            //}

            // Desocupo antigua casilla
            Vector2 dolphinMatrixPos = _matrixCubeInfo.GetXY();
            DolphinLevelManager.Instance.SetOccupation((int)dolphinMatrixPos.x, (int)dolphinMatrixPos.y, Box.Empty);

            // Ocupo nueva casilla
            DolphinLevelManager.Instance.SetOccupation((int)cubePosInMatrix.x, (int)cubePosInMatrix.y, Box.Dolphin);
            _myTransform.position = new Vector3(hit.transform.position.x, _dropPlane.transform.position.y, hit.transform.position.z);
            _matrixCubeInfo.SetXY((int)cubePosInMatrix.x, (int)cubePosInMatrix.y);

            // Guardo nueva posicion
            SetInitialPosition();

        }
        // Si no vuelvo a posicion inicial
        else
            _myTransform.position = _initialPosition;

    }

    public bool DropForceOnOBj(GameObject obj) //Para droppear exactamente en un objeto en el plano (ej. flotadores), no se centra en el cubo 
    {
        // Desocupo antigua casilla
        Vector2 dolphinMatrixPos = _matrixCubeInfo.GetXY();
        Vector2 cubePosInMatrix = obj.GetComponent<MatrixCubeInfo>().GetXY();

       Box type = DolphinLevelManager.Instance.GetOccupationFromMatrix((int)cubePosInMatrix.x, (int)cubePosInMatrix.y);
        if (type == Box.Empty || type == Box.Floatie)
        {
            DolphinLevelManager.Instance.SetOccupation((int)dolphinMatrixPos.x, (int)dolphinMatrixPos.y, Box.Empty);
            // Ocupo nueva casilla
            DolphinLevelManager.Instance.SetOccupation((int)cubePosInMatrix.x, (int)cubePosInMatrix.y, Box.Dolphin);
            Vector3 newPos = DolphinLevelManager.Instance.GetWorldPositionFromCube((int)cubePosInMatrix.x, (int)cubePosInMatrix.y);
            _myTransform.position = new Vector3(obj.transform.position.x, _dropPlane.transform.position.y, newPos.z); //obj.trans.x
            _rePos = new Vector3(newPos.x, _dropPlane.transform.position.y, newPos.z);
            StartCoroutine("RePositionDolphin");
            _matrixCubeInfo.SetXY((int)cubePosInMatrix.x, (int)cubePosInMatrix.y);
            _initialPosition = _myTransform.position;
            return true;
        }
        else
        {
            _myTransform.position = _initialPosition;
            return false;
        }
    }

    IEnumerator RePositionDolphin() //Colocar bien el delfin a mitad de posicion en el centro del cubo
    {
        journeyLength = Vector3.Distance(transform.position, _rePos);
        yield return new WaitForSeconds(1.0f);
        startReposTime = Time.time;
        _isBeingRepositioned = true;
    }

    public void PreparingToDrop(int ind)
    {
        Vector3 mousePos = Input.mousePosition;

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
            _myTransform.position = new Vector3(hit.point.x, _dropPlane.transform.position.y - _dolphinHighOffset, hit.point.z);

            // Recoloca dependiendo de la posicion del collider por la animacion
            Vector3 posCol = _clickCollider.transform.localPosition;
            if (posCol != Vector3.zero)
            {
                _myTransform.position -= posCol;
            }
        }
        _isBeingRepositioned = false;
    }

    public void ObjectClick(float dropHigh)
    {
        // Cambio alttura del plano a la altura del delfin
        Vector3 planePos = _dropPlane.transform.position;
        _dropPlane.transform.position = new Vector3(planePos.x, _myTransform.position.y, planePos.z);
    }

    // Vuelve a su tamanyo normal
    public void Belittle()
    {
        _myTransform.localScale = _initialScale;
    }

    public void SetInitialPosition(Vector3 pos)
    {
        _initialPosition = pos;
    }
    public void SetInitialPosition()
    {
        _initialPosition = transform.position;
    }
}

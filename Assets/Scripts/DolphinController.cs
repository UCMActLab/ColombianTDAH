using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DolphinController : MonoBehaviour
{
    protected Animator animator;
    protected Buceo buceoComponent;
    protected DolphinManager dolphinMngr;
    protected Drag dragComponent;
    public enum DolphinStates { FLOATING, DIVING, JUMPING, SPECIALJUMPING };
    public DolphinStates currentState;
    public DolphinStates startingState;
    bool isAboutToDive;

    AudioSource _myAudioSource;

    [SerializeField, Tooltip("Capa con la que querremos clicar la vuelta especial (delfines)")]
    LayerMask _layerMask;
    bool hasBeenHit;

    [SerializeField]
    float _raycastDistance = 10.0f;

    [SerializeField]
    GameObject _pointsTextPrefab;
    [SerializeField]
    float _pointsTextLifeTime;

    [SerializeField, Tooltip("Área que detecta click delfín")]
    GameObject _colliderClickDolphin;
    float riverFloatingHeight;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        buceoComponent = GetComponent<Buceo>();
        dragComponent = GetComponent<Drag>();
        _myAudioSource = GetComponent<AudioSource>();
        hasBeenHit = false;
        _colliderClickDolphin.SetActive(false);
        currentState = DolphinStates.FLOATING; //default, ajustar para que detecte si está arriba o no (por posición o diseño de nivel)
        isAboutToDive = false;

        transform.Rotate(new Vector3(0, 90, 0));
        riverFloatingHeight = DolphinLevelManager.Instance.GetRiverFloatingHeight();

        //Si su estado es diving se va
        if(startingState == DolphinStates.DIVING)
        {
            Dive();
        }
    }

    public void SetStartingState(DolphinStates state)
    {
        startingState = state;
    }
    public void RegisterDolphinManager(DolphinManager mngr)
    {
        dolphinMngr = mngr;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if (_colliderClickDolphin.activeSelf)
        {
            BoxCollider boxCollider = _colliderClickDolphin.GetComponent<BoxCollider>();
            Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.bounds.size);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Debug.Log("AUTX!");
        Dive();
    }

    public DolphinStates getDolphinState()
    {
        return currentState;
    }
    public bool Jump()
    {
        if (currentState == DolphinStates.FLOATING && !dragComponent.AmIBeingDragged())
        {
            currentState = DolphinStates.JUMPING;
            animator.SetTrigger("Jump");
            _colliderClickDolphin.SetActive(true);
            //Debug.Log(currentState);
            return true;
        }
        return false;
    }
    public bool SpecialJump()
    {
        if (currentState == DolphinStates.FLOATING && !dragComponent.AmIBeingDragged())
        {
            currentState = DolphinStates.SPECIALJUMPING;
            animator.SetTrigger("SpecialJump");
            _colliderClickDolphin.SetActive(true);
            return true;
        }
        return false;
    }

    public void Dive() //+ probablemente haya que settear la ocupacion del cubo en la matriz (que ahora solo sabe el drop) a 0
    {
        isAboutToDive = true;
        buceoComponent.enabled = true;
        dragComponent.DeactivateDrag();
        dragComponent.enabled = false; //esto dependerá de cómo juntemos input, falta que se enabelee
        buceoComponent.SetPath(float3.zero);
        ClearMatrixOccupation();
        currentState = DolphinStates.DIVING;
    }
    public void ClearMatrixOccupation()
    {
        //vaciamos lugar en matriz
        Vector2 dolphinMatrixPos = GetComponent<MatrixCubeInfo>().GetXY();
        if (dolphinMatrixPos.x != -1)
        {
            DolphinLevelManager.Instance.SetOccupation((int)dolphinMatrixPos.x, (int)dolphinMatrixPos.y, Box.Empty);
        }
        //seteamos a no en matriz
        GetComponent<MatrixCubeInfo>().SetXY(-1, -1);

    }
    public bool Float() //-------------------------------
    {
        if (currentState == DolphinStates.DIVING)
        {
            //transform.position.x, 0, transform.position.z
            Vector2 matrixPos = DolphinLevelManager.Instance.GetNextAvailableMatrixSpot(this.transform.position);
            float3 pos = (float3)DolphinLevelManager.Instance.GetWorldPositionFromCube((int)matrixPos.x, (int)matrixPos.y);
            pos = new float3(pos.x, riverFloatingHeight, pos.z);
            buceoComponent.SetPath(pos);
            GetComponent<MatrixCubeInfo>().SetXY((int)matrixPos.x, (int)matrixPos.y);
            currentState = DolphinStates.FLOATING;
            isAboutToDive = false;
            this.GetComponent<Drag>().enabled = true;
            return true;
        }
        else return false;
    }
    public void OnAnimationEnded(string action) //función que se llama en evento de fin de animación 
    {
        switch (action)
        {
            case "Jump":
                //Debug.Log("ive ended jumping");
                break;
            case "Roll":
                hasBeenHit = false;
                //Debug.Log("Ive ended rolling");
                break;
        }
        _colliderClickDolphin.SetActive(false);
        if (isAboutToDive) { currentState = DolphinStates.DIVING; }
        else currentState = DolphinStates.FLOATING;
    }


    public void TryClickDolphin() //comprobacion un poco provisional (no se si el onmouseover aquí se podría reutilizar también)
    {
        Camera cam = Camera.main;
        Vector2 mousePos = new Vector2();

        mousePos.x = Input.mousePosition.x;
        mousePos.y = Input.mousePosition.y;

        Vector3 point = cam.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, _raycastDistance));
        Vector3 dir = point - cam.transform.position;
        bool hasHit = Physics.Raycast(cam.transform.position, dir, out RaycastHit hit, Mathf.Infinity, _layerMask);

        Debug.DrawRay(cam.transform.position, dir, UnityEngine.Color.yellow);

        if (hasHit && (hit.transform.gameObject == _colliderClickDolphin))
        {
            Debug.Log("I clicked the collider (jumping o rolling)");

            if (currentState == DolphinStates.SPECIALJUMPING && !hasBeenHit) //RIGHT GUESS SPECIAL JUMP
            {
                //Debug.Log("HIT 30000000 POINTS");

                hasBeenHit = true;
                _colliderClickDolphin.SetActive(false);

                // Dolphin sound
                _myAudioSource.Play();

                //In world points text
                int plusPoints = dolphinMngr.RightGuess();
                Vector3 offsetHeight = new Vector3(0.0f, 2.0f, 0.0f);
                GameObject pointsTetx = Instantiate(_pointsTextPrefab, transform.position + offsetHeight, Quaternion.identity);
                pointsTetx.GetComponentInChildren<TextMeshProUGUI>().SetText(plusPoints.ToString());

                Destroy(pointsTetx, _pointsTextLifeTime);
            }
            else if (currentState == DolphinStates.JUMPING && !dragComponent.AmIBeingDragged()) //WRONG GUESS SPECIAL JUMP
            {

                //In world points text
                int lessPoints = dolphinMngr.WrongGuess();
                Vector3 offsetHeight = new Vector3(0.0f, 2.0f, 0.0f);
                GameObject pointsTetx = Instantiate(_pointsTextPrefab, transform.position + offsetHeight, Quaternion.identity);
                pointsTetx.GetComponentInChildren<TextMeshProUGUI>().SetText(lessPoints.ToString());
                pointsTetx.GetComponentInChildren<TextMeshProUGUI>().color = Color.red;

                Destroy(pointsTetx, _pointsTextLifeTime);

                // Desactiva Velocidad aumentada
                DolphinLevelManager.Instance.DeactivateIncreasedSpeed();
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        // Si levanta click izquierdo
        if (Input.GetMouseButtonUp(0))
        {
            TryClickDolphin();
        }
    }
}

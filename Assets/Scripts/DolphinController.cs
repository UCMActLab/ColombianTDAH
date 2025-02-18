using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using Unity.Mathematics;

public class DolphinController : MonoBehaviour
{
    protected Animator animator;
    protected Buceo buceoComponent;
    protected DolphinManager dolphinMngr;
    public enum DolphinStates { FLOATING, DIVING, JUMPING, SPECIALJUMPING };
    DolphinStates currentState;


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


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        buceoComponent = GetComponent<Buceo>();
        hasBeenHit = false;
        _colliderClickDolphin.SetActive(false);
        currentState = DolphinStates.FLOATING; //default, ajustar para que detecte si está arriba o no (por posición o diseño de nivel)
    }

    public void registerDolphinManager(DolphinManager mngr)
    {
        dolphinMngr = mngr;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        if(_colliderClickDolphin.activeSelf)
        {
            BoxCollider boxCollider = _colliderClickDolphin.GetComponent<BoxCollider>();
            Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.bounds.size);
        }
    }

    public DolphinStates getDolphinState()
    {
        return currentState;
    }
    public bool Jump()
    {
        if(currentState==DolphinStates.FLOATING)
        {
           currentState = DolphinStates.JUMPING;
            animator.SetTrigger("Jump");
            _colliderClickDolphin.SetActive(true);
            return true;
        }
        return false;
    }
    public bool SpecialJump()
    {
        if (currentState == DolphinStates.FLOATING)
        {
            currentState = DolphinStates.SPECIALJUMPING;
            animator.SetTrigger("SpecialJump");
            _colliderClickDolphin.SetActive(true);
            return true;
        }
        return false;
    }

    public void Dive()
    {
        buceoComponent.enabled = true;
        this.GetComponent<Drag>().enabled = false; //esto dependerá de cómo juntemos input, falta que se enabelee
        buceoComponent.SetPath(float3.zero);
        currentState = DolphinStates.DIVING;
    }

    public void Float() //-------------------------------
    {
        float3 pos = new float3(0, 0, 4);
        buceoComponent.SetPath(pos);
        currentState=DolphinStates.FLOATING;
    }
    public void OnAnimationEnded(string action) //función que se llama en evento de fin de animación 
    {
        switch(action)
        {
            case "Jump":
                Debug.Log("ive ended jumping");
                currentState = DolphinStates.FLOATING;
                break;
            case "Roll":
                hasBeenHit = false;
                Debug.Log("Ive ended rolling");
                currentState = DolphinStates.FLOATING;
                break;
        }
        _colliderClickDolphin.SetActive(false);

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

                if (currentState == DolphinStates.SPECIALJUMPING && !hasBeenHit)
                {
                    Debug.Log("HIT 30000000 POINTS");

                    hasBeenHit = true;
                    _colliderClickDolphin.SetActive(false); //esto hace que hasbeenhit no sea necesario
                    
                    //creacion texto in world con puntos por la acción
                    int plusPoints = dolphinMngr.rightGuess();
                    Vector3 offsetHeight = new Vector3(0.0f, 2.0f, 0.0f);
                    GameObject pointsTetx = Instantiate(_pointsTextPrefab, transform.position + offsetHeight, Quaternion.identity, transform);
                    pointsTetx.GetComponentInChildren<TextMeshProUGUI>().SetText(plusPoints.ToString());
                    Destroy(pointsTetx, _pointsTextLifeTime);
                }
                else
                {   
                    //haptic info, pirueta
                }
            }
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0)) //haciendo pruebas
        {
            TryClickDolphin();
        }
        //if (Input.GetKeyDown(KeyCode.Mouse1)) //haciendo pruebas
        //{
        //    SpecialJump();
        //}
    }
}

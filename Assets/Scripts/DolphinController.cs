using TMPro;
using UnityEngine;

public class DolphinController : MonoBehaviour
{
    protected Animator animator;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    bool isJumping;
    bool isPirueting;
    bool hasBeenHit;

    

    [SerializeField, Tooltip("Capa con la que querremos que colisione (delfines)")]
    LayerMask _layerMask;

    [SerializeField]
    float _raycastDistance = 10.0f;

    [SerializeField]
    GameObject _pointsTextPrefab;
    [SerializeField]
    float _pointsTextLifeTime;

    void Start()
    {
        animator = GetComponent<Animator>();
        hasBeenHit = false;
    }

    public void Jump()
    {
        isJumping = true;
        animator.SetTrigger("Jump");
    }
    public void SpecialJump()
    {
        isPirueting = true;
        animator.SetTrigger("SpecialJump");
    }

    public void OnAnimationEnded(string action) //función que se llama en evento de fin de animación 
    {
        switch(action)
        {
            case "Jump":
                isJumping = false; //no tenemos en cuenta tiempo de fade
                break;
            case "Roll":
                isPirueting = false; hasBeenHit = false;
                break;
        }
        
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

            if (hasHit && (hit.transform.gameObject ==this.gameObject))
            {
                if (isPirueting&&!hasBeenHit)
                {
                    hasBeenHit = true;
                    Debug.Log("HIT 30000000 POINTS");
                    Vector3 offsetHeight = new Vector3(0.0f, 2.0f, 0.0f);
                    //texto provisional de puntos (que pasaría si hay mas de un texto?) (estan contenidos en un mismo canvas que se instancia? o varios?)
                    GameObject pointsTetx = Instantiate(_pointsTextPrefab, this.GetComponent<Transform>().transform.position + offsetHeight, Quaternion.identity, this.GetComponent<Transform>().transform);
                    Destroy(pointsTetx, _pointsTextLifeTime);
                    pointsTetx.GetComponentInChildren<Rigidbody2D>().linearVelocityY = 0.5f;
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

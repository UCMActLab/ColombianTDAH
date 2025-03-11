using TMPro;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering.Universal;
using System;
using System.Collections.Generic;

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

    [SerializeField]
    protected bool canBeDamaged;
    [SerializeField]
    protected float invincibilityTime = 2.0f;
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

    [SerializeField, Tooltip("�rea que detecta click delf�n")]
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
        currentState = DolphinStates.FLOATING; //default, ajustar para que detecte si est� arriba o no (por posici�n o dise�o de nivel)
        isAboutToDive = false;
        canBeDamaged = true;

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
        if(collision.gameObject.GetComponent<Obstaculo>())
        {
            OnHitObstacle();
            //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.OColision, GetComponent<Drag>().GetIndex().ToString("00")));
            //EventRegister.Instance.EvntToJson();
        }
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.GetComponent<Floatie>())
        {
            OnHitFloatie();
            //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.FColision, GetComponent<Drag>().GetIndex().ToString("00")));
            //EventRegister.Instance.EvntToJson();
        }
    }
    protected void OnHitObstacle()
    {
        if (canBeDamaged)
        {
            StartCoroutine("PauseDamage");
            Dive();
            int points = dolphinMngr.HitObstacle();
            showPointsOnDolphin(points, Color.red);
        }
    }
    protected void OnHitFloatie()
    {
        int points = dolphinMngr.FloatHit();
        showPointsOnDolphin(points, Color.green);
        //Dive();
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
            //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DSaltoInit, GetComponent<Drag>().GetIndex().ToString("00")));
            //EventRegister.Instance.EvntToJson();
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
            //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DPiruetaInit, GetComponent<Drag>().GetIndex().ToString("00")));
            //EventRegister.Instance.EvntToJson();
            return true;
        }
        return false;
    }

    public void Dive() //+ probablemente haya que settear la ocupacion del cubo en la matriz (que ahora solo sabe el drop) a 0
    {
        isAboutToDive = true;
        buceoComponent.enabled = true;
        //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DSaleSuperficie, GetComponent<Drag>().GetIndex().ToString("00")));
        //EventRegister.Instance.EvntToJson();
        dragComponent.DeactivateDrag();
        dragComponent.enabled = false; //esto depender� de c�mo juntemos input, falta que se enabelee
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
            //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DEntraSuperficie, GetComponent<Drag>().GetIndex().ToString("00")));
            //EventRegister.Instance.EvntToJson();
            PauseDamage();
            return true;
        }
        else return false;
    }
    public void OnAnimationEnded(string action) //funci�n que se llama en evento de fin de animaci�n 
    {
        switch (action)
        {
            case "Jump":
                //Debug.Log("ive ended jumping");
                //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DSaltoFin, GetComponent<Drag>().GetIndex().ToString("00")));
                //EventRegister.Instance.EvntToJson();
                break;
            case "Roll":
                hasBeenHit = false;
                //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DPiruetaFin, GetComponent<Drag>().GetIndex().ToString("00")));
                //EventRegister.Instance.EvntToJson();
                //Debug.Log("Ive ended rolling");
                break;
        }
        _colliderClickDolphin.SetActive(false);
        if (isAboutToDive) { currentState = DolphinStates.DIVING; }
        else currentState = DolphinStates.FLOATING;
    }


    public void TryClickDolphin() //comprobacion un poco provisional (no se si el onmouseover aqu� se podr�a reutilizar tambi�n)
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

            if (currentState == DolphinStates.SPECIALJUMPING && !hasBeenHit) //RIGHT GUESS SPECIAL JUMP
            {
                //Deactivate jump collider
                hasBeenHit = true;
                _colliderClickDolphin.SetActive(false);

                // Dolphin sound
                _myAudioSource.Play();

                //In world points text
                //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.RespuestaCorrecta, GetComponent<Drag>().GetIndex().ToString("00")));
                int plusPoints = dolphinMngr.RightGuess();
                showPointsOnDolphin(plusPoints, Color.green);
            }
            else if (currentState == DolphinStates.JUMPING && !dragComponent.AmIBeingDragged()) //WRONG GUESS SPECIAL JUMP
            {
                //In world points text
                //EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.RespuestaIncorrecta, GetComponent<Drag>().GetIndex().ToString("00")));
                int lessPoints = dolphinMngr.WrongGuess();
                showPointsOnDolphin(lessPoints, Color.red);

                // Desactiva Velocidad aumentada
                DolphinLevelManager.Instance.DeactivateIncreasedSpeed();
            }
        }
    }

    void showPointsOnDolphin(int points, Color col)
    {
        Vector3 offsetHeight = new Vector3(0.0f, 2.0f, 0.0f);
        GameObject pointsTetx = Instantiate(_pointsTextPrefab, transform.position + offsetHeight, Quaternion.identity);
        pointsTetx.GetComponentInChildren<TextMeshProUGUI>().SetText(points.ToString());
        pointsTetx.GetComponentInChildren<TextMeshProUGUI>().color = col;
        Destroy(pointsTetx, _pointsTextLifeTime);
    }

    IEnumerator PauseDamage()
    {
        canBeDamaged = false;
        yield return new WaitForSeconds(invincibilityTime);
        canBeDamaged = true;
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

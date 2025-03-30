using TMPro;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using System;

public class DolphinController : MonoBehaviour
{
    protected Animator animator;
    protected Buceo buceoComponent;
    protected DolphinManager dolphinMngr;
    protected Drag dragComponent;
    public enum DolphinStates { FLOATING, DIVING, JUMPING, SPECIALJUMPING, FLOATIEJUMPING};
    public DolphinStates currentState;
    public DolphinStates startingState;
    bool isAboutToDive;
    bool scoringFloatie;

    [SerializeField]
    protected bool canBeDamaged;
    [SerializeField]
    protected float invincibilityTime = 2.0f;
    AudioSource _myAudioSource;

    [SerializeField, Tooltip("Capa con la que querremos clicar la vuelta especial (delfines)")]
    LayerMask _layerMask;

    [SerializeField]
    float _raycastDistance = 10.0f;

    [SerializeField]
    GameObject _pointsTextPrefab;
    [SerializeField]
    float _pointsTextLifeTime;

    [SerializeField, Tooltip("�rea que detecta click delf�n")]
    GameObject _colliderClickDolphin;
    float riverFloatingHeight;

    int _index;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
        buceoComponent = GetComponent<Buceo>();
        dragComponent = GetComponent<Drag>();
        _myAudioSource = GetComponent<AudioSource>();
        currentState = DolphinStates.FLOATING; //default, ajustar para que detecte si est� arriba o no (por posici�n o dise�o de nivel)
        isAboutToDive = false;
        canBeDamaged = true;
        scoringFloatie = false;

        transform.Rotate(new Vector3(0, 90, 0));
        riverFloatingHeight = DolphinLevelManager.Instance.GetRiverFloatingHeight();

        //Si su estado es diving se va
        if (startingState == DolphinStates.DIVING)
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
        if (collision.gameObject.GetComponent<Obstaculo>())
        {
            OnHitObstacle();
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.OColision, _index.ToString("00")));
            EventRegister.Instance.EvntToJson();
        }
    }

    private void OnTriggerStay(Collider other)
    {       
        Floatie f = other.gameObject.GetComponentInParent<Floatie>();
        if (f != null)
        {
            if (!scoringFloatie)
            {
                if (other.gameObject.CompareTag("JumpTrigger"))
                {
                    if (!dragComponent.AmIBeingDragged() && f.TryScore(dragComponent.GetIndex()))
                    {
                        FloatieTrick(); // \(._.)/
                        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.FColision, _index.ToString("00")));
                        EventRegister.Instance.EvntToJson();
                    }
                }
                else if (other.gameObject.CompareTag("DropTrigger"))
                {
                    if (dragComponent.AmIBeingDragged() && f.TryScore(dragComponent.GetIndex()))
                    {
                        FloatieDrop(other.transform.parent.gameObject);
                        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.FColision, _index.ToString("00")));
                        EventRegister.Instance.EvntToJson();
                    }
                }
                else if (other.gameObject.CompareTag("BodyTrigger"))
                {
                    if(currentState!=DolphinStates.FLOATIEJUMPING && !dragComponent.AmIBeingDragged())
                    {
                        currentState = DolphinStates.FLOATIEJUMPING; //Esto es mentira pero bueno
                        animator.SetTrigger("QuickDive");
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Floatie f = other.gameObject.GetComponent<Floatie>();
        if (f != null && scoringFloatie)
        {
            scoringFloatie = false;
            OnHitFloatie();
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

            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DSaltoInit, _index.ToString("00")));
            EventRegister.Instance.EvntToJson();
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
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DPiruetaInit, _index.ToString("00")));
            EventRegister.Instance.EvntToJson();
            return true;
        }
        return false;
    }

    public void Dive()
    {
        isAboutToDive = true;
        buceoComponent.enabled = true;
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DSaleSuperficie, _index.ToString("00")));
        EventRegister.Instance.EvntToJson();
        dragComponent.DeactivateDrag();
        dragComponent.enabled = false;
        buceoComponent.SetPath(float3.zero);
        ClearMatrixOccupation();
        currentState = DolphinStates.DIVING;
    }

    public void FloatieTrick()
    {
        scoringFloatie = true;
        currentState = DolphinStates.FLOATIEJUMPING;
        animator.SetTrigger("FloatieJump");
    }
    public void FloatieDrop(GameObject floatie)
    {
        bool success = GetComponent<Drop>().DropForceOnOBj(floatie);
        scoringFloatie = false;
        if (success)
        {
            scoringFloatie = true;
            currentState = DolphinStates.FLOATIEJUMPING;
            animator.SetTrigger("FloatieDive");
            Invoke("OnHitFloatie", 0.5f);
            dragComponent.DeactivateDrag();
            dragComponent.enabled = false;
        }
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
            dragComponent.enabled = true;
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DEntraSuperficie, _index.ToString("00")));
            EventRegister.Instance.EvntToJson();
            StartCoroutine("PauseDamage");
            return true;
        }
        else return false;
    }
    public void OnAnimationEnded(string action) //funci�n que se llama en evento de fin de animaci�n 
    {
        switch (action)
        {
            case "Jump":
                EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DSaltoFin, _index.ToString("00")));
                EventRegister.Instance.EvntToJson();
                break;
            case "Roll":
                //hasBeenHit = false;
                EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DPiruetaFin, _index.ToString("00")));
                EventRegister.Instance.EvntToJson();
                break;
            case "FloatieJump":
                break;
            case "FloatieDive":
                //este es el que ha arrastrado manualmente al flotador, el otro puede que lo haya colocado o que se de la casualidad
                dragComponent.enabled = true;
                break;
        }
        if (isAboutToDive) { currentState = DolphinStates.DIVING; }
        else
        {
            currentState = DolphinStates.FLOATING;
        }
    }


    public void TryClickDolphin()
    {

        if (currentState == DolphinStates.SPECIALJUMPING) //RIGHT GUESS SPECIAL JUMP
        {
            // Dolphin sound
            _myAudioSource.Play();

            //In world points text
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.RespuestaCorrecta, _index.ToString("00")));
            int plusPoints = dolphinMngr.RightGuess();
            showPointsOnDolphin(plusPoints, Color.green);
        }
        else if (currentState == DolphinStates.JUMPING /*&& !dragComponent.AmIBeingDragged()*/) //WRONG GUESS SPECIAL JUMP
        {
            //In world points text
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.RespuestaIncorrecta, _index.ToString("00")));
            int lessPoints = dolphinMngr.WrongGuess();
            showPointsOnDolphin(lessPoints, Color.red);

            // Desactiva Velocidad aumentada
            DolphinLevelManager.Instance.DeactivateIncreasedSpeed();
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

    public void SetIndex(int index)
    {
        _index = index;
    }
}

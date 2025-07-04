using TMPro;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using System;

public class DolphinController : MonoBehaviour
{
    //ID delfín
    int _index;

    // Componentes 
    private Animator animator;
    private Buceo buceoComponent;
    private DolphinManager dolphinMngr;
    private Drag dragComponent; // Tiene todas las gestiones de input (tanto arrastre como clicks en saltos)
    private AudioSource _myAudioSource;

    // Estado 
    public enum DolphinStates { FLOATING, DIVING, JUMPING, SPECIALJUMPING, FLOATIEJUMPING};
    public DolphinStates currentState;
    public DolphinStates startingState;
    private bool isAboutToDive; // True si acaba de recibir orden de Dive() (para evitar problemas si hay animaciones inacabadas)
    private bool scoringFloatie; // True mientras se lleva a cabo la acción de saltar o sumergirse en un flotador

    // Daño 
    [SerializeField]
    protected bool canBeDamaged;
    [SerializeField]
    protected float invincibilityTime = 2.0f;

    // Texto puntos encima del delfín
    [SerializeField]
    GameObject _pointsTextPrefab;
    [SerializeField]
    float _pointsTextLifeTime;

    // Collider click salto (para visualización del collider en debug)
    [SerializeField, Tooltip("�rea que detecta click delf�n")]
    GameObject _colliderClickDolphin;
    float riverFloatingHeight;

    void Start()
    {
        animator = GetComponent<Animator>();
        buceoComponent = GetComponent<Buceo>();
        dragComponent = GetComponent<Drag>();
        _myAudioSource = GetComponent<AudioSource>();
        currentState = DolphinStates.FLOATING; 
        isAboutToDive = false;
        canBeDamaged = true;
        scoringFloatie = false;

        transform.Rotate(new Vector3(0, 90, 0));
        riverFloatingHeight = DolphinLevelManager.Instance.GetRiverFloatingHeight();

        //Si su estado inicial es diving se va
        if (startingState == DolphinStates.DIVING)
        {
            Dive();
        }
    }

    public void SetIndex(int index)
    {
        _index = index;
    }
    public DolphinStates getDolphinState()
    {
        return currentState;
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
        //Para visualizar el collider de click de salto cuando está activo en escena
        Gizmos.color = Color.red;
        if (_colliderClickDolphin.activeSelf)
        {
            BoxCollider boxCollider = _colliderClickDolphin.GetComponent<BoxCollider>();
            Gizmos.DrawWireCube(boxCollider.bounds.center, boxCollider.bounds.size);
        }
    }

    // Colisión con obstáculos
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponent<Obstaculo>())
        {            
            OnHitObstacle(collision.gameObject.GetComponent<Obstaculo>().GetType());
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.OColision, _index.ToString("00")));
            EventRegister.Instance.EvntToJson();
        }
    }

    /// <summary>
    /// Resta de puntos por colisión con obstáculo y sumersión delfín.
    /// </summary>
    protected void OnHitObstacle(Box type)
    {
        if (canBeDamaged)
        {
            StartCoroutine("PauseDamage");
            Dive();
            int points = dolphinMngr.HitObstacle(type);
            ShowPointsOnDolphin(points, Color.red);
        }
    }

    /// <summary>
    /// Pausa momentánea de la capacidad de recibir daño del delfín.
    /// </summary>
    /// <returns></returns>
    IEnumerator PauseDamage()
    {
        canBeDamaged = false;
        yield return new WaitForSeconds(invincibilityTime);
        canBeDamaged = true;
    }

    // Interacción con flotadores
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("BodyTrigger")) //Cualquier colisión con el flotador (aunque no haya score)
        {
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.FColision, _index.ToString("00")));
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
                        FloatieTrick(); 
                    }
                }
                else if (other.gameObject.CompareTag("DropTrigger"))
                {
                    if (dragComponent.AmIBeingDragged() && f.TryScore(dragComponent.GetIndex()))
                    {
                        FloatieDrop(other.transform.parent.gameObject);
                    }
                }
                else if (other.gameObject.CompareTag("BodyTrigger"))
                {
                    if(currentState!=DolphinStates.FLOATIEJUMPING&&!dragComponent.AmIBeingDragged())
                    {
                       FloatieAvoid();
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

    /// <summary>
    /// Suma de puntos por colisión con flotador.
    /// </summary>
    private void OnHitFloatie()
    {
        int points = dolphinMngr.FloatHit();
        ShowPointsOnDolphin(points, Color.green);
    }

    /// <summary>
    /// Salto automático hacia el flotador (triggereado por collider adelantado del flotador)
    /// </summary>
    private void FloatieTrick()
    {
        scoringFloatie = true;
        currentState = DolphinStates.FLOATIEJUMPING;
        animator.SetTrigger("FloatieJump");
    }

    /// <summary>
    /// Sumersión del delfín al ser arrastrado por el jugador
    /// </summary>
    /// <param name="floatie"> El flotador a atravesar</param>
    private void FloatieDrop(GameObject floatie)
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

    /// <summary>
    /// Sumersión del delfín automática para evitar colisionar con flotador.
    /// </summary>
    private void FloatieAvoid()
    {
        currentState = DolphinStates.FLOATIEJUMPING;
        animator.SetTrigger("QuickDive");
    }

    /// <summary>
    /// Salto normal del delfín.
    /// </summary>
    /// <returns>True on success</returns>
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

    /// <summary>
    /// Salto especial del delfín.
    /// </summary>
    /// <returns>True on success</returns>
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

    /// <summary>
    /// Orden de buceo del delfín.
    /// </summary>
    public void Dive()
    {
        // Deshabilite presencia en superfície
        dragComponent.enabled = false;
        dragComponent.DeactivateDrag();
        ClearMatrixOccupation();

        // Comienzo del buceo
        isAboutToDive = true;
        buceoComponent.enabled = true;
        currentState = DolphinStates.DIVING;
        dolphinMngr.AddDivingDolphin(gameObject);
        buceoComponent.SetPath(float3.zero);
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DSaleSuperficie, _index.ToString("00")));
        EventRegister.Instance.EvntToJson();
    }

    /// <summary>
    /// Vacío posición actual del delfín en matriz de objetos y propia (ej: si posición actual está por debajo del agua).
    /// </summary>
    public void ClearMatrixOccupation()
    {
        // Vaciamos lugar en matriz del río
        Vector2 dolphinMatrixPos = GetComponent<MatrixCubeInfo>().GetXY();
        if (dolphinMatrixPos.x != -1)
        {
            DolphinLevelManager.Instance.SetOccupation((int)dolphinMatrixPos.x, (int)dolphinMatrixPos.y, Box.Empty);
        }
        // Reseteo mi posición
        GetComponent<MatrixCubeInfo>().SetXY(-1, -1);
    }

    /// <summary>
    /// Retorno del delfín a la superficie del río.
    /// </summary>
    /// <returns>True on success</returns>
    public bool Float() 
    {
        if (currentState == DolphinStates.DIVING)
        {
            // Cogemos posición del río disponible
            Vector2 matrixPos = DolphinLevelManager.Instance.GetNextAvailableMatrixSpot(this.transform.position);
            float3 pos = (float3)DolphinLevelManager.Instance.GetWorldPositionFromCube((int)matrixPos.x, (int)matrixPos.y);
            GetComponent<Drop>().SetInitialPosition(pos);
            pos = new float3(pos.x, riverFloatingHeight, pos.z);

            // Delfín se dirige a ella en su útlima vuelta buceando
            buceoComponent.SetPath(pos);

            // Guardo posición en matriz y establezco nuevo estado 
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

    /// <summary>
    /// Función que se llama desde las animaciones de acciones del delfín al estar terminándose.
    /// </summary>
    /// <param name="action">Acción animada terminada</param>
    public void OnAnimationEnded(string action) 
    {
        switch (action)
        {
            case "Jump":
                EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DSaltoFin, _index.ToString("00")));
                EventRegister.Instance.EvntToJson();
                break;
            case "Roll": // Salto especial 
                EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.DPiruetaFin, _index.ToString("00")));
                EventRegister.Instance.EvntToJson();
                break;
            case "FloatieJump": // Salto automático 
                break;
            case "FloatieDive": // Arrastre a flotador
                dragComponent.enabled = true;
                break;
        }
        if (isAboutToDive) { currentState = DolphinStates.DIVING; }
        else
        {
            currentState = DolphinStates.FLOATING; // Vuelta al estado base
        }
    }

    /// <summary>
    /// Tras click sobre salto o pirueta del delfín (acierto/fallo)
    /// </summary>
    public void TryClickDolphin()
    {
        if (currentState == DolphinStates.SPECIALJUMPING) //RIGHT GUESS SPECIAL JUMP
        {
            // Dolphin sound
            _myAudioSource.Play();

            //In world points text
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.RespuestaCorrecta, _index.ToString("00")));
            int plusPoints = dolphinMngr.RightGuess();
            ShowPointsOnDolphin(plusPoints, Color.green);
        }
        else if (currentState == DolphinStates.JUMPING /*&& !dragComponent.AmIBeingDragged()*/) //WRONG GUESS SPECIAL JUMP
        {
            //In world points text
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.RespuestaIncorrecta, _index.ToString("00")));
            int lessPoints = dolphinMngr.WrongGuess();
            ShowPointsOnDolphin(lessPoints, Color.red);

            // Desactiva Velocidad aumentada
            DolphinLevelManager.Instance.DeactivateIncreasedSpeed();
        }
    }

    void ShowPointsOnDolphin(int points, Color col)
    {
        if (points != 0)
        {
            //Texto con puntos adquiridos instanciado encima del delfín
            Vector3 offsetHeight = new Vector3(0.0f, 2.0f, 0.0f);
            GameObject pointsTetx = Instantiate(_pointsTextPrefab, transform.position + offsetHeight, Quaternion.identity);
            pointsTetx.GetComponentInChildren<TextMeshProUGUI>().SetText(points.ToString());
            pointsTetx.GetComponentInChildren<TextMeshProUGUI>().color = col;
            Destroy(pointsTetx, _pointsTextLifeTime);
        }
    }
}

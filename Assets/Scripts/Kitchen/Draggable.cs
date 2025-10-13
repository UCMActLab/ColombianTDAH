using System;
using UnityEngine;
using UnityEngine.Events;

public class Draggable : MonoBehaviour
{
    #region references
    private Camera mainCamera;
    private LayerMask layerMask;
    private ProcessableIngredient p;
    #endregion

    #region properties
    [HideInInspector]
    public bool isDragging { get; private set; }
    [SerializeField] private bool draggableObject = true;
    [SerializeField] private float dragStartDelay = 0.02f;
    #endregion

    #region parameters
    [SerializeField]
    private float dragDistance = 4.5f; // Distancia a la que agarramos objetos desde la cámara
    private static bool anyDragging = false;
    private bool pressedOnThis = false;
    private float pressTime = 0f;
    private bool clickablePressActive = false;

    [Header("Events")]
    public UnityEvent onStartDragging;
    public UnityEvent onStopDragging;

    #endregion

    #region methods
    void Start()
    {
        mainCamera = Camera.main;
        isDragging = false;
        layerMask = LayerMask.GetMask("Click");
        Input.simulateMouseWithTouches = true;
        p = GetComponent<ProcessableIngredient>();
    }

    void Update()
    {
        if (!enabled)
        {
            if (isDragging) StopDrag();
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            if (!DraggableBlocker.IsAllowed(gameObject)) return;

            if (RayHitsMe(Input.mousePosition))
            {
                pressedOnThis = true;
                pressTime = Time.time;

                // Caso no arrastrable
                if (!draggableObject)
                {
                    clickablePressActive = true;
                    onStartDragging?.Invoke();   
                }
            }
            else
            {
                pressedOnThis = false;
            }
        }


        // Hold
        if (Input.GetMouseButton(0))
        {
            if (draggableObject)
            {
                // arranque del drag (tras delay)
                if (!isDragging && pressedOnThis && !anyDragging)
                {
                    if (Time.time - pressTime >= dragStartDelay)
                        StartDrag();
                }

                // mover mientras arrastras
                if (isDragging)
                {
                    if (mainCamera == null) mainCamera = Camera.main;
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    Vector3 targetPosition = ray.GetPoint(dragDistance);
                    transform.position = targetPosition;
                }
            }
        }

        // Up
        if (Input.GetMouseButtonUp(0))
        {
            if (isDragging)
                StopDrag();

            if (clickablePressActive)
            {
                onStopDragging?.Invoke();
                clickablePressActive = false;
            }

            pressedOnThis = false;  
        }
    }

    private void StartDrag()
    {
        if (anyDragging) return;
        if (!DraggableBlocker.IsAllowed(gameObject)) return;
        anyDragging = true;
        isDragging = true;
        onStartDragging?.Invoke();
        if(p != null)
        {
            string s = p.ingredientType.ToString();
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.KitchenSeleccionarIngrediente, s));
            EventRegister.Instance.EvntToJson();
            Debug.Log("Agarramos: " + s);
        }
        
        KitchenSoundManager.Instance.PlayOneShotRaw(ObjetosSound.HandGrab, "Grab");  
    }

    private void StopDrag()
    {
        isDragging = false;
        anyDragging = false;
        onStopDragging?.Invoke();
        KitchenSoundManager.Instance.PlayOneShotRaw(ObjetosSound.HandDrop, "Drop");
    }

    private bool RayHitsMe(Vector2 screenPos)
    {
        if (mainCamera == null) mainCamera = Camera.main;
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, layerMask))
        {
            return hit.collider != null && hit.collider.gameObject == gameObject;
        }
        return false;
    }

    public bool IsGrabbable => draggableObject;
    public float DragDistance => dragDistance;
    #endregion
}

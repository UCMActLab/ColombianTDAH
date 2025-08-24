using UnityEngine;
using UnityEngine.Events;

public class Draggable : MonoBehaviour
{
    #region references
    private Camera mainCamera;
    #endregion

    #region properties
    [HideInInspector]
    public bool isDragging { get; private set; }
    [SerializeField] private bool draggableObject = true;
    #endregion

    #region parameters
    [SerializeField]
    private float dragDistance = 4.5f; // Puedes ajustar la distancia predeterminada

    [Header("Events")]
    public UnityEvent onStartDragging;
    public UnityEvent onStopDragging;
    #endregion

    #region methods
    void Start()
    {
        mainCamera = Camera.main;
        isDragging = false;
    }

    void Update()
    {
        if (isDragging && draggableObject)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 targetPosition = ray.GetPoint(dragDistance);
            transform.position = targetPosition;
        }
    }

    void OnMouseDown()
    {
        if (enabled)
        {
            isDragging = true;
            onStartDragging?.Invoke();
        }
        
    }

    void OnMouseUp()
    {
        if (enabled)
        {
            isDragging = false;
            onStopDragging?.Invoke();
        }
        
    }

    #endregion
}

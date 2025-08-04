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
    #endregion

    #region parameters
    [SerializeField]
    private float dragDistance = 5f; // Puedes ajustar la distancia predeterminada

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
        if (isDragging)
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            Vector3 targetPosition = ray.GetPoint(dragDistance);
            transform.position = targetPosition;
        }
    }

    void OnMouseDown()
    {
        isDragging = true;
        onStartDragging?.Invoke();
    }

    void OnMouseUp()
    {
        isDragging = false;
        onStopDragging?.Invoke();
    }
    #endregion
}

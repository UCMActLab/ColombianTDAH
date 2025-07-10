using UnityEngine;

public class Draggable : MonoBehaviour
{
    #region references
    private Camera mainCamera;
    #endregion

    #region properties
    [HideInInspector]
    public bool isDragging { get; private set; } // Booleano para saber si el objeto está siendo arrastrado
    #endregion

    #region parameters
    [SerializeField]
    private float dragDistance; // Distancia constante desde la cámara
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
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    #endregion
}
using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DropZoneFeedback : MonoBehaviour
{
    #region references
    private Renderer rend;
    #endregion

    #region properties
    private Color originalColor; // Color original del objeto
    #endregion

    #region parameters
    [SerializeField]
    private Color readyToDropColor; // Color cuando el draggable está dentro del objeto
    #endregion

    #region methods
    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
    }

    void OnTriggerEnter(Collider other)
    {
        var draggable = other.GetComponent<Draggable>();
        if (draggable != null && draggable.isDragging)
        {
            rend.material.color = readyToDropColor;
        }
    }

    void OnTriggerExit(Collider other)
    {
        var draggable = other.GetComponent<Draggable>();
        if (draggable != null)
        {
            rend.material.color = originalColor;
        }
    }
    #endregion
}

using UnityEngine;

[RequireComponent(typeof(Renderer))]
public class DraggableFeedback : MonoBehaviour
{
    #region references
    private Renderer rend;
    private Draggable draggable;
    #endregion

    #region properties
    private Color originalColor; // Color original del objeto
    #endregion

    #region parameters
    [SerializeField]
    private Color draggingColor; // Color mientras se arrastra
    #endregion

    #region methods
    void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;
        draggable = GetComponent<Draggable>();
    }

    void Update()
    {
        if (draggable != null)
        {
            if (draggable.isDragging)
            {
                rend.material.color = draggingColor;
            }
            else
            {
                rend.material.color = originalColor;
            }
        }
    }
    #endregion
}

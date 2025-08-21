using UnityEngine;

[RequireComponent(typeof(Draggable))]
public class ConveyorSnapOnDrop : MonoBehaviour
{
    #region references
    private Draggable draggable;
    private ConveyorDetector detector; // Debe estar en este mismo GO (igual que con las workstations)
    #endregion

    #region state
    private bool wasDragging;
    #endregion

    #region mehods
    void Start()
    {
        draggable = GetComponent<Draggable>();
        detector = GetComponent<ConveyorDetector>();
    }

    void Update()
    {
        // Solo actuamos cuando soltamos el objeto
        if (wasDragging && !draggable.isDragging)
        {
            HandleDrop();
        }
        wasDragging = draggable.isDragging;
    }

    private void HandleDrop()
    {
        if (detector == null)
        {
            return;
        }

        // 1) No hay cinta bajo el cursor
        ConveyorBelt conv;
        if (!detector.TryGetCurrentConveyor(out conv) || conv == null)
        {
            detector.ForceClearOverlay();
            return;
        }
        Debug.Log("Convoyer: "+ conv.name);
        // 2) ¿La cinta acepta este objeto? (por defecto: requiere CompletedDish)
        if (!conv.CanBoard(gameObject))
        {
            detector.ForceClearOverlay();      
            return;
        }

        // 3) Montamos en la cinta
        var entry = conv.GetEntryPoint();
        if (entry != null)
        {
            transform.position = entry.position;
            transform.rotation = entry.rotation;
        }

        // Desactivamos overlay y entregamos a la cinta
        conv.Board(gameObject);
        detector.ForceClearOverlay();       
    }
    #endregion
}

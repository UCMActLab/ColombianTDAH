using UnityEngine;

public class ConveyorSnapOnDrop : MonoBehaviour
{
    #region references
    private Draggable draggable;
    private ConveyorDetector raycaster;
    private CompletedRecipe completedRecipe;
    #endregion

    #region properties
    private bool wasDragging; // Flag para detectar el soltado
    #endregion

    #region methods
    void Start()
    {
        draggable = GetComponent<Draggable>();
        raycaster = GetComponent<ConveyorDetector>();
        completedRecipe = GetComponent<CompletedRecipe>();

        if (draggable == null) Debug.LogError("[ConveyorSnapOnDrop] Falta Draggable.");
        if (raycaster == null) Debug.LogError("[ConveyorSnapOnDrop] Falta ConveyorDetector.");
        if (completedRecipe == null) Debug.LogError("[ConveyorSnapOnDrop] Falta CompletedRecipe (marca de plato final).");
    }

    void Update()
    {
        if (wasDragging && !draggable.isDragging)
        {
            HandleDrop();
        }
        wasDragging = draggable.isDragging;
    }

    private void HandleDrop()
    {
        if (raycaster == null || completedRecipe == null)
        {
            raycaster?.ForceClearOverlay();
            return;
        }
        var returner = GetComponent<ReturnToSpawn>();
        if (returner) returner.MarkDropHandledThisFrame();

        var conveyor = raycaster.GetCurrentConveyor();
        if (conveyor == null)
        {
            // No está sobre cinta
            raycaster.ForceClearOverlay();
            var ret = GetComponent<IngredientSpawn>();
            if (ret) ret.ReturnToSpawn();
            return;
        }

        // Solo platos completos
        if (!conveyor.CanBoard(gameObject))
        {
            raycaster.ForceClearOverlay();
            var ret = GetComponent<IngredientSpawn>();
            if (ret) ret.ReturnToSpawn();
            return;
        }

        // Snap a la entrada de la cinta
        if (conveyor.GetEntryPoint() != null)
        {
            transform.position = conveyor.GetEntryPoint().position;
            transform.rotation = conveyor.GetEntryPoint().rotation;
        }
        
        // Montamos en la cinta
        conveyor.Board(gameObject);

        // Limpiamos overlay
        raycaster.ForceClearOverlay();
    }
    #endregion
}

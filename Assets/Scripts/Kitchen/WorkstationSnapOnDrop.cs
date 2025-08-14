using UnityEngine;

public class WorkstationSnapOnDrop : MonoBehaviour
{
    #region references
    private Draggable draggable;
    private WorkstationDetector raycaster;
    private ProcessableIngredient processable;
    #endregion

    #region properties
    private bool wasDragging; // Flag para detectar el soltado
    #endregion

    #region methods
    void Start()
    {
        draggable = GetComponent<Draggable>();
        raycaster = GetComponent<WorkstationDetector>();
        processable = GetComponent<ProcessableIngredient>();
        processable = GetComponent<ProcessableIngredient>();

        if (draggable == null) Debug.LogError("[WorkstationSnapOnDrop] Falta Draggable.");
        if (raycaster == null) Debug.LogError("[WorkstationSnapOnDrop] Falta WorkstationDetector.");
        if (processable == null) Debug.LogError("[WorkstationSnapOnDrop] Falta ProcessableIngredient.");
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
        // 1) Obtenemos la estación detectada en el último frame de drag
        if (raycaster == null || processable == null)
        {
            raycaster?.ForceClearOverlay();
            return;
        }

        WorkstationProcessor processor;
        if (!raycaster.TryGetCurrentProcessor(out processor) || processor == null)
        {
            raycaster.ForceClearOverlay();
            return;
        }

        // 2) Comprobamos si se puede procesar aquí (regla: intermedias siempre; finales solo si fueron seleccionadas)
        bool canProcess = raycaster.CanProcessHere(processable.ingredientType, processor.workstationType);
        if (!canProcess)
        {
            raycaster.ForceClearOverlay();
            return;
        }

        // 3) Movemos al spawnPoint y lanzamos el procesamiento inmediato
        if (processor.spawnPoint != null)
        {
            transform.position = processor.spawnPoint.position;
            transform.rotation = processor.spawnPoint.rotation;
        }

        // Lanzamos el flujo de procesado de la estación
        processor.OnItemPlaced(processable);

        // 4) Limpiamos overlay/target después de usarlo
        raycaster.ForceClearOverlay();
    }
    #endregion
}

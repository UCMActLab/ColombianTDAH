using UnityEngine;

public class WorkstationSnapOnDrop : MonoBehaviour
{
    #region references
    private Draggable draggable;
    private WorkstationDetector raycaster;
    private ProcessableIngredient processableIngredient;
    #endregion

    #region methods
    void Start()
    {
        draggable = GetComponent<Draggable>();
        raycaster = GetComponent<WorkstationDetector>();
        processableIngredient = GetComponent<ProcessableIngredient>();
    }

    void Update()
    {
        // Solo actuamos cuando soltamos el objeto
        if (!draggable.isDragging && raycaster.GetCurrentWorkstation() != null)
        {
            Transform station = raycaster.GetCurrentWorkstation();
            var processor = station.GetComponent<WorkstationProcessor>();

            if (processor != null)
            {
                // Comprobamos si se puede procesar en esta workstation
                bool canProcess = raycaster.CanProcessHere(processableIngredient.ingredientType, processor.workstationType);

                if (canProcess && processor.spawnPoint != null)
                {
                    // Movemos ingrediente al spawn point
                    transform.position = processor.spawnPoint.position;
                    transform.rotation = processor.spawnPoint.rotation;

                    processor.StartProcessing(gameObject); // Comenzamos el proceso
                }
            }
        }
    }
    #endregion
}

using UnityEditor;
using UnityEngine;

public class SpongeCleaner : MonoBehaviour
{
    [SerializeField] private LayerMask workstationLayer;   // Capa de estaciones
    [SerializeField] private float raycastDistance = 20f;  
    [SerializeField] private float cleanTime = 1.2f; // Tiempo para limpiar

    private Draggable drag;
    private Camera cam;

    private WorkstationProcessor currentTarget;
    private float timer;
    private bool cleaning;

    void Awake()
    {
        drag = GetComponent<Draggable>();
        cam = Camera.main;
    }

    void Update()
    {
        if (drag == null || cam == null) return;

        if (!drag.isDragging)
        {
            StopScrub(resetTimer: true);
            return;
        }

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, workstationLayer))
        {
            var ws = hit.transform.GetComponentInParent<WorkstationProcessor>();
            if (ws != currentTarget)
            {
                
                if (!IsStationCleanable(ws))
                {
                    StopScrub(true);
                }
                else
                {
                    StopScrub(true);
                    StartScrub(ws);
                }
            }
        }
        else
        {
            StopScrub(true);
        }

        // Avanza temporizador si estamos limpiando el mismo objetivo
        if (cleaning && currentTarget != null)
        {
            if (!IsStationCleanable(currentTarget))
            {       
                StopScrub(true);
                return;
            }
            // Debug.Log(timer);
            timer += Time.deltaTime;
            if (timer >= cleanTime)
                CompleteScrub();
        }
    }

    private void StartScrub(WorkstationProcessor ws)
    {
        currentTarget = ws;
        cleaning = true;
        timer = 0f;

        AnimatorManager.Instance.PlayAndPauseAt(ObjetosAnim.Esponja, "Clean", 2f);
    }

    private void StopScrub(bool resetTimer)
    {
        if (!cleaning) return;

        cleaning = false;
        if (resetTimer) timer = 0f;

        currentTarget = null;
        
    }

    private void CompleteScrub()
    {
        if (currentTarget != null && IsStationCleanable(currentTarget))
            currentTarget.ClearInventory();

        currentTarget = null;
        cleaning = false;
        timer = 0f;
    }

    private bool IsStationCleanable(WorkstationProcessor ws)
    {
        if (ws == null) return false;

        var inv = ws.GetComponent<WorkstationInventory>();
        if (inv == null) inv = ws.GetComponentInChildren<WorkstationInventory>(true);

        return inv != null && inv.TotalItems > 0;
    }
}

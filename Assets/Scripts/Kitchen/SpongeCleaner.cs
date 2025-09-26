using UnityEditor;
using UnityEngine;

public class SpongeCleaner : MonoBehaviour
{
    [SerializeField] private LayerMask workstationLayer; // Capa de estaciones
    [SerializeField] private float raycastDistance = 20f;  
    [SerializeField] private float cleanTime = 1.2f; // Tiempo para limpiar

    [SerializeField] private ObjetosAnim spongeAnimKey = ObjetosAnim.Esponja;
    [SerializeField] private string idleState = "Idle";
    [SerializeField] private string cleanLoopState = "Clean";

    [SerializeField] private ParticleSystem cleaningFx;

    private Draggable drag;
    private Camera cam;

    private WorkstationProcessor currentTarget;
    private float timer;
    private bool cleaning;

    private Vector3 lastHitPoint;
    private Vector3 lastHitNormal;

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
            lastHitPoint = hit.point;
            lastHitNormal = hit.normal;
            if (ws != currentTarget)
            {
                StopScrub(true);

                if (IsStationCleanable(ws))
                    StartScrub(ws);
            }
        }
        else
        {
            StopScrub(true);
        }

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

            UpdateFxAtContact();
        }
    }

    private void StartScrub(WorkstationProcessor ws)
    {
        currentTarget = ws;
        cleaning = true;
        timer = 0f;

        AnimatorManager.Instance.ChangeAnimation(spongeAnimKey, cleanLoopState, 0.1f);

        // FX
        if (cleaningFx != null)
        {
            UpdateFxAtContact();
            cleaningFx.Play();
        }
    }

    private void StopScrub(bool resetTimer)
    {
        if (!cleaning) return;

        cleaning = false;
        if (resetTimer) timer = 0f;

        AnimatorManager.Instance.ChangeAnimation(spongeAnimKey, idleState, 0.1f);

        // FX
        if (cleaningFx != null)
            cleaningFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        currentTarget = null;
        
    }

    private void CompleteScrub()
    {
        if (currentTarget != null && IsStationCleanable(currentTarget))
            currentTarget.ClearInventory();

        StopScrub(resetTimer: true);
    }

    private bool IsStationCleanable(WorkstationProcessor ws)
    {
        if (ws == null) return false;

        var inv = ws.GetComponent<WorkstationInventory>();
        if (inv == null) inv = ws.GetComponentInChildren<WorkstationInventory>(true);

        return inv != null && inv.TotalItems > 0;
    }

    private void UpdateFxAtContact()
    {
        if (cleaningFx == null) return;

        cleaningFx.transform.position = lastHitPoint + lastHitNormal * 0.01f;
        cleaningFx.transform.rotation = Quaternion.LookRotation(-lastHitNormal, Vector3.up);
    }
}

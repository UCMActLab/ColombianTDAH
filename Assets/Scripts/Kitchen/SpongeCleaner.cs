using UnityEngine;
using System.Collections;

public class SpongeCleaner : MonoBehaviour
{
    [SerializeField] private LayerMask workstationLayer; // Capa de estaciones
    [SerializeField] private float raycastDistance = 20f;  
    [SerializeField] private float cleanTime = 2.3f; // Tiempo para limpiar
    [SerializeField] private float contactOffset = 0.01f; // Separación del FX respecto a la superficie

    [SerializeField] private ObjetosAnim spongeAnimKey = ObjetosAnim.Esponja;
    [SerializeField] private string idleState = "Idle";
    [SerializeField] private string cleanLoopState = "Clean";

    [SerializeField] private ObjetosSound soundKey;
    [SerializeField] private string cleaningSfxName = "";

    [SerializeField] private ParticleSystem cleaningFx;

    private Draggable drag;
    private Camera cam;
    private ReturnToSpawn returner;

    private Coroutine cleaningRoutine;
    private Vector3 lastHitPoint;
    private Vector3 lastHitNormal;


    void Awake()
    {
        drag = GetComponent<Draggable>();
        cam = Camera.main;
        returner = GetComponent<ReturnToSpawn>();
    }
    void OnEnable()
    {
        if (drag != null)
        {
            drag.onStartDragging.AddListener(OnStartDragging);
            drag.onStopDragging.AddListener(OnStopDragging);
        }
    }

    void OnDisable()
    {
        if (drag != null)
        {
            drag.onStartDragging.RemoveListener(OnStartDragging);
            drag.onStopDragging.RemoveListener(OnStopDragging);
        }
        StopCleaningRoutine();
    }

    private void OnStartDragging()
    {
        // Si alguien la coge durante (o justo después) de un ciclo, cancelamos
        StopCleaningRoutine();
        SetIdleAnim();
        StopFx();
    }

    private void OnStopDragging()
    {
        // Al soltar, lanzamos un raycast desde el puntero para ver si hay Workstation
        if (cam == null) return;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, workstationLayer))
        {
            var ws = hit.transform.GetComponentInParent<WorkstationProcessor>();
            if (IsStationCleanable(ws))
            {
                lastHitPoint = hit.point;
                lastHitNormal = hit.normal;

                if (returner != null)
                {
                    returner.MarkDropHandledThisFrame();
                    returner.BeginTransit();
                }

                // Iniciamos ciclo de limpieza
                StartCleaningRoutine(ws);
                return;
            }
        }
    }
    private void StartCleaningRoutine(WorkstationProcessor ws)
    {
        StopCleaningRoutine();
        cleaningRoutine = StartCoroutine(CleanThenReturn(ws));
    }

    private void StopCleaningRoutine()
    {
        if (cleaningRoutine != null)
        {
            StopCoroutine(cleaningRoutine);
            cleaningRoutine = null;
        }
    }

    private IEnumerator CleanThenReturn(WorkstationProcessor ws)
    {
        // Bloqueamos interacción mientras limpia
        if (drag != null) drag.enabled = false;

        // Anim de limpiar
        AnimatorManager.Instance.ChangeAnimation(spongeAnimKey, cleanLoopState, 0.1f);

        // Sonido
        if (!string.IsNullOrEmpty(cleaningSfxName))
            //KitchenSoundManager.Instance.PlayLoopForDurationFaded(soundKey, cleaningSfxName, cleanTime);
        KitchenSoundManager.Instance.PlayLoopFaded(soundKey, cleaningSfxName, 0.2f);

        // Posicionamos FX en el punto de contacto del drop
        PlayFxAtContact();

        // Alineamos la esponja visualmente con la superficie durante la limpieza
        AlignSpongeAtContact();

        float t = 0f;
        while (t < cleanTime)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // Limpiamos inventario si sigue siendo válido y hay contenido
        if (ws != null && IsStationCleanable(ws))
            ws.ClearInventory();

        KitchenSoundManager.Instance.StopLoopFaded(soundKey, 0.2f);
        StopFx();
        SetIdleAnim();
        
        if (drag != null) drag.enabled = true;

        // Volvemos al spawn
        if (returner != null)
        {
            returner.EndTransit();   // Libera el bloqueo
            returner.Return(false);  
        }

        cleaningRoutine = null;
    }

    private bool IsStationCleanable(WorkstationProcessor ws)
    {
        if (ws == null) return false;

        var inv = ws.GetComponent<WorkstationInventory>();
        if (inv == null) inv = ws.GetComponentInChildren<WorkstationInventory>(true);

        return inv != null && inv.TotalItems > 0;
    }

    private void PlayFxAtContact()
    {
        if (cleaningFx == null) return;
        cleaningFx.transform.position = lastHitPoint + lastHitNormal * contactOffset;
        cleaningFx.transform.rotation = Quaternion.LookRotation(-lastHitNormal, Vector3.up);
        cleaningFx.Play();
    }

    private void StopFx()
    {
        if (cleaningFx != null)
            cleaningFx.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }

    private void AlignSpongeAtContact()
    {
        transform.position = lastHitPoint + lastHitNormal * contactOffset;
        transform.rotation = Quaternion.LookRotation(-lastHitNormal, Vector3.up);
    }

    private void SetIdleAnim()
    {
        AnimatorManager.Instance.ChangeAnimation(spongeAnimKey, idleState, 0.1f);
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class WorkstationDetector : MonoBehaviour
{
    #region references
    [SerializeField] 
    private LayerMask workstationLayer; // Capa para las workstations
    [SerializeField] private RecetasDatabase recetasDatabase;
    private Draggable draggable; // Referencia al script Draggable del objeto
    private Transform currentWorkstation;
    private WorkstationProcessor currentProcessor;
    private ProcessableIngredient processableIngredient;
    private List<Renderer> currentRenderers = new List<Renderer>();
    private List<Material[]> originalMaterials = new List<Material[]>();
    #endregion

    #region parameters
    [SerializeField] private float raycastDistance; // Distancia del raycast
    [Header("Feedback Overlay Materials")]
    [SerializeField] private Material overlayGreen;
    [SerializeField] private Material overlayRed;
    #endregion

    #region methods
    void Start()
    {
        draggable = GetComponent<Draggable>();
        processableIngredient = GetComponent<ProcessableIngredient>();
    }

    void Update()
    {
        if (draggable != null && draggable.isDragging)
        {
            DetectWorkstation();
        }
    }

    private void DetectWorkstation()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, workstationLayer))
        {
            Transform hitTransform = hit.transform;

            if (hitTransform != currentWorkstation)
            {
                ForceClearOverlay();
                
                currentWorkstation = hitTransform;
                currentProcessor = currentWorkstation.GetComponentInParent<WorkstationProcessor>();

                if (currentProcessor != null && processableIngredient != null)
                {
                    bool canProcess = CanProcessHere(processableIngredient.ingredientType, currentProcessor.workstationType);
                    ApplyOverlay(canProcess ? overlayGreen : overlayRed);
                }
            }
        }
        else
        {
            // Si no apuntas a estación, no hay target.
            ForceClearOverlay();
        }
    }

    private void ApplyOverlay(Material overlayMat)
    {
        currentRenderers.Clear();
        originalMaterials.Clear();

        if (currentWorkstation == null || overlayMat == null) return;

        currentRenderers.Clear();
        originalMaterials.Clear();

        // Todos los renderers de la estación (incluye hijos)
        Renderer[] renderers = currentProcessor != null
            ? currentProcessor.GetComponentsInChildren<Renderer>(true)
            : currentWorkstation.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            currentRenderers.Add(rend);

            // Guardamos array original exacto
            Material[] mats = rend.materials;
            originalMaterials.Add(mats);

            // Añadimos overlay al final
            var newMats = new Material[mats.Length + 1];
            mats.CopyTo(newMats, 0);
            newMats[mats.Length] = overlayMat;
            rend.materials = newMats;
        }
    }

    private void RemoveOverlay()
    {
        // Restauramos materiales originales a cada renderer
        for (int i = 0; i < currentRenderers.Count; i++)
        {
            if (currentRenderers[i] != null && originalMaterials.Count > i && originalMaterials[i] != null)
            {
                currentRenderers[i].materials = originalMaterials[i];
            }
        }
        currentRenderers.Clear();
        originalMaterials.Clear();
    }

    public void ForceClearOverlay()
    {
        RemoveOverlay();
        currentProcessor = null;
        currentWorkstation = null;
    }

    public Transform GetCurrentWorkstation() => currentWorkstation;

    public bool TryGetCurrentProcessor(out WorkstationProcessor processor)
    {
        processor = currentProcessor;
        return processor != null;
    }

    public bool CanProcessHere(Ingredientes ingrediente, PuestosDeTrabajo puesto)
    {
        if (recetasDatabase == null) return false;

        var seleccion = (LevelKitchenManager.Instance != null)
            ? LevelKitchenManager.Instance.GetRecetasSeleccionadasActuales()
            : Enumerable.Empty<RecetaData>();

        var receta = recetasDatabase.GetRecetaValida(ingrediente, puesto, seleccion);
        return receta != null;
    }

    void OnDisable()
    {
        ForceClearOverlay();
    }

    void OnDestroy()
    {
        ForceClearOverlay();
    }

    #endregion
}

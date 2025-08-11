using System.Collections.Generic;
using UnityEngine;

public class WorkstationDetector : MonoBehaviour
{
    #region references
    [SerializeField] 
    private LayerMask workstationLayer; // Capa para las workstations
    [SerializeField] private ProcesamientoDatabase procesamientoDatabase;
    [SerializeField] private RecetasDatabase recetasDatabase;
    private Draggable draggable; // Referencia al script Draggable del objeto
    private Transform currentWorkstation;
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
        else
        {
            RemoveOverlay();
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
                RemoveOverlay(); // Quitamos overlay anterior

                currentWorkstation = hitTransform;

                var processor = currentWorkstation.GetComponent<WorkstationProcessor>();
                if (processor != null && processableIngredient != null)
                {
                    bool canProcess = CanProcessHere(processableIngredient.ingredientType, processor.workstationType);

                    ApplyOverlay(canProcess ? overlayGreen : overlayRed);
                }
            }
        }
        else
        {
            ClearCurrentWorkstation();
        }
    }

    private void ApplyOverlay(Material overlayMat)
    {
        currentRenderers.Clear();
        originalMaterials.Clear();

        // Obtenemos todos los renderers de la workstation (incluyendo hijos)
        Renderer[] renderers = currentWorkstation.GetComponentsInChildren<Renderer>();

        foreach (Renderer rend in renderers)
        {
            currentRenderers.Add(rend);

            // Guardamos materiales originales
            originalMaterials.Add(rend.materials);

            // Creamos array nuevo con overlay añadido al final
            var mats = rend.materials;
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
            if (currentRenderers[i] != null)
            {
                currentRenderers[i].materials = originalMaterials[i];
            }
        }

        currentRenderers.Clear();
        originalMaterials.Clear();
    }

    public void ClearCurrentWorkstation()
    {
        if (currentWorkstation != null)
        {
            RemoveOverlay();
            currentWorkstation = null;
        }
    }

    public Transform GetCurrentWorkstation() => currentWorkstation;

    public bool CanProcessHere(Ingredientes ingrediente, PuestosDeTrabajo puesto)
    {
        // Caso 1: Proceso inmediato
        if (procesamientoDatabase.GetProcesamiento(ingrediente, puesto) != null)
            return true;

        // Caso 2: Proceso por receta
        if (recetasDatabase != null)
        {
            foreach (var receta in recetasDatabase.recetas)
            {
                if (receta.puestos.Contains(puesto) && receta.ingredientes.Contains(ingrediente))
                    return true;
            }
        }

        return false;
    }

    void OnDisable()
    {
        ClearCurrentWorkstation();
    }

    void OnDestroy()
    {
        ClearCurrentWorkstation();
    }

    #endregion
}

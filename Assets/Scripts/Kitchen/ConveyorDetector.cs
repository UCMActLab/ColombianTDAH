using System.Collections.Generic;
using UnityEngine;

public class ConveyorDetector : MonoBehaviour
{
    #region references
    [SerializeField] private LayerMask conveyorLayer; // Capa para la cinta
    private Draggable draggable;
    private ConveyorBelt currentConveyor;
    private Transform currentConveyorTransform;

    private CompletedRecipe completedRecipe; // Marca de que esto es un plato final

    private List<Renderer> currentRenderers = new List<Renderer>();
    private List<Material[]> originalMaterials = new List<Material[]>();
    #endregion

    #region parameters
    [SerializeField] private float raycastDistance = 14f; // Distancia de detección
    [Header("Feedback Overlay Materials")]
    [SerializeField] private Material overlayGreen;
    [SerializeField] private Material overlayRed;
    #endregion

    #region methods
    void Start()
    {
        draggable = GetComponent<Draggable>();
        completedRecipe = GetComponent<CompletedRecipe>();
    }

    void Update()
    {
        if (draggable != null && draggable.isDragging)
        {
            DetectConveyor();
        }
    }

    private void DetectConveyor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, conveyorLayer))
        {
            Transform hitTransform = hit.transform;

            if (hitTransform != currentConveyorTransform)
            {
                ForceClearOverlay();

                currentConveyorTransform = hitTransform;
                currentConveyor = currentConveyorTransform.GetComponentInParent<ConveyorBelt>();

                if (currentConveyor != null && completedRecipe != null)
                {
                    // Si tiene CompletedRecipe => verde, si no => rojo
                    ApplyOverlay(overlayGreen);
                }
                else
                {
                    ApplyOverlay(overlayRed);
                }
            }
        }
        else
        {
            ForceClearOverlay();
        }
    }

    private void ApplyOverlay(Material overlayMat)
    {
        currentRenderers.Clear();
        originalMaterials.Clear();

        if (currentConveyorTransform == null || overlayMat == null) return;

        Renderer[] renderers = currentConveyor.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in renderers)
        {
            if (rend == null) continue;

            currentRenderers.Add(rend);

            Material[] mats = rend.materials;
            originalMaterials.Add(mats);

            var newMats = new Material[mats.Length + 1];
            mats.CopyTo(newMats, 0);
            newMats[mats.Length] = overlayMat;
            rend.materials = newMats;
        }
    }

    private void RemoveOverlay()
    {
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
        currentConveyor = null;
        currentConveyorTransform = null;
    }

    public ConveyorBelt GetCurrentConveyor() => currentConveyor;

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
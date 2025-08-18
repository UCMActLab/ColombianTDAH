using UnityEngine;

public class ConveyorDetector : MonoBehaviour
{
    [SerializeField] private LayerMask conveyorLayer;
    [SerializeField] private float raycastDistance = 14f;
    [Header("Overlay")]
    [SerializeField] private Material overlayGreen;
    [SerializeField] private Material overlayRed;

    private Draggable draggable;
    private Camera cam;
    private ConveyorBelt currentConveyor;

    private Renderer[] rends;
    private Material[][] originals;


    #region methods
    void Start()
    {
        draggable = GetComponent<Draggable>();
        cam = Camera.main;
    }

    void Update()
    {
        if (draggable != null && draggable.isDragging)
            Detect();
        else
            ForceClearOverlay();
    }

    private void Detect()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, raycastDistance, conveyorLayer))
        {
            var conv = hit.transform.GetComponentInParent<ConveyorBelt>();
            if (conv != currentConveyor)
            {
                ForceClearOverlay();
                currentConveyor = conv;
                ApplyOverlay(conv, CanSendToConveyor());
            }
        }
        else
        {
            ForceClearOverlay();
        }
    }

    private bool CanSendToConveyor()
    {
        return GetComponent<CompletedRecipe>() != null;
    }

    private void ApplyOverlay(ConveyorBelt conv, bool green)
    {
        if (conv == null) return;
        rends = conv.GetComponentsInChildren<Renderer>(true);
        originals = new Material[rends.Length][];

        for (int i = 0; i < rends.Length; i++)
        {
            var mats = rends[i].materials;
            originals[i] = mats;
            var newMats = new Material[mats.Length + 1];
            mats.CopyTo(newMats, 0);
            newMats[mats.Length] = green ? overlayGreen : overlayRed;
            rends[i].materials = newMats;
        }
    }

    private void Clear()
    {
        if (rends != null && originals != null)
        {
            for (int i = 0; i < rends.Length; i++)
                if (rends[i] != null && originals[i] != null)
                    rends[i].materials = originals[i];
        }
        rends = null;
        originals = null;
    }

    public bool TryGetCurrentConveyor(out ConveyorBelt conv)
    {
        conv = currentConveyor;
        return conv != null;
    }

    public void ForceClearOverlay() => Clear();
    #endregion
}

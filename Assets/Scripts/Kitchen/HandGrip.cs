using UnityEngine;

public class HandGrip : MonoBehaviour
{
    private Draggable d; 

    [SerializeField] private Vector3 localGrabOffset = new Vector3(0, 0.1f, 0);

    void Awake()
    {
        d = GetComponent<Draggable>();
    }

    void OnEnable()
    {
        d.onStartDragging.AddListener(OnDragStart);
        d.onStopDragging.AddListener(OnDragStop);
    }

    void OnDisable()
    {
        d.onStartDragging.RemoveListener(OnDragStart);
        d.onStopDragging.RemoveListener(OnDragStop);
    }

    void OnDragStart()
    {
        if (!d.IsGrabbable) return;

        Vector3 grabPoint = transform.TransformPoint(localGrabOffset);
        LevelKitchenManager.Instance?.StartHandFollow(transform, grabPoint, d.DragDistance);
    }

    void OnDragStop()
    {
        LevelKitchenManager.Instance?.StopHandFollow();
    }
}
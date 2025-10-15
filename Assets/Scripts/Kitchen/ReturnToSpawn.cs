using UnityEngine;
using System.Collections;

public class ReturnToSpawn : MonoBehaviour
{  
    [SerializeField] private Transform spawn;             
    [SerializeField] private bool createHomeAtStart = true;
    [SerializeField] private bool copyInitialRotation = true;
   
    [SerializeField] private bool autoReturnOnDrop = true;
    [SerializeField] private float returnDuration = 0.35f;
    [SerializeField]
    private AnimationCurve ease =
        AnimationCurve.EaseInOut(0, 0, 1, 1);
    [SerializeField] private float snapDistance = 0.02f; 

    [SerializeField] private bool disableDraggable = true;

    [SerializeField] private bool blockReturnWhileTransit = true;

    public System.Action OnReturnStarted;
    public System.Action OnReturnFinished;

    private Draggable drag;
    private bool isReturning;
    private bool inTransit;
    private bool suppressNextAutoReturn; 
    private bool subscribed;

    void Awake()
    {
        drag = GetComponent<Draggable>();
        if (spawn == null && createHomeAtStart)
        {
            var h = new GameObject($"{name}Spawn").transform;
            h.position = transform.position;
            h.rotation = copyInitialRotation ? transform.rotation : Quaternion.identity;
            spawn = h;
        }
    }

    void OnEnable()
    {
        TrySubscribeToDraggable();
    }

    void OnDisable()
    {
        StopAllCoroutines();
        isReturning = false;
        inTransit = false;
        suppressNextAutoReturn = false;

        TryUnsubscribeFromDraggable();
    }

    private void TrySubscribeToDraggable()
    {
        if (drag == null || subscribed) return;
        drag.onStartDragging.AddListener(HandleStartDragging);
        drag.onStopDragging.AddListener(HandleStopDragging);
        subscribed = true;
    }

    private void TryUnsubscribeFromDraggable()
    {
        if (drag == null || !subscribed) return;
        drag.onStartDragging.RemoveListener(HandleStartDragging);
        drag.onStopDragging.RemoveListener(HandleStopDragging);
        subscribed = false;
    }

    private void HandleStartDragging()
    {
        // Cancela un retorno en curso
        if (isReturning)
        {         
            StopAllCoroutines();
            isReturning = false;
        }

        // Limpiamos el supresor por si quedó activo del drop anterior 
        suppressNextAutoReturn = false;
    }
    private void HandleStopDragging()
    {
        if (!autoReturnOnDrop) return;

        if (blockReturnWhileTransit && inTransit) return;
        // Si otro sistema (workstation/cinta) ha gestionado el drop, no retornamos
        if (suppressNextAutoReturn)
        {
            suppressNextAutoReturn = false; 
            return;
        }

        Return(false);
    }

    public void MarkDropHandledThisFrame()
    {
        suppressNextAutoReturn = true;
    }

    public void BeginTransit()
    {
        inTransit = true;
 
        if (isReturning)
        {
            StopAllCoroutines();
            isReturning = false;
        }
    }

    public void EndTransit() => inTransit = false;

    public void Return(bool snap)
    {
        if (spawn == null || isReturning) return;
        if (blockReturnWhileTransit && inTransit) return;

        StopAllCoroutines();
        StartCoroutine(ReturnRoutine(snap));
    }

    public void SetHome(Transform t) => spawn = t;
    public Transform GetHome() => spawn;

    private IEnumerator ReturnRoutine(bool snap)
    {
        isReturning = true;
        OnReturnStarted?.Invoke();

        if (disableDraggable && drag) drag.enabled = false;

        Vector3 p0 = transform.position;
        Quaternion r0 = transform.rotation;
        Vector3 p1 = spawn.position;
        Quaternion r1 = spawn.rotation;

        if (snap || returnDuration <= 0.01f || Vector3.Distance(p0, p1) <= snapDistance)
        {
            transform.SetPositionAndRotation(p1, r1);
        }
        else
        {
            float t = 0f;
            while (t < 1f)
            {
                float k = ease.Evaluate(t);
                transform.position = Vector3.LerpUnclamped(p0, p1, k);
                transform.rotation = Quaternion.SlerpUnclamped(r0, r1, k);
                t += Time.deltaTime / returnDuration;
                yield return null;
            }
            transform.SetPositionAndRotation(p1, r1);
        }

        if (disableDraggable && drag) drag.enabled = true;
       
        isReturning = false;
        OnReturnFinished?.Invoke();
    }
}

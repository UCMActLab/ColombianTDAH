using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GuideArrow : MonoBehaviour
{
    [SerializeField] private Transform from;
    [SerializeField] private Transform to;
    [SerializeField] private Transform dot; 

    private LineRenderer lr;
    private float t;

    void Awake() { lr = GetComponent<LineRenderer>(); lr.positionCount = 2; }
    void Update()
    {
        if (!from || !to) { lr.enabled = false; if (dot) dot.gameObject.SetActive(false); return; }
        lr.enabled = true;
        lr.SetPosition(0, from.position);
        lr.SetPosition(1, to.position);

        if (dot)
        {
            dot.gameObject.SetActive(true);
            t = (t + Time.deltaTime * 0.6f) % 1f;
            dot.position = Vector3.Lerp(from.position, to.position, t);
        }
    }

    public void Set(Transform a, Transform b) { from = a; to = b; t = 0f; }
    public void Hide() { from = to = null; }
}

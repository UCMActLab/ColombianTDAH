using UnityEngine;
using UnityEngine.UI;

public class BillboardFX : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Camera cam;
    [SerializeField] private Image mainImage;   // Image principal 
    [SerializeField] private Image outlineImage; // Duplicada

    [Header("Flotación/Pulso")]
    [SerializeField] private float bobAmplitude = 0.05f;
    [SerializeField] private float bobSpeed = 2f;
    [SerializeField] private float pulseAmplitude = 0.06f; 
    [SerializeField] private float pulseSpeed = 1.3f;
    [SerializeField] private bool bobActivated = true;
    [SerializeField] private bool pulseActivated = true;


    Vector3 baseLocalPos, baseLocalScale;

    void Start()
    {
        if (!cam) cam = Camera.main;
        baseLocalPos = transform.localPosition;
        baseLocalScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (!cam) cam = Camera.main;

        transform.forward = (transform.position - cam.transform.position).normalized;

        // Flotación
        if(bobActivated)
        {
            float y = Mathf.Sin(Time.time * bobSpeed) * bobAmplitude;
            transform.localPosition = baseLocalPos + new Vector3(0, y, 0);
        }

        // Pulso
        if (pulseActivated)
        {
            float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
            transform.localScale = baseLocalScale * s;
        }
       
        if (outlineImage)
        {
            var c = outlineImage.color;
            c.a = Mathf.Lerp(0.5f, 0.9f, (Mathf.Sin(Time.time * pulseSpeed) + 1f) * 0.5f);
            outlineImage.color = c;
        }
    }
}

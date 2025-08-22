using UnityEngine;

public class Billboard : MonoBehaviour
{
    public Camera targetCam;
    public float baseWorldScale = 0.0001f;   // ajusta a tu escena
    public float referenceDistance = 5.7f;   // a esta distancia el tamaño se ve “correcto”

    void Awake()
    {
        if (!targetCam) targetCam = Camera.main;
    }

    void LateUpdate()
    {
        // Escala para tamaño aparente constante
        float dist = Vector3.Distance(transform.position, targetCam.transform.position);
        float k = Mathf.Max(0.35f, dist / referenceDistance);
        transform.localScale = Vector3.one * baseWorldScale * k;
    }
}

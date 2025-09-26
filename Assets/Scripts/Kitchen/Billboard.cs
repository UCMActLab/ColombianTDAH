using UnityEngine;


public class Billboard : MonoBehaviour
{
    public Camera targetCam;
    public float baseWorldScale = 0.0001f;   // ajusta a tu escena
    public float referenceDistance = 5.7f;   // a esta distancia el tamaño se ve “correcto”

    [SerializeField] private bool billboard = false;
    [SerializeField] private bool scale = true;

    [Tooltip("Si está activo, solo rota sobre Y (evita inclinaciones en X/Z).")]
    public bool yawOnly = false;

    void Awake()
    {
        if (!targetCam) targetCam = Camera.main;
    }

    void LateUpdate()
    {
        if (!targetCam) return;

        if (billboard)
        {
            Vector3 fwd = targetCam.transform.forward;
            
            if (yawOnly)
            {
                // Proyecta al plano horizontal (evita cabeceos)
                fwd.y = 0f;
                if (fwd.sqrMagnitude < 1e-6f)
                    fwd = Vector3.forward; // fallback por si coincide verticalmente
                fwd.Normalize();
            }

            // Asegura que tenemos una dirección válida
            if (fwd.sqrMagnitude > 1e-6f)
                transform.forward = fwd;
        }

        if (scale)
        {
            // Escala para tamaño aparente constante
            float dist = Vector3.Distance(transform.position, targetCam.transform.position);
            float k = Mathf.Max(0.35f, dist / referenceDistance);
            transform.localScale = Vector3.one * baseWorldScale * k;
        }
    }
}

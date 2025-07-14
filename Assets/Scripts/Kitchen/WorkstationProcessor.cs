using UnityEngine;

public class WorkstationProcessor : MonoBehaviour
{
    #region references
    [SerializeField]
    private PuestosDeTrabajo workstationType;
    [SerializeField]
    private Transform spawnPoint;
    [SerializeField]
    private AudioClip sonido;
    [SerializeField]
    private ProcesamientoDatabase procesamientoDatabase;

    private AudioSource audioSource;
    #endregion

    #region properties
    private bool processed;
    #endregion

    #region methods
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        processed = false;
    }

    void OnTriggerStay(Collider other)
    {
        if (!processed)
        {
            if (other.TryGetComponent(out ProcessableIngredient pi) &&
                other.TryGetComponent(out Draggable drag) &&
                !drag.isDragging)
            {               
                var procesamiento = procesamientoDatabase.GetProcesamiento(pi.ingredientType, workstationType);
                if (procesamiento != null)
                {
                    processed = true;
                    StartCoroutine(Procesar(pi.gameObject, procesamiento));
                }                
            }
        }
    }

    private System.Collections.IEnumerator Procesar(GameObject ingrediente, ProcesamientoData data)
    {
        if (sonido != null) audioSource.PlayOneShot(sonido);
        yield return new WaitForSeconds(data.processTime); // Tiempo que dura el procesado

        if (data.processedIngredient != null)
        {
            Instantiate(data.processedIngredient, spawnPoint.position, spawnPoint.rotation);
        }

        var feedback = GetComponent<DropZoneFeedback>();
        if (feedback != null)
            feedback.ResetColor();

        Destroy(ingrediente);
        processed = false;
    }
    #endregion
}

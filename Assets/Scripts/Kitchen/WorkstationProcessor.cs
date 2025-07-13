using UnityEngine;
using System.Linq;

public class WorkstationProcessor : MonoBehaviour
{
    #region references
    [SerializeField]
    public PuestosDeTrabajo workstationType;
    [SerializeField]
    public Transform spawnPoint;
    [SerializeField]
    public AudioClip sonido;
    [SerializeField]
    private GameObject processedIngredient;

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
                // Aquí habría una lógica de selección de ingrediente a procesar
                processed = true;
                StartCoroutine(Procesar(pi.gameObject, null));
            }
        }
        
    }

    private System.Collections.IEnumerator Procesar(GameObject ingrediente, RecetaData receta)
    {
        if (sonido != null) audioSource.PlayOneShot(sonido);
        yield return new WaitForSeconds(2.7f); // Aquí habría que poner el tiempo de procesamiento(de momento está de ejemplo)

        if (processedIngredient != null)
        {
            Instantiate(processedIngredient, spawnPoint.position, spawnPoint.rotation);
        }

        var feedback = GetComponent<DropZoneFeedback>();
        if (feedback != null)
            feedback.ResetColor();

        Destroy(ingrediente);
    }
    #endregion
}

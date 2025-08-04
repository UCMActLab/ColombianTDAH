using UnityEngine;

public class WorkstationProcessor : MonoBehaviour
{
    #region references
    [SerializeField]
    public PuestosDeTrabajo workstationType;
    [SerializeField]
    public Transform spawnPoint;
    [SerializeField]
    private AudioClip sonido;
    [SerializeField]
    private ProcesamientoDatabase procesamientoDatabase;
    [SerializeField]
    private GameObject objAnim; // Objeto con animación
    [SerializeField] 
    private Animator animator;

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

    // Lo dejo comentado por si lo necesito más tarde
    //void OnTriggerStay(Collider other)
    //{
    //    if (!processed &&
    //        other.TryGetComponent(out ProcessableIngredient pi) &&
    //        other.TryGetComponent(out Draggable drag) &&
    //        !drag.isDragging)
    //    {
    //        processed = true;
    //        var procesamiento = procesamientoDatabase.GetProcesamiento(pi.ingredientType, workstationType);
    //        StartCoroutine(Procesar(pi.gameObject, procesamiento));
    //    }
    //}

    public void StartProcessing(GameObject ingrediente)
    {
        if (!processed && ingrediente.TryGetComponent(out ProcessableIngredient pi))
        {
            var procesamiento = procesamientoDatabase.GetProcesamiento(pi.ingredientType, workstationType);
            if (procesamiento != null)
            {
                processed = true;
                StartCoroutine(Procesar(ingrediente, procesamiento));
            }
        }
    }

    private System.Collections.IEnumerator Procesar(GameObject ingrediente, ProcesamientoData data)
    {
        objAnim.SetActive(true);

        if (animator != null) animator.SetBool("IsProcessing", true);

        if (sonido != null) {
            audioSource.clip = sonido;
            audioSource.loop = true;
            audioSource.Play();
        }
        yield return new WaitForSeconds(data.processTime); // Tiempo que dura el procesado

        if (data.processedIngredient != null)
        {
            Instantiate(data.processedIngredient, spawnPoint.position, spawnPoint.rotation);
        }

        if (animator != null) animator.SetBool("IsProcessing", false);

        audioSource.Stop();
        audioSource.loop = false;
        audioSource.clip = null;

        objAnim.SetActive(false);
        Destroy(ingrediente);
        processed = false;
    }
    #endregion
}

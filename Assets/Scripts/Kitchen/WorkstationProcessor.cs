using UnityEngine;
using System.Collections;
using System.Linq;

public class WorkstationProcessor : MonoBehaviour
{
    #region references
    [Header("Workstation")]
    [SerializeField] public PuestosDeTrabajo workstationType;
    [Header("Spawn of ingredients")]
    [SerializeField] public Transform spawnPoint;

    [Header("Animation")]
    [SerializeField] private GameObject objAnim; // Objeto visible que tiene animación
    [SerializeField] private ObjetosAnim animKey;
    [SerializeField] private string idleAnim = "Idle";
    [SerializeField] private string processingAnim = "Processing";
    [SerializeField] private float animTime;

    [Header("Sounds")]  
    [SerializeField] private ObjetosSound soundKey;
    [SerializeField] private string processingSfxName = "";

    [Header("Databases")]
    [SerializeField] private ProcesamientoDatabase procesamientoDatabase;
    [SerializeField] private RecetasDatabase recetasDatabase;

    [Header("Inventario (solo modo receta)")]
    [SerializeField] private WorkstationInventory inventory;
    [SerializeField] private int capacity = 6;
    #endregion

    #region properties
    private bool processed;
    [SerializeField] private bool immediateSingleProcess;
    #endregion

    #region methods
    void Start()
    {
        processed = false;
    }

    //public void StartProcessing(GameObject ingrediente)
    //{
    //    if (!processed && ingrediente.TryGetComponent(out ProcessableIngredient pi))
    //    {
    //        var procesamiento = procesamientoDatabase.GetProcesamiento(pi.ingredientType, workstationType);
    //        if (procesamiento != null)
    //        {
    //            processed = true;
    //            StartCoroutine(Procesar(ingrediente, procesamiento));
    //        }
    //    }
    //}

    public void OnItemPlaced(ProcessableIngredient pi)
    {
        if (processed || pi == null) return;

        if (immediateSingleProcess)
        {
            var pdata = procesamientoDatabase.GetProcesamiento(pi.ingredientType, workstationType);
            if (pdata == null) { Destroy(pi.gameObject); return; }
            StartCoroutine(ProcessImmediate(pi.gameObject, pdata));
            return;
        }

        // Modo receta
        if (!inventory.TryAdd(pi.ingredientType, capacity))
        {
            Destroy(pi.gameObject); // No cabe
            return;
        }

        var recipe = FindMatchingRecipe();
        Destroy(pi.gameObject);

        if (recipe != null)
        {
            inventory.ConsumeFor(recipe);
            StartCoroutine(ProcessRecipe(recipe));
        }
    }

    private RecetaData FindMatchingRecipe()
    {
        if (recetasDatabase == null || recetasDatabase.recetas == null) return null;
        // Filtramos solo recetas que se pueden hacer en esta estación
        var candidates = recetasDatabase.recetas.Where(r => r.puestos != null && r.puestos.Contains(workstationType));
        foreach (var r in candidates)
            if (inventory.Meets(r)) return r;
        return null;
    }

    private IEnumerator ProcessImmediate(GameObject ingredienteGO, ProcesamientoData data)
    {
        processed = true;

        // Animación
        AnimatorManager.Instance.PlayAndPauseAt(animKey, processingAnim, animTime);

        // Sonido
        if (!string.IsNullOrEmpty(processingSfxName))
            SoundManager.Instance.PlayLoop(soundKey, processingSfxName);

        yield return new WaitForSeconds(data.processTime); // Esperamos

        // Spawn ingrediente procesado
        if (data.processedIngredient)
            Instantiate(data.processedIngredient, spawnPoint.position, spawnPoint.rotation);

        // Fin anim & sonido
        AnimatorManager.Instance.PlayAndPauseAt(animKey, idleAnim, 0f);
        if (!string.IsNullOrEmpty(processingSfxName))
            SoundManager.Instance.StopLoop(soundKey);

        Destroy(ingredienteGO);
        processed = false;
    }

    private IEnumerator ProcessRecipe(RecetaData recipe)
    {
        processed = true;

        // Anim & sonido inicio
        AnimatorManager.Instance.PlayAndPauseAt(animKey, processingAnim, animTime);
        if (!string.IsNullOrEmpty(processingSfxName))
            SoundManager.Instance.PlayLoop(soundKey, processingSfxName);

        yield return new WaitForSeconds(recipe.tiempo_est_segs); // Esperamos

        // Spawn resultado de la receta
        //if (recipe.resultado)
        //    Instantiate(recipe.resultado, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("Receta Completada");

        // Fin anim & sonido
        AnimatorManager.Instance.PlayAndPauseAt(animKey, idleAnim, 0f);
        if (!string.IsNullOrEmpty(processingSfxName))
            SoundManager.Instance.StopLoop(soundKey);

        processed = false;
    }
    #endregion
}

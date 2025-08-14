using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;

public class WorkstationProcessor : MonoBehaviour
{
    #region references
    [Header("Workstation")]
    [SerializeField] public PuestosDeTrabajo workstationType;
    [SerializeField] private float workstationTime = 5f;
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

    public void OnItemPlaced(ProcessableIngredient pi)
    {
        if (processed || pi == null) return;

        // Lo que se selecciona para cada jornada
        var seleccion = LevelKitchenManager.Instance != null
            ? LevelKitchenManager.Instance.GetRecetasSeleccionadasActuales()
            : Enumerable.Empty<RecetaData>();

        if (immediateSingleProcess)
        {
            // Buscamos receta válida para (ingrediente + estación) respetando la regla:
            // intermedia => siempre; final => solo si está en 'seleccion'
            var receta = recetasDatabase.GetRecetaValida(pi.ingredientType, workstationType, seleccion);
            if (receta == null)
            {
                // No se puede procesar aquí (no seleccionada y no intermedia)
                Destroy(pi.gameObject);
                return;
            }

            StartCoroutine(ProcessImmediate(pi.gameObject, receta));
            return;
        }

        // Modo receta
        if(!inventory.TryAdd(pi.ingredientType, capacity))
        {
            Destroy(pi.gameObject); // No cabe
            return;
        }

        var recipe = FindMatchingRecipe(seleccion);
        Destroy(pi.gameObject);

        if (recipe != null)
        {
            inventory.ConsumeFor(recipe);
            StartCoroutine(ProcessRecipe(recipe));
        }
    }

    private RecetaData FindMatchingRecipe(IEnumerable<RecetaData> seleccion)
    {
        if (recetasDatabase == null || recetasDatabase.recetas == null) return null;

        var candidates = recetasDatabase.recetas
            .Where(r => r.puestos != null && r.puestos.Contains(workstationType))
            .Where(r => r.esIntermedia || seleccion.Contains(r)); 

        foreach (var r in candidates)
            if (inventory.Meets(r)) return r;

        return null;
    }

    private IEnumerator ProcessImmediate(GameObject ingredienteGO, RecetaData data)
    {
        processed = true;
        // Anim & sonido inicio      
        AnimatorManager.Instance.PlayAndPauseAt(animKey, processingAnim, animTime);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.PlayLoop(soundKey, processingSfxName);

        yield return new WaitForSeconds(workstationTime); // Esperamos

        // Spawn ingrediente procesado
        if (data.processedRecipe != null)
            Instantiate(data.processedRecipe, spawnPoint.position, spawnPoint.rotation);

        // Fin anim & sonido
        AnimatorManager.Instance.PlayAndPauseAt(animKey, idleAnim, 0f);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.StopLoop(soundKey);

        Destroy(ingredienteGO);
        processed = false;
    }

    private IEnumerator ProcessRecipe(RecetaData data)
    {
        processed = true;

        // Anim & sonido inicio
        if (GetComponent<Animator>() != null)
            AnimatorManager.Instance.PlayAndPauseAt(animKey, processingAnim, animTime);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.PlayLoop(soundKey, processingSfxName);

        yield return new WaitForSeconds(workstationTime); // Esperamos

        // Spawn resultado de la receta
        if (data.processedRecipe)
            Instantiate(data.processedRecipe, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("Receta Completada");

        // Fin anim & sonido
        if (GetComponent<Animator>() != null)
            AnimatorManager.Instance.PlayAndPauseAt(animKey, idleAnim, 0f);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.StopLoop(soundKey);

        processed = false;
    }
    #endregion
}

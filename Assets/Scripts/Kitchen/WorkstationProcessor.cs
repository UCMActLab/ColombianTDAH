using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine.Experimental.GlobalIllumination;

public class WorkstationProcessor : MonoBehaviour
{
    #region references
    [Header("Workstation")]
    [SerializeField] public PuestosDeTrabajo workstationType;
    [SerializeField] private float workstationTime = 5f;
    [SerializeField] private FadeLight myLight;

    [Header("Spawn of ingredients")]
    [SerializeField] public Transform spawnPoint;

    [Header("Animation")]
    [SerializeField] private ObjetosAnim animKey;
    [SerializeField] private string idleAnim = "Idle";
    [SerializeField] private string processingAnim = "Processing";
    [SerializeField] private float animTime;
    [SerializeField] private ParticleSystem particles;

    [Header("Sounds")]  
    [SerializeField] private ObjetosSound soundKey;
    [SerializeField] private string processingSfxName = "";

    [Header("Databases")]
    [SerializeField] private RecetasDatabase recetasDatabase;

    [SerializeField] private ProgressBar progressBar;

    [Header("Inventario (solo modo receta)")]
    [SerializeField] private WorkstationInventory inventory;
    [SerializeField] private int capacity = 6;
    #endregion

    #region properties
    private bool processed;
    [SerializeField] private bool immediateSingleProcess;
    #endregion

    public static System.Action<Ingredientes, GameObject> OnIngredientSpawnedGlobal;
    public static event System.Action<Ingredientes, PuestosDeTrabajo> OnItemPlacedGlobal;
    public static System.Action<RecetaData, PuestosDeTrabajo> OnRecipeCraftedGlobal;

    #region methods
    void Start()
    {
        processed = false;
    }

    public void OnItemPlaced(ProcessableIngredient pi)
    {
        if (processed || pi == null) return;
        OnItemPlacedGlobal?.Invoke(pi.ingredientType, workstationType);

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
                var retInv = pi.GetComponent<IngredientSpawn>();
                if (retInv) retInv.ReturnToSpawn();
                return;
            }

            StartCoroutine(ProcessImmediate(pi.gameObject, receta));
            return;
        }

        // Modo receta
        if(!inventory.TryAdd(pi.ingredientType, capacity))
        {
            var retFull = pi.GetComponent<IngredientSpawn>();
            if (retFull) retFull.ReturnToSpawn();
            return;
        }

        var recipe = FindMatchingRecipe(seleccion);

        var ret = pi.GetComponent<IngredientSpawn>();
        if (ret != null) ret.ConsumeAndScheduleRespawn(); 
        else pi.gameObject.SetActive(false);

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

    public void ClearInventory()
    {
        if (inventory != null)
        {
            inventory.ClearAll();
        }
    }
    private IEnumerator ProcessImmediate(GameObject ingredienteGO, RecetaData data)
    {
        Debug.Log("Procesando receta intermedia");

        processed = true;
        // Anim & sonido inicio      
        AnimatorManager.Instance.ChangeAnimation(animKey, processingAnim);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.PlayLoop(soundKey, processingSfxName);


        if (gameObject.GetComponent<MixerRotation>() != null)
            gameObject.GetComponent<MixerRotation>().Play(300);

        if (particles != null) particles.Play();

        // Iniciamos la luz (para el horno pero podria aplicarse para todos los puestos si se necesita en alguno)
        if (myLight != null)
            myLight.FadeIn();

        progressBar.HandleStart(workstationTime);

        yield return new WaitForSeconds(workstationTime); // Esperamos

        progressBar.HandleEnd();

        // Iniciamos la luz (para el horno pero podria aplicarse para todos los puestos si se necesita en alguno)
        if (myLight != null)
            myLight.FadeOut();

        if (gameObject.GetComponent<MixerRotation>() != null)
            gameObject.GetComponent<MixerRotation>().Stop();

        if (particles != null) particles.Stop();

        // Spawn ingrediente procesado
        if (data.processedRecipe != null)
        {
            var go = Instantiate(data.processedRecipe, spawnPoint.position, spawnPoint.rotation);
            var pi = go.GetComponent<ProcessableIngredient>();
            if (pi != null) OnIngredientSpawnedGlobal?.Invoke(pi.ingredientType, go);
            OnRecipeCraftedGlobal?.Invoke(data, workstationType);
            Debug.Log("Ingrediente Procesado");
        }


        // Fin anim & sonido
        AnimatorManager.Instance.ChangeAnimation(animKey, idleAnim);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.StopLoop(soundKey);

        var ret = ingredienteGO.GetComponent<IngredientSpawn>();
        if (ret != null) ret.ConsumeAndScheduleRespawn(); 
        else ingredienteGO.SetActive(false); // fallback
        //Destroy(ingredienteGO);
        processed = false;
    }

    private IEnumerator ProcessRecipe(RecetaData data)
    {
        Debug.Log("Procesando receta");

        processed = true;

        // Anim & sonido inicio
        if (GetComponent<Animator>() != null)
            AnimatorManager.Instance.ChangeAnimation(animKey, processingAnim);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.PlayLoop(soundKey, processingSfxName);

        if (gameObject.GetComponent<MixerRotation>() != null)
            gameObject.GetComponent<MixerRotation>().Play(300);

        if (particles != null) particles.Play();

        // Iniciamos la luz (para el horno pero podria aplicarse para todos los puestos si se necesita en alguno)
        if (myLight != null)
            myLight.FadeIn();

        progressBar.HandleStart(workstationTime);

        yield return new WaitForSeconds(workstationTime); // Esperamos

        progressBar.HandleEnd();

        // Iniciamos la luz (para el horno pero podria aplicarse para todos los puestos si se necesita en alguno)
        if (myLight != null)
            myLight.FadeOut();

        if (particles != null) particles.Stop();

        if (gameObject.GetComponent<MixerRotation>() != null)
            gameObject.GetComponent<MixerRotation>().Stop();

        // Spawn resultado de la receta
        if (data.processedRecipe)
        {
            var go = Instantiate(data.processedRecipe, spawnPoint.position, spawnPoint.rotation);
            OnRecipeCraftedGlobal?.Invoke(data, workstationType);
            if (!data.esIntermedia)
            {
                var cd = go.GetComponent<CompletedRecipe>() ?? go.AddComponent<CompletedRecipe>();
                cd.receta = data;
            }
        }

        // Fin anim & sonido
        if (GetComponent<Animator>() != null)
            AnimatorManager.Instance.ChangeAnimation(animKey, idleAnim);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.StopLoop(soundKey);

        processed = false;
    }
    #endregion
}

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
    [SerializeField] private Light light;

    [Header("Spawn of ingredients")]
    [SerializeField] public Transform spawnPoint;

    [Header("Animation")]
    [SerializeField] private GameObject objAnim; // Objeto visible que tiene animación
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

    private IEnumerator ProcessImmediate(GameObject ingredienteGO, RecetaData data)
    {
        Debug.Log("Procesando receta intermedia");

        processed = true;
        // Anim & sonido inicio      
        AnimatorManager.Instance.PlayAndPauseAt(animKey, processingAnim, animTime);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.PlayLoop(soundKey, processingSfxName);


        if (gameObject.GetComponent<MixerRotation>() != null)
            gameObject.GetComponent<MixerRotation>().Play(300);

        if (particles != null) particles.Play();

        // Iniciamos la luz (para el horno pero podria aplicarse para todos los puestos si se necesita en alguno)
        if (light != null)
            StartCoroutine(HandleLight(workstationTime, 1f)); // tiempo que tarda en encender y apagar

        progressBar.HandleStart(workstationTime);

        yield return new WaitForSeconds(workstationTime); // Esperamos

        progressBar.HandleEnd();

        if (gameObject.GetComponent<MixerRotation>() != null)
            gameObject.GetComponent<MixerRotation>().Stop();

        if (particles != null) particles.Stop();

        // Spawn ingrediente procesado
        if (data.processedRecipe != null)
        {
            Instantiate(data.processedRecipe, spawnPoint.position, spawnPoint.rotation);
            Debug.Log("Ingrediente Procesado");
        }
            

        // Fin anim & sonido
        AnimatorManager.Instance.PlayAndPauseAt(animKey, idleAnim, 0f);
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
            AnimatorManager.Instance.PlayAndPauseAt(animKey, processingAnim, animTime);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.PlayLoop(soundKey, processingSfxName);

        if (gameObject.GetComponent<MixerRotation>() != null)
            gameObject.GetComponent<MixerRotation>().Play(300);

        if (particles != null) particles.Play();

        // Iniciamos la luz (para el horno pero podria aplicarse para todos los puestos si se necesita en alguno)
        if (light != null)
            StartCoroutine(HandleLight(workstationTime, 1f)); // tiempo que tarda en encender y apagar

        progressBar.HandleStart(workstationTime);

        yield return new WaitForSeconds(workstationTime); // Esperamos

        progressBar.HandleEnd();

        if (particles != null) particles.Stop();

        if (gameObject.GetComponent<MixerRotation>() != null)
            gameObject.GetComponent<MixerRotation>().Stop();

        // Spawn resultado de la receta
        if (data.processedRecipe)
        {
            var go = Instantiate(data.processedRecipe, spawnPoint.position, spawnPoint.rotation);
            if (!data.esIntermedia)
            {
                var cd = go.GetComponent<CompletedRecipe>() ?? go.AddComponent<CompletedRecipe>();
                cd.receta = data;
            }
        }

        // Fin anim & sonido
        if (GetComponent<Animator>() != null)
            AnimatorManager.Instance.PlayAndPauseAt(animKey, idleAnim, 0f);
        if (!string.IsNullOrEmpty(processingSfxName))
            KitchenSoundManager.Instance.StopLoop(soundKey);

        processed = false;
    }

    // Corrutina para manejar la luz progresiva
    private IEnumerator HandleLight(float duration, float fadeTime)
    {
        float targetIntensity = 100f;
        float startIntensity = 0f;

        // Encendido progresivo
        float t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            light.intensity = Mathf.Lerp(startIntensity, targetIntensity, t / fadeTime);
            yield return null;
        }

        // Mantener encendida durante el tiempo del puesto menos lo que dura encendido/apagado
        yield return new WaitForSeconds(Mathf.Max(0, duration - 2 * fadeTime));

        // Apagado progresivo
        t = 0f;
        while (t < fadeTime)
        {
            t += Time.deltaTime;
            light.intensity = Mathf.Lerp(targetIntensity, startIntensity, t / fadeTime);
            yield return null;
        }

        light.intensity = 0f;
    }
    #endregion
}

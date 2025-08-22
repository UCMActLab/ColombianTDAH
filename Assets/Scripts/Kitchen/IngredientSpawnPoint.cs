using UnityEngine;
using System.Collections;

public class IngredientSpawnPoint : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] public Ingredientes ingredientType;
    [SerializeField] private GameObject ingredientPrefab; // Prefab a instanciar
    [SerializeField] private Transform spawnTransform;
    [SerializeField] private bool spawnOnStart = true;
    [SerializeField] private float respawnDelay = 1.0f; // Segundos para reaparición

    [Header("Runtime")]
    [SerializeField, Tooltip("Instancia viva actual (si existe)")]
    private GameObject currentInstance; // No se le asigna nada en el inspector

    private bool isRespawning;

    private void Awake()
    {
        if (spawnTransform == null) spawnTransform = this.transform;
        IngredientSpawnManager.Instance?.Register(this);
    }

    private void Start()
    {
        if (spawnOnStart) SpawnNow();
    }

    private void OnDestroy()
    {
        IngredientSpawnManager.Instance?.Unregister(this);
    }

    public void SpawnNow()
    {
        if (currentInstance != null) return; // Evita duplicados

        if (ingredientPrefab == null)
        {
            Debug.LogWarning($"[{name}] ingredientPrefab no asignado.");
            return;
        }

        currentInstance = Instantiate(
            ingredientPrefab,
            spawnTransform.position,
            spawnTransform.rotation);

        var ret = currentInstance.GetComponent<IngredientSpawn>();
        if (ret == null) ret = currentInstance.AddComponent<IngredientSpawn>();
        ret.Init(this, spawnTransform);
    }

    public void ScheduleReactivate(IngredientSpawn ret)
    {
        StartCoroutine(ReactivateRoutine(ret, respawnDelay));
    }

    IEnumerator ReactivateRoutine(IngredientSpawn ret, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (ret == null) yield break;

        // Reposicionamos y activamos el objeto
        if (spawnTransform != null)
        {
            ret.transform.SetPositionAndRotation(spawnTransform.position, spawnTransform.rotation);
        }
        ret.gameObject.SetActive(true);
    }

    public bool HasLiveInstance => currentInstance != null && currentInstance.activeInHierarchy;
    public GameObject CurrentInstance => currentInstance;
}
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
        IngredientSpawnManager.Instance.Register(this);
    }

    private void Start()
    {
        if (spawnOnStart) SpawnNow();
    }

    private void OnDestroy()
    {
        if (IngredientSpawnManager.HasInstance)
            IngredientSpawnManager.Instance.Unregister(this);
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

        // Adjuntamos notificador para ver si se destruye
        var notifier = currentInstance.GetComponent<DespawnNotifier>();
        if (notifier == null)
            notifier = currentInstance.AddComponent<DespawnNotifier>();

        notifier.OnDespawned += HandleDespawned;
    }

    private void HandleDespawned()
    {
        currentInstance = null;
        if (!isRespawning)
            StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        isRespawning = true;
        yield return new WaitForSeconds(respawnDelay);
        isRespawning = false;
        SpawnNow();
    }

    public void ForceRespawn()
    {
        if (currentInstance != null)
        {
            // Desuscribir para evitar dobles llamadas
            var notifier = currentInstance.GetComponent<DespawnNotifier>();
            if (notifier != null) notifier.OnDespawned -= HandleDespawned;

            Destroy(currentInstance);
            currentInstance = null;
        }

        StopAllCoroutines();
        isRespawning = false;
        SpawnNow();
    }

    public bool HasLiveInstance => currentInstance != null;
    public GameObject CurrentInstance => currentInstance;
}
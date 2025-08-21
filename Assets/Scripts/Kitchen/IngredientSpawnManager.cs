using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class IngredientSpawnManager : MonoBehaviour
{
    private static IngredientSpawnManager _instance;
    public static IngredientSpawnManager Instance
    {
        get
        {
            return _instance;
        }
    }
    public static bool HasInstance => _instance != null;

    private readonly Dictionary<Ingredientes, IngredientSpawnPoint> byType =
        new Dictionary<Ingredientes, IngredientSpawnPoint>();

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void Register(IngredientSpawnPoint sp)
    {
        if (byType.ContainsKey(sp.ingredientType))
        {
            Debug.LogWarning($"Ya existe un spawn para {sp.ingredientType}. Reemplazando referencia por el nuevo [{sp.name}].");
            byType[sp.ingredientType] = sp;
        }
        else
        {
            byType.Add(sp.ingredientType, sp);
        }
    }

    public void Unregister(IngredientSpawnPoint sp)
    {
        if (byType.TryGetValue(sp.ingredientType, out var current) && current == sp)
            byType.Remove(sp.ingredientType);
    }

    public void SpawnAllMissing()
    {
        foreach (var sp in byType.Values)
            if (!sp.HasLiveInstance) sp.SpawnNow();
    }

    public GameObject GetLiveInstance(Ingredientes tipo)
    {
        if (byType.TryGetValue(tipo, out var sp))
            return sp.CurrentInstance;
        return null;
    }

    public IngredientSpawnPoint GetSpawnPoint(Ingredientes tipo)
    {
        byType.TryGetValue(tipo, out var sp);
        return sp;
    }
}
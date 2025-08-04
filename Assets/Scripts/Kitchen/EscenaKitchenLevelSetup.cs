using UnityEngine;

public class EscenaKitchenLevelSetup : MonoBehaviour
{

    [SerializeField] private GameObject[] lights;
    [SerializeField] private GameObject libroDeRecetas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LevelKitchenManager.Instance.SetLibro(libroDeRecetas);
        LevelKitchenManager.Instance.SetLights(lights);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

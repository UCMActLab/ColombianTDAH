using UnityEngine;

public class MisionLevelManager : MonoBehaviour
{
    // Singleton
    static private MisionLevelManager _instance;
    public static MisionLevelManager Instance { get { return _instance; } }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Mision Colombia emppieza");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void DialogEnded()
    {
        // Show buttons ui
        Debug.Log("Botones para escoger");
    }
}

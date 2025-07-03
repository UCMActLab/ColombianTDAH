using UnityEngine;

public class MisionLevelManager : MonoBehaviour
{
    // Singleton
    static private MisionLevelManager _instance;
    public static MisionLevelManager Instance { get { return _instance; } }

    [SerializeField]
    MisionUIManager misionUIManager;

    private void Awake()
    {
        // Si no hay instancia de esta clase ya creada se almacena
        if (_instance == null)
            _instance = this;
        // Si esta creada se destruye porque no necesitamos una mas
        else
            Destroy(this.gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Mision Colombia emppieza");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowDecisionButtons()
    {
        // Show buttons ui
        Debug.Log("Botones para escoger");
        misionUIManager.ShowDecisionButtons();
    }
}

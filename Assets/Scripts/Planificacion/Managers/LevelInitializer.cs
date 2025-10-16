using UnityEngine;

public class LevelInitializer : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MisionLevelManager.Instance.InitLevel();

        Destroy(gameObject);
    }
}

using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader _instance = null;

    public int levelId;
    public static bool teacherMode;
    
    void Awake()
    {
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

        levelId = 01;
        teacherMode = false;
        DontDestroyOnLoad(this.gameObject);
    }

    public static void LoadScene(string name = "DolphinLevel")
    {
        Debug.Log("Loading scene: " + name);
        if (name == "dolphin")
            name = "DolphinLevel";
        if (name == "DolphinLevelSelector")
        {
            teacherMode = false;
        }
        else if (name == "DolphinLevel")
        {
            //Solo pasaremos a los dialogos si vamos desde el selector de niveles
            if (!teacherMode && SceneManager.GetActiveScene().name == "DolphinLevelSelector")
            {
                name = "Dialogs";
                Debug.Log("NIÑO MODE");
            }
        }

        SceneManager.LoadScene(name);
    }

    public void setMode(bool mode)
    {
        teacherMode = mode;
    }

    public bool getMode()
    {
        return teacherMode;
    }

    public int getLevelId()
    {
        return levelId;
    }

    public void setLevelId(int id)
    {
        levelId = id;
    }
}

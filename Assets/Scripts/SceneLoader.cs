using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    private static SceneLoader _instance = null;

    public int levelId;
    public static bool teacherMode;
    private bool lastLevelWon;
    
    void Awake()
    {
        if(_instance == null)
        {
            levelId = 1; // Default level ID
            lastLevelWon = false; // Default last level won state
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }

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
        if (name == "Worlds") //Botón de exit del minijuego
        {
            EventRegister.Instance.WriteEnd();
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

    public bool getLastLevelWon()
    {
        return lastLevelWon;
    }

    public void setLastLevelWon(bool won)
    {
        lastLevelWon = won;
    }
}

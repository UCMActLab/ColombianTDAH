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

    public static void LoadScene(string name = "dolphin")
    {
        if (name == "DolphinLevelSelector")
        {
            teacherMode = false;
        }
        else if (name == "dolphin")
        {
            
            if (teacherMode)
            {
                name = "DolphinLevel_1";
                Debug.Log("TEACHER MODE");
            }
            else
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
}

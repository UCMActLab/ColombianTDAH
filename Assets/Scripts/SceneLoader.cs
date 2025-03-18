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
        DontDestroyOnLoad(this.gameObject);
    }

    public static void LoadScene(string name = "dolphin")
    {
        if (name == "dolphin")
        {
            if (teacherMode)
            {
                Debug.Log("Loading teacher mode");
                name = "DolphinLevel_1";
            }
            else
            {
                Debug.Log("Loading NOT teacher mode");
                name = "Dialogs";
            }
        }
        SceneManager.LoadScene(name);
    }

    public void setMode(bool mode)
    {
        teacherMode = !teacherMode;
        Debug.Log("Teacher mode: " + teacherMode);
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

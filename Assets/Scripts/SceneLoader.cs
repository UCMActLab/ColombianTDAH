using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public int levelId;
    public static bool teacherMode;

    void Awake()
    {
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

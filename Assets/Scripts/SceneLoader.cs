using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using static EventRegister;

public class SceneLoader : MonoBehaviour
{
    static private SceneLoader _instance = null;

    public static SceneLoader Instance { get { return _instance; } }

    [System.Serializable] //por si apetece ponerlos fuera y editarlos
    public class GameSceneData
    {
        public string selectorScene;
        public string levelScene;
        public string introScene;
        public int currentLevelId;
        public int maxLevelIdUnlocked;
    }

    public static bool teacherMode;
    public static bool defaultConfig;

    //saber si se ha hecho el dialogo de los delfines o no
    // private static bool dialogSeen = false;
    private Dictionary<TipoJuego, GameSceneData> juegos = new Dictionary<TipoJuego, GameSceneData>();

    // Configuracion de cada juego

    [SerializeField]
    GameSceneData delfinesData = new GameSceneData
    {
        selectorScene = "DolphinLevelSelector",
        levelScene = "DolphinLevel",
        introScene = "Dialogs",
        currentLevelId = 1,
        maxLevelIdUnlocked = 1
    };

    [SerializeField]
    GameSceneData misionColombiaData = new GameSceneData
    {
        selectorScene = "MisionColombiaLevelSelector",
        levelScene = "MC_Level",
        introScene = "MC_Mapa",
        currentLevelId = 1,
        maxLevelIdUnlocked = 1
    };

    void Awake()
    {
        if(_instance == null)
        {
          
            juegos[TipoJuego.Delfines] = delfinesData;
            juegos[TipoJuego.MisionColombia] = misionColombiaData;


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
            if (!teacherMode && SceneManager.GetActiveScene().name == "DolphinLevelSelector" && _instance.getCurrentLevelId(TipoJuego.Delfines) == 1)
            { 

                //dialogSeen = true;
                name = "Dialogs";
                Debug.Log("NI�O MODE");
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

    public void setIsDefaultConfig(bool isDefault)
    {
        defaultConfig = isDefault;
    }

    public bool getIsDefaultConfig()
    {
        return defaultConfig;
    }

    //getters y setters tanto en int como con enum

    // SelectorScene
    public string getSelectorScene(TipoJuego tipo = TipoJuego.Delfines)
    {
        return juegos[tipo].selectorScene;
    }
    public string getSelectorScene(int tipoJuegoEnum)
    {
        return getSelectorScene(ParseTipoJuego(tipoJuegoEnum));
    }

    public void setSelectorScene(string scene, TipoJuego tipo = TipoJuego.Delfines)
    {
        juegos[tipo].selectorScene = scene;
    }
    public void setSelectorScene(string scene, int tipoJuegoEnum)
    {
        setSelectorScene(scene, ParseTipoJuego(tipoJuegoEnum));
    }

    // LevelScene
    public string getLevelScene(TipoJuego tipo = TipoJuego.Delfines)
    {
        return juegos[tipo].levelScene;
    }
    public string getLevelScene(int tipoJuegoEnum)
    {
        return getLevelScene(ParseTipoJuego(tipoJuegoEnum));
    }

    public void setLevelScene(string scene, TipoJuego tipo = TipoJuego.Delfines)
    {
        juegos[tipo].levelScene = scene;
    }
    public void setLevelScene(string scene, int tipoJuegoEnum)
    {
        setLevelScene(scene, ParseTipoJuego(tipoJuegoEnum));
    }

    // IntroScene
    public string getIntroScene(TipoJuego tipo)
    {
        return juegos[tipo].introScene;
    }
    public string getIntroScene(int tipoJuegoEnum)
    {
        return getIntroScene(ParseTipoJuego(tipoJuegoEnum));
    }

    public void setIntroScene(string scene, TipoJuego tipo)
    {
        juegos[tipo].introScene = scene;
    }
    public void setIntroScene(string scene, int tipoJuegoEnum)
    {
        setIntroScene(scene, ParseTipoJuego(tipoJuegoEnum));
    }

    // LevelId
    public int getCurrentLevelId(TipoJuego tipo)
    {
        return juegos[tipo].currentLevelId;
    }
    public int getCurrentLevelId(int tipoJuegoEnum)
    {
        return getCurrentLevelId(ParseTipoJuego(tipoJuegoEnum));
    }

    public void setCurrentLevelId(int id, TipoJuego tipo)
    {
        juegos[tipo].currentLevelId = id;
    }
    public void setCurrentLevelId(int id, int tipoJuegoEnum)
    {
        setCurrentLevelId(id, ParseTipoJuego(tipoJuegoEnum));
    }
    // LevelId
    public int getMaxLevelId(TipoJuego tipo)
    {
        return juegos[tipo].maxLevelIdUnlocked;
    }
    public int getMaxLevelId(int tipoJuegoEnum)
    {
        return getMaxLevelId(ParseTipoJuego(tipoJuegoEnum));
    }

    public void setMaxLevelId(int id, TipoJuego tipo)
    {
        juegos[tipo].maxLevelIdUnlocked = id;
    }
    public void setMaxLevelId(int id, int tipoJuegoEnum)
    {
        setMaxLevelId(id, ParseTipoJuego(tipoJuegoEnum));
    }

    //para pasar de int a enum
    private TipoJuego ParseTipoJuego(int tipoJuegoEnum)
    {
        if (Enum.IsDefined(typeof(TipoJuego), tipoJuegoEnum))
        {
            return (TipoJuego)tipoJuegoEnum;
        }
        Debug.LogWarning($"SceneLoader.ParseTipoJuego, tipoIdEnum {tipoJuegoEnum} no es valido, se ha puesto el valor delfines (1) por defecto");
        return TipoJuego.Delfines;
    }




}

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
        public int levelId;
        public bool lastLevelWon;

    }

    public static bool teacherMode;

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
        levelId = 1
    };

    [SerializeField]
    GameSceneData misionColombiaData = new GameSceneData
    {
        selectorScene = "MisionColombiaLevelSelector",
        levelScene = "MC_Level",
        introScene = "MC_Mapa",
        levelId = 1
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
            if (!teacherMode && SceneManager.GetActiveScene().name == "DolphinLevelSelector" && _instance.getLevelId() == 1)
            { 

                //dialogSeen = true;
                name = "Dialogs";
                Debug.Log("NI�O MODE");
            }
        }
        if (name == "Worlds") //Bot�n de exit del minijuego
        {
            if (EventRegister.Instance != null)
            {
                EventRegister.Instance.WriteEnd();
            }
            else
            {
                Debug.LogWarning("EventRegister.Instance es null, SceneLoader.LoadScene() del boton de Atras de Worlds");
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

    //getters y setters tanto en int como con enum
    public bool getLastLevelWon(TipoJuego tipo = TipoJuego.Delfines)
    {
        return juegos[tipo].lastLevelWon;
    }
    public bool getLastLevelWon(int tipoJuegoEnum)
    {
        return getLastLevelWon(ParseTipoJuego(tipoJuegoEnum));
    }

    public void setLastLevelWon(bool won, TipoJuego tipo = TipoJuego.Delfines)
    {
        juegos[tipo].lastLevelWon = won;
    }
    public void setLastLevelWon(bool won, int tipoJuegoEnum)
    {
        setLastLevelWon(won, ParseTipoJuego(tipoJuegoEnum));
    }

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
    public string getIntroScene(TipoJuego tipo = TipoJuego.Delfines)
    {
        return juegos[tipo].introScene;
    }
    public string getIntroScene(int tipoJuegoEnum)
    {
        return getIntroScene(ParseTipoJuego(tipoJuegoEnum));
    }

    public void setIntroScene(string scene, TipoJuego tipo = TipoJuego.Delfines)
    {
        juegos[tipo].introScene = scene;
    }
    public void setIntroScene(string scene, int tipoJuegoEnum)
    {
        setIntroScene(scene, ParseTipoJuego(tipoJuegoEnum));
    }

    // LevelId
    public int getLevelId(TipoJuego tipo = TipoJuego.Delfines)
    {
        return juegos[tipo].levelId;
    }
    public int getLevelId(int tipoJuegoEnum)
    {
        return getLevelId(ParseTipoJuego(tipoJuegoEnum));
    }

    public void setLevelId(int id, TipoJuego tipo = TipoJuego.Delfines)
    {
        juegos[tipo].levelId = id;
    }
    public void setLevelId(int id, int tipoJuegoEnum)
    {
        setLevelId(id, ParseTipoJuego(tipoJuegoEnum));
    }

    //para pasar de int a enum
    private TipoJuego ParseTipoJuego(int tipoJuegoEnum)
    {
        if (Enum.IsDefined(typeof(TipoJuego), tipoJuegoEnum))
        {
            return (TipoJuego)tipoJuegoEnum;
        }
        Debug.LogWarning($"SceneLoader.SetLevelId, tipoId {tipoJuegoEnum} no es valido, se ha puesto el valor delfines (1) por defecto");
        return TipoJuego.Delfines;
    }




}

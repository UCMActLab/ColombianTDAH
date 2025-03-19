using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Windows;
using System.Linq;


public class configUi : MonoBehaviour
{
    [SerializeField]
    GameObject environmentObject;
    [SerializeField]
    GameObject managerstObject;
    [SerializeField]
    GameObject canvasObject;

    VisualElement fase1;
    VisualElement fase2;
    VisualElement fase3;

    IntegerField input_carrilesN;
    IntegerField input_delfinesN;
    Button input_fase1Complet;
    IntegerField input_obstacleMin;
    IntegerField input_obstacleMax;
    Toggle input_piruetasSimult;
    IntegerField input_jumpMin;
    IntegerField input_jumpMax;
    IntegerField input_piruetMin;
    IntegerField input_piruetMax;
    FloatField input_obstacleVel;
    IntegerField input_pointMax;
    IntegerField input_pointPirueta;
    IntegerField input_pointWrongGuess;
    IntegerField input_pointChoque;
    Toggle input_floatiesEnabled;
    Toggle input_obstaclesEnabled;
    Button input_guardar;
    VisualElement input_toggleGroup;
    Label text_delfinesRestantes;
    IntegerField input_whaleRightGuess;
    FloatField input_increasedVelFactor;
    IntegerField input_pointsPiruetaVelocidad;
    IntegerField input_pointsFloatie;
    IntegerField input_pointsFloatieVelocidad;
    int delfinesColocados = 0;
    bool hayErrores = true;
    bool mensajeError = false;


    [SerializeField]
    configData config = null;
    string levelInfoPath = "";

    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        fase1 = root.Q("Fase1");
        fase2 = root.Q("Fase2");
        fase3 = root.Q("Fase3");

        input_carrilesN = root.Q<IntegerField>("carrilesN");
        input_delfinesN = root.Q<IntegerField>("delfinesN");
        input_fase1Complet = root.Q<Button>("fase1Complet");
        input_obstacleMin = root.Q<IntegerField>("obstacleMin");
        input_obstacleMax = root.Q<IntegerField>("obstacleMax");
        input_piruetasSimult = root.Q<Toggle>("piruetasSimult");
        input_jumpMin = root.Q<IntegerField>("jumpMin");
        input_jumpMax = root.Q<IntegerField>("jumpMax");
        input_piruetMin = root.Q<IntegerField>("piruetMin");
        input_piruetMax = root.Q<IntegerField>("piruetMax");
        input_obstacleVel = root.Q<FloatField>("obstacleVel");
        input_pointMax = root.Q<IntegerField>("pointMax");
        input_pointPirueta = root.Q<IntegerField>("pointPirueta");
        input_pointWrongGuess = root.Q<IntegerField>("pointWrongGuess");
        input_pointChoque = root.Q<IntegerField>("pointChoque");
        input_guardar = root.Q<Button>("guardar");
        input_toggleGroup = root.Q<VisualElement>("toggleGroup");
        text_delfinesRestantes = root.Q<Label>("delfinesRestantes");
        input_floatiesEnabled = root.Q<Toggle>("floatEnabled");
        input_obstaclesEnabled = root.Q<Toggle>("obsEnabled");

        input_whaleRightGuess = root.Q<IntegerField>("whaleRightGuess");
        input_increasedVelFactor = root.Q<FloatField>("velFactor");
        input_pointsPiruetaVelocidad = root.Q<IntegerField>("pointPiruetaVelocidad");
        input_pointsFloatie = root.Q<IntegerField>("pointFlotador");
        input_pointsFloatieVelocidad = root.Q<IntegerField>("pointFlotadorVelocidad");

        //Debug.Log(input_whaleRightGuess.value + " " + input_increasedVelFactor + " " + input_pointsPiruetaVelocidad + " " + input_pointsFloatie + " " + input_pointsFloatieVelocidad);
        input_guardar.RegisterCallback<ClickEvent>(GuardarTodo);
        input_fase1Complet.RegisterCallback<ClickEvent>(Fase1Complet);
        input_toggleGroup.RegisterCallback<ClickEvent>(DelfinColocado);

        // Busca si tiene que cargar la configuración
        SceneLoader sceneLoader = GameObject.Find("SceneLoader").GetComponent<SceneLoader>();
        bool editMode = sceneLoader.getMode();
        int levelId = sceneLoader.getLevelId();
        string writeDir = System.IO.Path.Combine(Application.persistentDataPath, "configInfo");
        if (!System.IO.Directory.Exists(writeDir))
        {
            System.IO.Directory.CreateDirectory(writeDir);
        }
        levelInfoPath = System.IO.Path.Combine(writeDir, "configData" + levelId.ToString("00") + ".json");

        if (editMode) //el usuario quiere editar el juego
        {
            // Desactiva Juego
            ActivateGame(false);
        }
        else //se carga el nivel por default
        {
            // Activa Juego
            LoadLevelConfig();
        }
    }

    void GuardarTodo(ClickEvent e)
    {
        if (hayErrores)
        {
            if (!mensajeError)
            {
                mensajeError = true;
                VisualTreeAsset uiAsset = Resources.Load<VisualTreeAsset>("UI/errores");
                VisualElement ui = uiAsset.Instantiate();
                ui.style.color = new Color(1, 0, 0, 1);
                fase3.Add(ui);
            }
        }
        else
        {
            //RIVER CONFIG
            config.NumCarriles = input_carrilesN.value;
            config.NumDelfines = input_delfinesN.value;

            // Posiciones de delfines: Active Toggles to Vector2d
            Vector2[] posDelfines = new Vector2[input_delfinesN.value];
            int i = 0;
            int delfinesEncontrados = 0;
            while (i < input_toggleGroup.childCount)
            {
                int j = 0;
                while (j < input_toggleGroup[i][0].childCount)
                {
                    if (input_toggleGroup[i][0][j].Q<Toggle>().value)
                    {
                        if (delfinesEncontrados < input_delfinesN.value)
                        {
                            posDelfines[delfinesEncontrados] = new Vector2(i, j); //si se ha pasado se fastidian los ultimos :p
                        }
                        delfinesEncontrados++;
                    }
                    j++;
                }
                i++;
            }

            for (int d = delfinesEncontrados; d < config.NumDelfines; d++)
            {
                posDelfines[d] = new Vector2(-1, -1);
            }

            config.PosDelfines = posDelfines;

            //OBS
            config.MinObstacleSpawn = input_obstacleMin.value;
            config.MaxObstacleSpawn = input_obstacleMax.value;
            config.ObstacleSpeed = input_obstacleVel.value;
            config.ObstaclesEnabled = input_obstaclesEnabled.value;
            config.IncreasedSpeedFactor = input_increasedVelFactor.value;

            //FLOATS
            config.FloatsEnabled = input_floatiesEnabled.value;

            //JUMP
            config.canSpecialJumpSimultaneously = input_piruetasSimult.value;
            config.MinTimeBetweenJumps = input_jumpMin.value;
            config.MaxTimeBetweenJumps = input_jumpMax.value;
            config.MinCountBetweenSpecialJumps = input_piruetMin.value;
            config.MaxCountBetweenSpecialJumps = input_piruetMax.value;

            //POINTS
            config.LevelPoints = input_pointMax.value;
            config.RightGuessPoints = input_pointPirueta.value;
            config.RightGuessPointsVel = input_pointsPiruetaVelocidad.value;
            config.FloatiePoints = input_pointsFloatie.value;
            config.FloatiePointsVel = input_pointsFloatieVelocidad.value;
            config.WrongGuessPoints = input_pointWrongGuess.value;
            config.HitObstaclePoints = input_pointChoque.value;

            // Whale
            config.WhaleApearingGuests = input_whaleRightGuess.value;

            //Guarda la configuración en el json correspondiente
            SaveLevelConfig(config);
            ActivateGame(true);
        }
    }

    private void SaveLevelConfig(configData config)
    {
        string info = JsonUtility.ToJson(config, true);

        Debug.Log("Saving level config at " + levelInfoPath);

        System.IO.FileStream fs = new System.IO.FileStream(levelInfoPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
        file.WriteLine(info);
        file.Close();
        fs.Close();
    }

    private void LoadLevelConfig()
    {
        if (!System.IO.File.Exists(levelInfoPath))
        {
            Debug.LogError("No existe el archivo de configuración en " + levelInfoPath);
            Debug.LogError("Cargaremos el default");
            config = Resources.Load<configData>("default01_configData");
            Debug.Log(config);

            ActivateGame(true);
        }
        else
        {
            try
            {
                string levelInfo = System.IO.File.ReadAllText(levelInfoPath);
                JsonUtility.FromJsonOverwrite(levelInfo, config);
                Debug.Log(config);

                ActivateGame(true);
            }
            catch (System.Exception e)
            {
                Debug.LogError("Error loading level config: " + e.Message);
                ActivateGame(false);
            }
        }
    }

    void Update()
    {
        if (!hayErrores && mensajeError)
        {
            mensajeError = false;
            fase3.RemoveAt(fase3.childCount - 1);
        }
    }

    void Fase1Complet(ClickEvent e)
    {
        delfinesColocados = 0;
        input_toggleGroup.Clear(); //Borramos los carriles
        int n = input_carrilesN.value;
        if (n > 6) n = 6;
        for (int i = 0; i < n; i++)
        {
            VisualTreeAsset uiAsset = Resources.Load<VisualTreeAsset>("UI/carrilToggles");
            VisualElement ui = uiAsset.Instantiate();

            for (int j = 0; j < ui.childCount; j++)
            {
                ui[j].name = "Toggle" + i.ToString() + j.ToString();
            }

            input_toggleGroup.Add(ui);
        }
    }

    void DelfinColocado(ClickEvent e)
    {
        delfinesColocados = 0;

        for (int i = 0; i < input_toggleGroup.childCount; i++)
        {
            for (int j = 0; j < input_toggleGroup[i][0].childCount; j++)
            {
                if (input_toggleGroup[i][0][j].Q<Toggle>().value)
                {
                    delfinesColocados++;
                }
            }
        }
        int delfinesRestantes = input_delfinesN.value - delfinesColocados;
        if (delfinesRestantes < 0)
        {
            hayErrores = true;
            text_delfinesRestantes.style.color = new Color(1, 0, 0, 1);
            text_delfinesRestantes.text = "Por favor retire " + Mathf.Abs(delfinesRestantes) + " delfines. \r\n Como mucho puede tener " + input_delfinesN.value + " delfines en el río.";
        }
        else
        {
            hayErrores = false;
            text_delfinesRestantes.style.color = new Color(0, 0, 0, 1);
            text_delfinesRestantes.text = "Ahora mismo tienes " + delfinesRestantes + " delfines restantes en el \r\njuego, aparecerán buceando.";
        }
    }

    void ActivateGame(bool enable)
    {
        environmentObject.SetActive(enable);
        managerstObject.SetActive(enable);
        canvasObject.SetActive(enable);
        if (enable) DolphinLevelManager.Instance.InitLevel(config);
    }
}
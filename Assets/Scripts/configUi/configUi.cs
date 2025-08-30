using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Windows;
using System.Linq;
using UnityEngine.SceneManagement;


public class configUi : MonoBehaviour
{
    [SerializeField]
    GameObject environmentObject;
    [SerializeField]
    GameObject managerstObject;
    [SerializeField]
    GameObject canvasObject;

    [SerializeField]
    GameObject backButton;

    VisualElement fase1;
    VisualElement fase2;
    VisualElement fase3C;

    //Fase1y2
    IntegerField input_carrilesN;
    IntegerField input_delfinesN;
    Button input_fase1Complet;
    VisualElement input_toggleGroup;
    Label text_delfinesRestantes;

    //Fase3
    //A
    Toggle input_obstaclesTroncoEnabled;
    Toggle input_obstaclesBarcaEnabled;
    Toggle input_floatiesEnabled;
    Toggle input_ballsEnabled;
    IntegerField input_obstacleT;
    IntegerField input_jumpT;
    Toggle input_piruetasSimult;
    IntegerField input_piruetMin;
    IntegerField input_whaleRightGuess;
    Toggle input_diffSpecies;

    //B
    FloatField input_obstacleVel;
    FloatField input_increasedVelFactor;

    //C
    IntegerField input_pointPirueta;
    IntegerField input_pointsPiruetaVelocidad;
    IntegerField input_pointsFloatie;
    IntegerField input_pointsFloatieVelocidad;
    IntegerField input_pointsBallHit;
    IntegerField input_pointsBallVelocidad;
    IntegerField input_pointsBallMiss;
    IntegerField input_pointMax;
    IntegerField input_pointWrongGuess;
    IntegerField input_pointChoqueTronco;
    IntegerField input_pointChoqueBarca;

    Toggle input_desbloqueado;
    Button input_guardarYjugar;
    Button input_guardarYvolver;

    int delfinesColocados = 0;
    bool hayErrores = true;
    bool mensajeError = false;

    [SerializeField]
    configData config = null;
    int levelId = 1;
    string levelInfoPath = "";

    private void OnEnable()
    {
        RegisterElems();

        SceneLoader sceneLoader = GameObject.Find("SceneLoader").GetComponent<SceneLoader>();
        bool editMode = sceneLoader.getMode();
        levelId = sceneLoader.getCurrentLevelId(EventRegister.TipoJuego.Delfines);
        string writeDir = System.IO.Path.Combine(Application.persistentDataPath, "configInfo");

        Debug.Log("LEVEL ID " + levelId);
        if (editMode) //el usuario quiere editar el juego
        {
            if (!System.IO.Directory.Exists(writeDir))
            {
                System.IO.Directory.CreateDirectory(writeDir);
            }
            levelInfoPath = System.IO.Path.Combine(writeDir, "configData" + levelId.ToString("00") + ".json");
            
            //si el usuario quiere usar la plantilla default del config o quiere que se ponga la ultima que guardo
            if(!sceneLoader.getIsDefaultConfig())SetUIFromJSONFull(); 

            // Desactiva Juego
            ActivateGame(false);
        }
        else //se carga el nivel por default
        {
            levelInfoPath = "default" + levelId.ToString("00") + "_configData";
            // Activa Juego
            LoadLevelConfig(true);
        }
    }

    private void RegisterElems()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        fase1 = root.Q("Fase1");
        fase2 = root.Q("Fase2");
        fase3C = root.Q("C");

        input_carrilesN = root.Q<IntegerField>("carrilesN");
        input_delfinesN = root.Q<IntegerField>("delfinesN");
        input_fase1Complet = root.Q<Button>("fase1Complet");

        input_obstaclesTroncoEnabled = root.Q<Toggle>("obsTroncoEnabled");
        input_obstaclesBarcaEnabled = root.Q<Toggle>("obsBarcasEnabled");
        input_floatiesEnabled = root.Q<Toggle>("floatEnabled");
        input_ballsEnabled = root.Q<Toggle>("ballsEnabled");
        input_obstacleT = root.Q<IntegerField>("obstacleT");
        input_jumpT = root.Q<IntegerField>("jumpT");
        input_piruetasSimult = root.Q<Toggle>("piruetasSimult");
        input_piruetMin = root.Q<IntegerField>("piruetMin");
        input_whaleRightGuess = root.Q<IntegerField>("whaleRightGuess");
        input_diffSpecies = root.Q<Toggle>("diffSpecies");

        input_obstacleVel = root.Q<FloatField>("obstacleVel");
        input_increasedVelFactor = root.Q<FloatField>("velFactor");

        input_pointPirueta = root.Q<IntegerField>("pointPirueta");
        input_pointsPiruetaVelocidad = root.Q<IntegerField>("pointPiruetaVelocidad");
        input_pointsFloatie = root.Q<IntegerField>("pointFlotador");
        input_pointsFloatieVelocidad = root.Q<IntegerField>("pointFlotadorVelocidad");
        input_pointsBallHit = root.Q<IntegerField>("pointPelotaHit");
        input_pointsBallMiss = root.Q<IntegerField>("pointPelotaMiss");
        input_pointsBallVelocidad = root.Q<IntegerField>("pointPelotaVelocidad");
        input_pointMax = root.Q<IntegerField>("pointMax");
        input_pointWrongGuess = root.Q<IntegerField>("pointWrongGuess");
        input_pointChoqueTronco = root.Q<IntegerField>("pointChoqueTronco");
        input_pointChoqueBarca = root.Q<IntegerField>("pointChoqueBarca");

        input_desbloqueado = root.Q<Toggle>("desbloqueado");
        input_guardarYjugar = root.Q<Button>("guardarYjugar");
        input_guardarYvolver = root.Q<Button>("guardarYvolver");

        input_toggleGroup = root.Q<VisualElement>("toggleGroup");
        text_delfinesRestantes = root.Q<Label>("delfinesRestantes");
        
        input_guardarYjugar.RegisterCallback<ClickEvent>(GuardarTodoyJugar);
        input_guardarYvolver.RegisterCallback<ClickEvent>(GuardarTodoyVolver);
        input_fase1Complet.RegisterCallback<ClickEvent>(Fase1Complet);
        input_toggleGroup.RegisterCallback<ClickEvent>(DelfinColocado);

    }

    void GuardarTodoyJugar(ClickEvent e)
    {
        if (Guardar())
        {
            if (config.Desbloqueado)
            {
                ActivateGame(true);
            }
            else
            {
                Debug.Log("El nivel no está desbloqueado");
            }
        }        
    }

    void GuardarTodoyVolver(ClickEvent e)
    {
        if (Guardar())
        {
            SceneManager.LoadScene("DolphinLevelSelector");
        }
    }

    private bool Guardar()
    {
        if (hayErrores)
        {
            if (!mensajeError)
            {
                mensajeError = true;
                VisualTreeAsset uiAsset = Resources.Load<VisualTreeAsset>("UI/errores");
                VisualElement ui = uiAsset.Instantiate();
                ui.style.color = new Color(1, 0, 0, 1);
                fase3C.Add(ui);
            }
            return false;
        }
        else
        {
            config.configName = "configData" + levelId.ToString("00");

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
            config.ObstaclesTroncoEnabled = input_obstaclesTroncoEnabled.value;
            config.ObstaclesBarcaEnabled = input_obstaclesBarcaEnabled.value;
            //FLOATS AND BALLS
            config.FloatsEnabled = input_floatiesEnabled.value;
            config.BallsEnabled = input_ballsEnabled.value;

            config.MinObstacleSpawn = input_obstacleT.value -1;
            if (config.MinObstacleSpawn < 1) config.MinObstacleSpawn = 1;
            config.MaxObstacleSpawn = input_obstacleT.value + 1;
            config.ObstacleSpeed = input_obstacleVel.value;
            config.IncreasedSpeedFactor = input_increasedVelFactor.value;

            // Whale and diff species pirueting
            config.WhaleApearingGuests = input_whaleRightGuess.value;
            config.DiffSpeciesEnabled = input_diffSpecies.value;

            //JUMP
            config.canSpecialJumpSimultaneously = input_piruetasSimult.value;
            config.MinTimeBetweenJumps = input_jumpT.value - 1;
            if (config.MinTimeBetweenJumps < 1) config.MinTimeBetweenJumps = 1;
            config.MaxTimeBetweenJumps = input_jumpT.value + 1;
            config.MinCountBetweenSpecialJumps = input_piruetMin.value - 1;
            if (config.MinCountBetweenSpecialJumps < 1) config.MinCountBetweenSpecialJumps = 1;
            config.MaxCountBetweenSpecialJumps = input_piruetMin.value + 1;

            //POINTS
            config.LevelPoints = input_pointMax.value;
            config.RightGuessPoints = input_pointPirueta.value;
            config.RightGuessPointsVel = input_pointsPiruetaVelocidad.value;
            config.FloatiePoints = input_pointsFloatie.value;
            config.FloatiePointsVel = input_pointsFloatieVelocidad.value;
            config.BallHitPoints = input_pointsBallHit.value;
            config.BallMissPoints = input_pointsBallMiss.value;
            //hacer tmbn miss
            config.BallHitPointsVel = input_pointsBallVelocidad.value;
            config.WrongGuessPoints = input_pointWrongGuess.value;
            config.HitObstacleTroncoPoints = input_pointChoqueTronco.value;
            config.HitObstacleBarcaPoints = input_pointChoqueBarca.value;

            config.Desbloqueado = input_desbloqueado.value;

            //Guarda la configuración en el json correspondiente
            SaveLevelConfig(config);

            return true;
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

    private void LoadLevelConfig(bool isDefault)
    {

        Debug.Log("Loading level config from " + levelInfoPath);
        if (isDefault) // si es el default no existirá el archivo ya que no es un path como tal
        {
            Debug.Log("Cargaremos el default");
            config = Resources.Load<configData>(levelInfoPath);

            // Los niveles por defecto están desbloqueados, pero se hace la comprobación por si acaso
            if (config.Desbloqueado)
            {
                ActivateGame(true);
            }
            else
            {
                Debug.Log("El nivel no está desbloqueado");
            }
        }
        else
        {
            Debug.Log("INTENTANDO LEER DE DE JSON");
            string writeDir = System.IO.Path.Combine(Application.persistentDataPath, "configInfo");
            levelInfoPath = System.IO.Path.Combine(writeDir, "configData" + levelId.ToString("00") + ".json");

            try
            {
                Debug.Log("LEYENDO DE desde el JSON desde el path persistente");

                string levelInfo = System.IO.File.ReadAllText(levelInfoPath);
                JsonUtility.FromJsonOverwrite(levelInfo, config);
                
                if (config.Desbloqueado)
                {


                    ActivateGame(true);
                }
                else
                {
                    Debug.Log("El nivel no está desbloqueado");
                }
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
            fase3C.RemoveAt(fase3C.childCount - 1);
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
        backButton.SetActive(!enable);
        if (enable) DolphinLevelManager.Instance.InitLevel(config);
    }

    private void SetUIFromJSONFull()
    {

        Debug.Log("SetUIFromJSONFull Before " + levelInfoPath);

        string writeDir = System.IO.Path.Combine(Application.persistentDataPath, "configInfo");
        levelInfoPath = System.IO.Path.Combine(writeDir, "configData" + levelId.ToString("00") + ".json");

        // Verificar si el archivo existe antes de leerlo
        if (!System.IO.File.Exists(levelInfoPath)) return;

        Debug.Log("SetUIFromJSONFull after " + levelInfoPath);

        string levelInfo = System.IO.File.ReadAllText(levelInfoPath);
        JsonUtility.FromJsonOverwrite(levelInfo, config);
        SetUIFromJSON();
    }
    private void SetUIFromJSON()
    {
        Debug.Log("SetUIFromJSON");

        input_carrilesN.value = config.NumCarriles;
        input_delfinesN.value = config.NumDelfines;
        //input_fase1Complet = config.fa;

        input_obstaclesTroncoEnabled.value = config.ObstaclesTroncoEnabled;
        input_obstaclesBarcaEnabled.value = config.ObstaclesBarcaEnabled;
        input_floatiesEnabled.value = config.FloatsEnabled;
        input_ballsEnabled.value = config.BallsEnabled;
        input_obstacleT.value = (int)config.MinObstacleSpawn;
        input_jumpT.value = (int)config.MinTimeBetweenJumps;
        input_piruetasSimult.value = config.canSpecialJumpSimultaneously;
        input_piruetMin.value = (int)config.MinCountBetweenSpecialJumps;
        input_whaleRightGuess.value = config.WhaleApearingGuests;
        input_diffSpecies.value = config.DiffSpeciesEnabled;

        input_obstacleVel.value = config.ObstacleSpeed;
        input_increasedVelFactor.value = config.IncreasedSpeedFactor;

        input_pointPirueta.value = (int)config.RightGuessPoints;
        input_pointsPiruetaVelocidad.value = (int)config.RightGuessPointsVel;
        input_pointsFloatie.value = (int)config.FloatiePoints;
        input_pointsFloatieVelocidad.value = (int)config.FloatiePointsVel;
        input_pointsBallHit.value = (int)config.BallHitPoints;
        input_pointsBallMiss.value = (int)config.BallMissPoints;
        input_pointsBallVelocidad.value = (int)config.BallHitPointsVel;
        input_pointMax.value = (int)config.LevelPoints;
        input_pointWrongGuess.value = (int)config.WrongGuessPoints;
        input_pointChoqueTronco.value = (int)config.HitObstacleTroncoPoints;
        input_pointChoqueBarca.value = (int)config.HitObstacleBarcaPoints;

        input_desbloqueado.value = config.Desbloqueado;


    }

  
}
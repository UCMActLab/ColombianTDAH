using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.STP;

public class UIConfigQuestion : MonoBehaviour
{
    UIDocument _document;
    Button _acceptButton;

    [SerializeField]
    GameObject _map;

    [SerializeField]
    GameObject _dialogs;

    VisualElement _questionsVisualElement;
    VisualElement _userQuestionsVisualElement;

    // ScriptableObject para guardar informacion
    MisionConfigurationData _config = null;

    int levelId;
    string levelInfoPath = "";

    void Awake()
    {
        // Guarda referencias
        _document = GetComponent<UIDocument>();

        if (_document != null)
        {
            _questionsVisualElement = _document.rootVisualElement.Q<VisualElement>("ParadasPreg");
            _userQuestionsVisualElement = _document.rootVisualElement.Q<VisualElement>("ParadasUsuario");

            // Callback boton
            _acceptButton = _document.rootVisualElement.Q("guardarYjugar") as Button;

            if (_acceptButton != null)
                _acceptButton.RegisterCallback<ClickEvent>(OnSaveClick);

        }
    }

    private void Start()
    {
        bool editMode = SceneLoader.Instance.getMode();
        levelId = SceneLoader.Instance.getCurrentLevelId();
        string writeDir = System.IO.Path.Combine(Application.persistentDataPath, "configInfoMC");

        Debug.Log("LEVEL ID " + levelId);
        if (editMode) //el usuario quiere editar el juego
        {
            if (!System.IO.Directory.Exists(writeDir))
            {
                System.IO.Directory.CreateDirectory(writeDir);
            }
            levelInfoPath = System.IO.Path.Combine(writeDir, "DefaultMisionConfigurationData" + levelId + ".json");

            // if (!sceneLoader.getIsDefaultConfig()) SetUIFromJSONFull();

        }
        else //se carga el nivel por default
        {
            levelInfoPath = "DefaultMisionConfigurationData" + levelId;

            Debug.Log("Cargaremos el default");
            _config = Resources.Load<MisionConfigurationData>(levelInfoPath);

            // Los niveles por defecto están desbloqueados, pero se hace la comprobación por si acaso
            if (_config.Desbloqueado)
            {
                ActivateGame();
            }
            else
            {
                Debug.Log("El nivel no está desbloqueado");
            }

        }
    }

    public void Init(List<string> stops, MisionConfigurationData config)
    {
        // Guardo referencia al ScriptableObject
        _config = config;

        // Busca VisualTreeAssets
        VisualTreeAsset uiAsset = Resources.Load<VisualTreeAsset>("Planificacion/UI/QuestionField");
        VisualTreeAsset toggleUIAsset = Resources.Load<VisualTreeAsset>("Planificacion/UI/Toggle");

        for (int i = 0; i < stops.Count; i++)
        {
            // Preguntas TextField
            VisualElement ui = uiAsset.Instantiate();

            TextField textField = (TextField)ui[0];
            textField.name = stops[i] + "TF";
            textField.label = stops[i];
            _questionsVisualElement.Add(textField);


            // Toggles
            VisualElement toggleUI = toggleUIAsset.Instantiate();
            Toggle toggle = (Toggle)toggleUI[0];
            toggle.name = stops[i] + "Toggle";
            toggle.label = stops[i];
            _userQuestionsVisualElement.Add(toggle);
        }
    }

    private void OnDisable()
    {
        _acceptButton.UnregisterCallback<ClickEvent>(OnSaveClick);
    }

    private void OnSaveClick(ClickEvent ce)
    {
        ActivateGame();
    }

    private void ActivateGame()
    {
        // Me desactivo
        gameObject.SetActive(false);

        // Guardo
        SaveData();
        SaveToJson(_config);

        // Empieza nivel activando mapa y dialogos
        _map.SetActive(true);
        _dialogs.SetActive(true);
    }

    // Guarda la informacion en el Scriptable Object
    private void SaveData()
    {
        Dictionary<string, string> q = new Dictionary<string, string>();

        for (int i = 0; i < _config.StopsNames.Count; i++)
        {
            TextField textF = _questionsVisualElement.Q<TextField>((_config.StopsNames[i] + "TF"));
            q.Add(_config.StopsNames[i], textF.value);
        }

        _config.Questions = q;

        MisionLevelManager.Instance.LoadQuestions(q);
    }

    private void SaveToJson(MisionConfigurationData config)
    {
        string info = JsonUtility.ToJson(config, true);

        Debug.Log("Saving level config at " + levelInfoPath);

        System.IO.FileStream fs = new System.IO.FileStream(levelInfoPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
        file.WriteLine(info);
        file.Close();
        fs.Close();
    }
}

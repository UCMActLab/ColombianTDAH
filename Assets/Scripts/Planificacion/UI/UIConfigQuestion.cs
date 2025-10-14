using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.STP;

public class UIConfigQuestion : MonoBehaviour
{
    UIDocument _document;
    Button _acceptButton;
    Toggle _unlock;

    VisualElement _questionsVisualElement;
    VisualElement _userQuestionsVisualElement;

    // ScriptableObject para guardar informacion
    MisionConfigurationData _config = null;

    void Awake()
    {
        // Guarda referencias
        _document = GetComponent<UIDocument>();

        if (_document != null)
        {
            _questionsVisualElement = _document.rootVisualElement.Q<VisualElement>("ParadasPreg");
            _userQuestionsVisualElement = _document.rootVisualElement.Q<VisualElement>("ParadasUsuario");
            _unlock = _document.rootVisualElement.Q<Toggle>("Desbloqueado");

            // Callback boton
            _acceptButton = _document.rootVisualElement.Q("guardarYjugar") as Button;

            if (_acceptButton != null)
                _acceptButton.RegisterCallback<ClickEvent>(OnSaveClick);

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

        SetUIFromJSON();
    }

    private void OnDisable()
    {
        _acceptButton.UnregisterCallback<ClickEvent>(OnSaveClick);
    }

    private void OnSaveClick(ClickEvent ce)
    {
        // Guardo
        SaveData();
        SaveToJson(_config);

        // Activa juego
        MisionLevelManager.Instance.ActivateGame();
        gameObject.SetActive(false);
    }

    // Guarda la informacion en el Scriptable Object
    private void SaveData()
    {
        // Desbloqueo
        _config.Desbloqueado = _unlock.value;

        // Preguntas
        Dictionary<string, string> q = new Dictionary<string, string>();
        _config.DistractionStops.Clear();
        _config.SelectableStops.Clear();

        for (int i = 0; i < _config.StopsNames.Count; i++)
        {
            // Preguntas texto
            TextField textF = _questionsVisualElement.Q<TextField>((_config.StopsNames[i] + "TF"));
            q.Add(_config.StopsNames[i], textF.value);

            // Preguntas distractoras
            Toggle toggle = _userQuestionsVisualElement.Q<Toggle>((_config.StopsNames[i] + "Toggle"));
            if (!toggle.value)
            {
                // Anyado a la lista de paradas de distraer
                _config.DistractionStops.Add(_config.StopsNames[i]);
            }
            else
            {
                _config.SelectableStops.Add(_config.StopsNames[i]);
            }

        }

        _config.Questions.FromDictionary(q);

        MisionLevelManager.Instance.LoadQuestions(q, _config.DistractionStops, _config.SelectableStops);
    }

    private void SaveToJson(MisionConfigurationData config)
    {
        string info = JsonUtility.ToJson(config, true);

        Debug.Log("Saving level config at " + _config.configName);

        System.IO.FileStream fs = new System.IO.FileStream(_config.configName, System.IO.FileMode.Create, System.IO.FileAccess.Write);
        System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
        file.WriteLine(info);
        file.Close();
        fs.Close();
    }


    private void SetUIFromJSON()
    {
        Debug.Log("SetUIFromJSON Questions");

        // desbloqueo
        _unlock.value = _config.Desbloqueado;

        // convertimos a dictionary de verdad para que sea mas comodo
        Dictionary<string, string> questions = _config.Questions.ToDictionary();

        if (_config.StopsNames != null && questions != null)
        {
            foreach (string stop in _config.StopsNames)
            {
                // parada
                TextField textF = _questionsVisualElement.Q<TextField>(stop + "TF");
                if (textF != null && questions.ContainsKey(stop))
                {
                    textF.value = questions[stop];
                }

                // Toggle asociado a la parada
                Toggle toggle = _userQuestionsVisualElement.Q<Toggle>(stop + "Toggle");
                if (toggle != null)
                {
                    //si la parada esta en el selectable stops es que esta a true
                    toggle.value = _config.SelectableStops != null && _config.SelectableStops.Contains(stop);
                }
            }
        }
    }
}

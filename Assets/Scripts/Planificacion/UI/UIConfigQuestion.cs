using System.Collections.Generic;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UIElements;

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
    public void Init(List<string> stops, MisionConfigurationData config)
    {
        // Referencia a scriptable object
        _config = config;

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
        // Me desactivo
        gameObject.SetActive(false);

        // Guardo
        SaveData();

        // Empieza nivel activando mapa y dialogos
        _map.SetActive(true);
        _dialogs.SetActive(true);
    }

    // Guarda la informacion en el Scriptable Object
    private void SaveData()
    {
        Dictionary<string, string> q = new Dictionary<string, string>();

        for(int i = 0; i < _config.StopsNames.Count; i++)
        {
            TextField textF = _document.rootVisualElement.Q<TextField>(_config.StopsNames[i] + "TF");
            q.Add(_config.StopsNames[i], textF.value);
            Debug.Log(textF.value);
        }


        _config.Questions = q;
    }

}

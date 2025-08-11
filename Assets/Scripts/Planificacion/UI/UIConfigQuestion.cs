using System.Collections.Generic;
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
    public void Init(List<string> stops)
    {

        //delfinesColocados = 0;
        //input_toggleGroup.Clear(); //Borramos los carriles
        //int n = input_carrilesN.value;
        //if (n > 6) n = 6;
        //for (int i = 0; i < n; i++)
        //{
        //    VisualTreeAsset uiAsset = Resources.Load<VisualTreeAsset>("Planificacion/UI/carrilToggles");
        //    VisualElement ui = uiAsset.Instantiate();

        //    for (int j = 0; j < ui.childCount; j++)
        //    {
        //        ui[j].name = "Toggle" + i.ToString() + j.ToString();
        //    }

        //    input_toggleGroup.Add(ui);
        //}
        /////////////////////////////////////////
        // Creo textField necesarios
        //_questionsVisualElement.Clear();

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

        // Empieza nivel activando mapa y dialogos
        _map.SetActive(true);
        _dialogs.SetActive(true);
    }

}

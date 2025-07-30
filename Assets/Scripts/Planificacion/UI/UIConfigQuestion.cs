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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Guarda referencias
        _document = GetComponent<UIDocument>();

        if (_document != null)
        {
            _questionsVisualElement = _document.rootVisualElement.Q<VisualElement>("ParadasPreg");

            // Callback boton
            _acceptButton = _document.rootVisualElement.Q("guardarYjugar") as Button;

            if (_acceptButton != null)
                _acceptButton.RegisterCallback<ClickEvent>(OnSaveClick);

        }
    }
    public void Init(List<string> stops)
    {       
        // Creo textField necesarios
        //for (int i = 0; i < stops.Count; i++) {
        //    TextField textField = new TextField(stops[i]);
        //    textField.name = stops[i] + "TF";
        //    _questionsVisualElement.Add(textField);
        //}
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

using UnityEngine;
using UnityEngine.UIElements;

public class UIMapData : MonoBehaviour
{
    UIDocument _document;
    Button _acceptButton;
    IntegerField _stopNumber;
    IntegerField _stopMins;
    IntegerField _sleepHours;
    IntegerField _location;

    [SerializeField]
    GameObject _map;

    [SerializeField]
    GameObject _dialogs;

    // Scriptable Object
    [SerializeField]
    MisionConfigurationData _config = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _document = GetComponent<UIDocument>();

        if (_document != null)
        {
            _stopNumber = _document.rootVisualElement.Q<IntegerField>("NumeroParadas");
            _stopMins = _document.rootVisualElement.Q<IntegerField>("TiempoParadas");
            _sleepHours = _document.rootVisualElement.Q<IntegerField>("HorasSueno");
            _location = _document.rootVisualElement.Q<IntegerField>("Ubicacion");

            _acceptButton = _document.rootVisualElement.Q("guardarYjugar") as Button;

            if (_acceptButton != null)
                _acceptButton.RegisterCallback<ClickEvent>(OnAcceptClick);
        }


        if (_map != null)
            _map.SetActive(false);

        if (_dialogs != null)
            _dialogs.SetActive(false);
    }

    private void OnDisable()
    {
        _acceptButton.UnregisterCallback<ClickEvent>(OnAcceptClick);
    }

    private void OnAcceptClick(ClickEvent ce)
    {
        SaveConfiguration();
        _document.enabled = false;
        _map.SetActive(true);
        _dialogs.SetActive(true);
    }

    private void SaveConfiguration()
    {
        // Paradas
        _config.NumStops = _stopNumber.value;
        _config.StopMins = _stopMins.value;

        // Dormir
        _config.SleepHours = _sleepHours.value;

        // Ubicacion
        _config.Location = _location.value;

        // Game Manager
        MisionLevelManager.Instance.LoadConfiguration(_config);
    }
}

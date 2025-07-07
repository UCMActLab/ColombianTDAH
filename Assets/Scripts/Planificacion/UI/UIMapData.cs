using UnityEngine;
using UnityEngine.UIElements;

public class UIMapData : MonoBehaviour
{
    UIDocument _document;
    Button _acceptButton;

    [SerializeField]
    GameObject _map;

    [SerializeField]
    GameObject _dialogs;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _document = GetComponent<UIDocument>();
        if (_document != null)
            _acceptButton = _document.rootVisualElement.Q("AcceptButton") as Button;

        if (_acceptButton != null)
            _acceptButton.RegisterCallback<ClickEvent>(OnAcceptClick);

        if (_map != null)
            _map.SetActive(false);

        if (_dialogs != null)
            _dialogs.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnDisable()
    {
        _acceptButton.UnregisterCallback<ClickEvent>(OnAcceptClick);
    }
    private void OnAcceptClick(ClickEvent ce)
    {
        _document.enabled = false;
        _map.SetActive(true);
        _dialogs.SetActive(true);
    }
}

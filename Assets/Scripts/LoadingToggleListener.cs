using UnityEngine;
using UnityEngine.UI;

public class LoadingToggleListener : MonoBehaviour
{
    private Toggle myToggle;

    void Start()
    {
        myToggle = GetComponent<Toggle>();
        myToggle.onValueChanged.AddListener(OnToggleChanged);

        SceneLoader.Instance.setIsDefaultConfig(false); //lo reeseteamos

    }

    private void OnToggleChanged(bool isOn)
    {
        SceneLoader.Instance.setIsDefaultConfig(isOn);
    }
}
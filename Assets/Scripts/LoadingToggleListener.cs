using UnityEngine;
using UnityEngine.UI;

public class LoadingToggleListener : MonoBehaviour
{
    private Toggle myToggle;

    void Start()
    {
        myToggle = GetComponent<Toggle>();
        myToggle.onValueChanged.AddListener(OnToggleChanged);
    }

    private void OnToggleChanged(bool isOn)
    {
        if (SceneLoader.Instance != null)
        {
            SceneLoader.Instance.setIsDefaultConfig(isOn);
        }
        else
        {
            Debug.LogWarning("wtf no hay sceneloader para LoadingToggleListener.OnToggleChanged.");
        }
    }
}
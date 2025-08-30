using UnityEngine;
using UnityEngine.UI;

public class LoadingListener : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SceneLoader.Instance.setMode(false); //por defecto sea false
        Toggle myToggle = GetComponent<Toggle>();
        myToggle.onValueChanged.AddListener(delegate { SceneLoader.Instance.setMode(myToggle.isOn); });
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

using UnityEngine;
using UnityEngine.UI;

public class LoadingListener : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Toggle myToggle = GetComponent<Toggle>();
        myToggle.onValueChanged.AddListener(delegate { GameObject.Find("SceneLoader").GetComponent<SceneLoader>().setMode(myToggle.isOn); });
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

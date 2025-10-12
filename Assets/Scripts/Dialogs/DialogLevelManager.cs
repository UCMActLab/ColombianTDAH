using System.Collections.Generic;
using UnityEngine;

public class DialogLevelManager : MonoBehaviour
{
    List<GameObject> _dialogs;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _dialogs = new List<GameObject>();

        // Consigo los hijos para tener los dialogos de cada nivel
        foreach (Transform child in transform)
        {
            _dialogs.Add(child.gameObject);
        }

        int level = SceneLoader.Instance.getCurrentLevelId(EventRegister.TipoJuego.MisionColombia) - 1;
        _dialogs[level].SetActive(true);
    }
}

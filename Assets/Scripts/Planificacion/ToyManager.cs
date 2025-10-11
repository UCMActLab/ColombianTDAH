using System.Collections.Generic;
using UnityEngine;

public class ToyManager : MonoBehaviour
{
    List<GameObject> _toys;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _toys = new List<GameObject>();

        // Consigo los hijos para tener las figuras
        foreach (Transform child in transform)
        {
            _toys.Add(child.gameObject);
        }

        // Quito filtro en negro a los conseguidos
        int collec = 1;
        List<bool[]> info = SceneLoader.Instance.GetCollectablesInfo();
        for (int i = 0; i < info.Count; i++)
        {
            if (info[i][collec])
            {
                UnlockToy(i);
            }
        }
    }

    void UnlockToy(int index)
    {
        _toys[index].SetActive(true);
    }
}

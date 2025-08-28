using System.Collections.Generic;
using UnityEngine;

public class ShowResumen : MonoBehaviour
{

    [SerializeField] private GameObject checkListGO;
    private List<Transform> checkListElements = new List<Transform>();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        foreach (Transform child in checkListGO.transform)
        {
            checkListElements.Add(child);

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void showChecklist()
    {

    }
    //que por cada hijo de la checklist haya que ir metiendolos con su info correcta
    void showCheckElement()
    {

    }
}

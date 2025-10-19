using System.Collections.Generic;
using UnityEngine;

public class ExplanationManager : MonoBehaviour
{
    List<GameObject> _explanationList;
    bool _active;
    int _count;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _active = false;
        _count = 0;

        _explanationList = new List<GameObject>();

        // Guardo cada gameobject de las imagenes de explicacion
        foreach (Transform child in transform)
        {
            _explanationList.Add(child.gameObject);
        }

        gameObject.SetActive(false);
    }

    public void StartExplanation()
    {
        gameObject.SetActive(true);
        _active = true;
        EnableImage(0, true);
    }

    void EndExplanation()
    {
        gameObject.SetActive(false);
        _active = false;
        EnableImage(_count, false);
        _count = 0;
    }

    public void Next()
    {
        if (_count < _explanationList.Count -1)
        {
            // Desactivo imagen actual
            EnableImage(_count, false);

            // Aunmento contador
            _count++;

            // Activo siguiente imagen
            EnableImage(_count, true);
        }
        else
        {
            EndExplanation();
        }

    }

    private void EnableImage(int index, bool enable)
    {
        _explanationList[index].SetActive(enable);
    }

    // Update is called once per frame
    void Update()
    {
        if (_active)
        {
            if (Input.GetMouseButtonDown(0))
            {
                Next();
            }
        }
    }
}

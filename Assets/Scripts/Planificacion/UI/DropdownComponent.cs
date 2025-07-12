using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DropdownComponent : MonoBehaviour
{
    TMP_Dropdown _myDropdown;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _myDropdown = GetComponent<TMP_Dropdown>();
        SetStartTime();
    }

    public void SetStartTime()
    {
        string selected = _myDropdown.options[_myDropdown.value].text;
        MisionLevelManager.Instance.SetStartTime(selected);
    }

    public void SetSelectedStops()
    {
        List<string> selectedOpctions = new List<string>();

        double raizValue = Math.Sqrt(_myDropdown.value);
        int optionIndex;
        // Si es un numero sin decimales
        if ((raizValue % 1) == 0) {
            optionIndex = (int)raizValue;
            selectedOpctions.Add(_myDropdown.options[optionIndex].text);
        }
        // Si tiene decimales significa que hay varias opciones seleccionadas
        else
        {
            // tengo que ir haciendo entre 2 hasta encontrar todos los numeros
        }

        MisionLevelManager.Instance.SetSelectedStops(selectedOpctions);
    }
}

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

    // Guarda hora de salida seleccionada
    public void SetStartTime()
    {
        string selected = _myDropdown.options[_myDropdown.value].text;
        MisionLevelManager.Instance.SetStartTime(selected);
    }

    // Guarda paradas seleccionadas
    public void SetSelectedStops()
    {
        List<string> selectedOpctions = new List<string>();

        // Logaritmo base 2
        double logValue = Math.Log(_myDropdown.value, 2);
        int optionIndex;
        // Si es un numero sin decimales
        if ((logValue % 1) == 0)
        {
            optionIndex = (int)logValue;
            selectedOpctions.Add(_myDropdown.options[optionIndex].text);
        }
        // Si tiene decimales significa que hay varias opciones seleccionadas
        else
        {
            // Dropdown seleccion multiple codificado en binario
            string binario = Convert.ToString(_myDropdown.value, 2);

            int binStringSize = binario.Length - 1;
            for (int i = binStringSize; i >= 0; i--)
            {
                // Si es 1 anyado la seleccion correspondiente
                if (binario[i] == '1')
                    selectedOpctions.Add(_myDropdown.options[binStringSize - i].text);
            }
        }

        // Guarda en Game Manager los seleccionados
        MisionLevelManager.Instance.SetSelectedStops(selectedOpctions);
    }
}

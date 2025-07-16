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
        SetSelectedStops();
        SetSelectedSleepHours();
        SetSelectedLocationHours();
    }

    // Guarda hora de salida seleccionada
    public void SetStartTime()
    {
        string selected = _myDropdown.options[_myDropdown.value].text;
        MisionLevelManager.Instance.SetStartTime(selected);
        CheckRules();
    }

    private List<string> GetSelectedOptions()
    {
        List<string> selectedOptions = new List<string>();

        // Logaritmo base 2
        double logValue = Math.Log(_myDropdown.value, 2);
        int optionIndex;
        // Si es un numero sin decimales
        if ((logValue % 1) == 0)
        {
            optionIndex = (int)logValue;
            selectedOptions.Add(_myDropdown.options[optionIndex].text);
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
                    selectedOptions.Add(_myDropdown.options[binStringSize - i].text);
            }
        }

        return selectedOptions;
    }

    // Guarda paradas seleccionadas
    public void SetSelectedStops()
    {
        // Guarda en Game Manager los seleccionados
        MisionLevelManager.Instance.SetSelectedStops(GetSelectedOptions());
        CheckRules();
    }

    // Guarda horas de dormir seleccionadas
    public void SetSelectedSleepHours()
    {
        // Guarda en Game Manager
        MisionLevelManager.Instance.SetSelectedSleepTime(GetSelectedOptions());
        CheckRules();
    }

    // Guarda horas de mandar ubicacion seleccionadas
    public void SetSelectedLocationHours()
    {
        // Guarda en Game Manager
        MisionLevelManager.Instance.SetSelectedLocationHours(GetSelectedOptions());
        CheckRules();
    }

    // Comprueba reglas
    private void CheckRules()
    {
        MisionLevelManager.Instance.CheckRules();
    }
}

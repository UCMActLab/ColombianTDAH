using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.STP;

public class PacienteConfig : MonoBehaviour
{
    [SerializeField] 
    private TMP_InputField pacienteInput;
    [SerializeField] 
    private TMP_InputField terapeutaInput;

    [SerializeField] 
    private GameObject edificios;

    private void Start()
    {
        if (EventRegister.Instance.PacientInfoIsRegistered)
        {
            gameObject.SetActive(false);

        }
        else
        {
            gameObject.SetActive(true);
            edificios.SetActive(false);
        }
    }
    public void OnAceptarClicked()
    {
        string paciente = pacienteInput.text;
        string terapeuta = terapeutaInput.text;

        EventRegister.Instance.AddEventSafe(EventRegister.EventosInfo.PacienteInfo, $"Paciente: {paciente}, Terapeuta: {terapeuta}");
        EventRegister.Instance.PacientInfoIsRegistered = true; //se pone a true el bool de que se ha registrado

        EventRegister.Instance.WriteEnd();

        Debug.Log($"Evento PacienteInfo. Datos guardados: Paciente={paciente}, Terapeuta={terapeuta}");

        //desactivar el canvas actual y ponemos las casas de fondo
        gameObject.SetActive(false);
        edificios.SetActive(true);

    }
}


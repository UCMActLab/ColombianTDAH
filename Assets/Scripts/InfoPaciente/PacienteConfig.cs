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

    EventRegister.InfoSesion infoSesion;

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
        string paciente = string.IsNullOrWhiteSpace(pacienteInput.text) ? "TEMP" : pacienteInput.text;
        string terapeuta = string.IsNullOrWhiteSpace(terapeutaInput.text) ? "TEMP" : terapeutaInput.text;

        infoSesion = new EventRegister.InfoSesion(paciente, terapeuta, "TEMP");
        //infoSesion.nombreTerapeuta = terapeuta;
        //infoSesion.nombrePaciente = paciente;
        //infoSesion.nombreJuego = "TEMP";
        //infoSesion.fechaHora = DateTime.UtcNow;

        EventRegister.Instance.SetInfoSesion(infoSesion);

        EventRegister.Instance.PacientInfoIsRegistered = true; //se pone a true el bool de que se ha registrado

        //EventRegister.Instance.AddEventSafe(EventRegister.EventosInfo.PacienteInfo, $"Paciente: {paciente}, Terapeuta: {terapeuta}");

        //EventRegister.Instance.WriteEnd();

        Debug.Log($"Evento PacienteInfo. Datos guardados: Paciente={paciente}, Terapeuta={terapeuta}");

        //desactivar el canvas actual y ponemos las casas de fondo
        gameObject.SetActive(false);
        edificios.SetActive(true);

    }
}


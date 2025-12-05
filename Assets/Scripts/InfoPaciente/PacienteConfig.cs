using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.STP;

public class PacienteConfig : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField nombrePacienteInput;
    [SerializeField] 
    private TMP_InputField idPacienteInput;
    [SerializeField] 
    private TMP_InputField sesionInput;

    [SerializeField] 
    private GameObject edificios;

    [SerializeField]
    private GameObject ui;

    EventRegister.InfoSesion infoSesion;

    private void Start()
    {
        if (EventRegister.Instance.PacientInfoIsRegistered)
        {
            gameObject.SetActive(false);

        }
        else
        {
            edificios.SetActive(false);
        }
    }

    private void OnEnable()
    {
        gameObject.SetActive(true);
        edificios.SetActive(false);

        if (EventRegister.Instance != null) { //a veces puede ser nulo si es la primera vez que se enablea y es antes de crearse el event register
            EventRegister.InfoSesion currentInfo = EventRegister.Instance.GetInfoSesion();

            // si ya hay datos guardados, ponerlos en los inputs
            if (!string.IsNullOrWhiteSpace(currentInfo.nombrePaciente))
            {
                nombrePacienteInput.text = currentInfo.nombrePaciente;
            }
            if (!string.IsNullOrWhiteSpace(currentInfo.idPaciente))
            {
                idPacienteInput.text = currentInfo.idPaciente;
            }

            if (!string.IsNullOrWhiteSpace(currentInfo.numeroSesion))
            {
                sesionInput.text = currentInfo.numeroSesion;
            }
        }
       
    }
    private void OnDisable()
    {

        if (edificios != null)
        {
            edificios.SetActive(true);
        }

        if (ui != null)
        {
            ui.SetActive(true);
        }
        gameObject.SetActive(false);

    }

    public void OnAceptarClicked()
    {
        string nombrePaciente = string.IsNullOrWhiteSpace(nombrePacienteInput.text) ? "TEMP" : nombrePacienteInput.text;
        string idPaciente = string.IsNullOrWhiteSpace(idPacienteInput.text) ? "TEMP" : idPacienteInput.text;
        string numSesion = string.IsNullOrWhiteSpace(sesionInput.text) ? "TEMP" : sesionInput.text;

        //si cambia el numero de la sesion o el paciente que termine el json para empezar otro al meterse en un juego
        if (EventRegister.Instance.GetInfoSesion().nombrePaciente != nombrePaciente || EventRegister.Instance.GetInfoSesion().idPaciente != idPaciente || EventRegister.Instance.GetInfoSesion().numeroSesion != numSesion)
        {
            EventRegister.Instance.WriteEnd();

        }
        //todavia no le ponemos el juego porque no ha entrado a ninguno, se pondra al
        infoSesion = new EventRegister.InfoSesion(nombrePaciente, idPaciente, numSesion, EventRegister.TipoJuego.DefaultGame); 

        EventRegister.Instance.SetInfoSesion(infoSesion);

        Debug.Log($"Evento PacienteInfo. Datos guardados: Paciente={nombrePaciente}, ID={idPaciente}, numsesion={numSesion}");

        //desactivar el canvas actual y ponemos las casas de fondo
        edificios.SetActive(true);
        gameObject.SetActive(false);


    }

}


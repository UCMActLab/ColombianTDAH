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


        EventRegister.InfoSesion currentInfo = EventRegister.Instance.GetInfoSesion();

        // si ya hay datos guardados, ponerlos en los inputs
        if (!string.IsNullOrWhiteSpace(currentInfo.idPaciente))
        {
            pacienteInput.text = currentInfo.idPaciente;
        }

        if (!string.IsNullOrWhiteSpace(currentInfo.numeroSesion))
        {
            sesionInput.text = currentInfo.numeroSesion;
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
        string paciente = string.IsNullOrWhiteSpace(pacienteInput.text) ? "TEMP" : pacienteInput.text;
        string numSesion = string.IsNullOrWhiteSpace(sesionInput.text) ? "TEMP" : sesionInput.text;

        //todavia no le ponemos el juego porque no ha entrado a ninguno, se pondra al
        infoSesion = new EventRegister.InfoSesion(paciente, numSesion, EventRegister.TipoJuego.DefaultGame); 

        EventRegister.Instance.SetInfoSesion(infoSesion);

        Debug.Log($"Evento PacienteInfo. Datos guardados: Paciente={paciente}, numsesion={numSesion}");

        //desactivar el canvas actual y ponemos las casas de fondo
        edificios.SetActive(true);
        gameObject.SetActive(false);


    }

}


using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TurnoBotonUI : MonoBehaviour // Clase para el comportamiento de los botones del menu de seleccion de nivel del juego de la cocina
{
    [Header("Informacion del nivel")]
    public Turno tipoTurno;
    private TurnoEstado tipoTurnoEstado;
    public int jornada;

    private Image iconoTurno;
    private Image fondo;

    private Color colorActivo = new Color32(180, 180, 180, 255);
    private Color colorCompletado = new Color32(178, 242, 187, 255);
    private Color colorBloqueado = new Color32(0, 0, 0, 255);

    private Sprite spriteSol;
    private Sprite spriteTarde;
    private Sprite spriteLuna;
    private Sprite spriteCandado;

    private ButtonAnimation botonAnim;

    private Button boton;

    void Start()
    {
        iconoTurno = transform.Find("Icono").GetComponent<Image>();
        fondo = GetComponent<Image>();
        boton = GetComponent<Button>();
        botonAnim = GetComponent<ButtonAnimation>();
        botonAnim.OnAnimationEnd += AccionDespuesDeAnimacion;

        spriteSol = Resources.Load<Sprite>("kitchen/sol");
        spriteTarde = Resources.Load<Sprite>("kitchen/tarde");
        spriteLuna = Resources.Load<Sprite>("kitchen/luna");
        spriteCandado = Resources.Load<Sprite>("kitchen/candado");

        Configurar();
    }

    public void Configurar()
    {
        switch (tipoTurno)
        {
            case Turno.Manana: iconoTurno.sprite = spriteSol; break;
            case Turno.Tarde: iconoTurno.sprite = spriteTarde; break;
            case Turno.Noche: iconoTurno.sprite = spriteLuna; break;
        }

        if (jornada < LevelKitchenManager.Instance.GetJornadaMaxDesbloqueada())
        {
            tipoTurnoEstado = TurnoEstado.Completado;
        }
        else if (jornada == LevelKitchenManager.Instance.GetJornadaMaxDesbloqueada())
        {
            if (tipoTurno < LevelKitchenManager.Instance.GetTurnoMaxDesbloqueado()) tipoTurnoEstado = TurnoEstado.Completado;
            else if (tipoTurno == LevelKitchenManager.Instance.GetTurnoMaxDesbloqueado()) tipoTurnoEstado = TurnoEstado.Activo;
            else if (tipoTurno > LevelKitchenManager.Instance.GetTurnoMaxDesbloqueado()) tipoTurnoEstado = TurnoEstado.Bloqueado;
        }
        else
        {
            tipoTurnoEstado = TurnoEstado.Bloqueado;
        }

        switch (tipoTurnoEstado)
        {
            case TurnoEstado.Bloqueado:
                iconoTurno.sprite = spriteCandado;
                fondo.color = colorBloqueado;
                boton.interactable = false;
                break;

            case TurnoEstado.Activo:
                fondo.color = colorActivo;
                boton.interactable = true;
                break;

            case TurnoEstado.Completado:
                fondo.color = colorCompletado;
                boton.interactable = true;
                break;
        }
    }

    void AccionDespuesDeAnimacion()
    {
        LevelKitchenManager.Instance.SetJornada(jornada);
        LevelKitchenManager.Instance.SetTurno(tipoTurno);

        int baseSegundos = LevelKitchenManager.Instance.GetNivelacionData().jornadas[jornada].tiempoBaseManual;
        float dificultad = LevelKitchenManager.Instance.GetNivelacionData().jornadas[jornada].margenDeError;

        float dificultadporturno = 1.0f;

        switch (tipoTurno)
        {
            case Turno.Tarde:
                dificultadporturno = 0.9f;
                break;
            case Turno.Noche:
                dificultadporturno = 0.8f;
                break;
        }
            
        LevelKitchenManager.Instance.SetTiempoPorTurno(Mathf.CeilToInt(baseSegundos * dificultad * dificultadporturno));

        string s = "Empieza la jornada " + jornada.ToString("00") + " en el turno de " + LevelKitchenManager.Instance.GetTurno().ToString();
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.KitchenInicioTurno, s));
        EventRegister.Instance.EvntToJson();

        SceneLoader.LoadScene("KitchenLevel");
    }
}

using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TurnoBotonUI : MonoBehaviour // Clase para el comportamiento de los botones del menu de seleccion de nivel del juego de la cocina
{
    [Header("Informacion del nivel")]
    public Turno tipoTurno;
    public TurnoEstado tipoTurnoEstado;
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

    private Button boton;

    private bool animando = false;

    void Start()
    {
        iconoTurno = transform.Find("Icono").GetComponent<Image>();
        fondo = GetComponent<Image>();
        boton = GetComponent<Button>();
        boton.onClick.AddListener(OnClickAnimacion);

        spriteSol = Resources.Load<Sprite>("kitchen/sol");
        spriteTarde = Resources.Load<Sprite>("kitchen/tarde");
        spriteLuna = Resources.Load<Sprite>("kitchen/luna");
        spriteCandado = Resources.Load<Sprite>("kitchen/candado");

        Configurar(TurnoEstado.Activo);
    }

    public void Configurar(TurnoEstado estado)
    {
        switch (tipoTurno)
        {
            case Turno.Manana: iconoTurno.sprite = spriteSol; break;
            case Turno.Tarde: iconoTurno.sprite = spriteTarde; break;
            case Turno.Noche: iconoTurno.sprite = spriteLuna; break;
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

    public void OnClickAnimacion()
    {
        if (animando) return;
        StartCoroutine(EscalarIcono());
    }

    IEnumerator EscalarIcono()
    {
        animando = true;

        float duracion = 0.1f;
        float tiempo = 0f;
        Vector3 original = transform.localScale;
        Vector3 objetivo = original * 1.3f;

        // Escalar hacia arriba
        while (tiempo < duracion)
        {
            transform.localScale = Vector3.Lerp(original, objetivo, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.localScale = objetivo;

        // Regreso
        tiempo = 0f;
        while (tiempo < duracion)
        {
            transform.localScale = Vector3.Lerp(objetivo, original, tiempo / duracion);
            tiempo += Time.deltaTime;
            yield return null;
        }

        transform.localScale = original;
        
        animando = false;

        LevelKitchenManager.Instance.SetJornada(jornada);
        SceneManager.LoadScene("KitchenLevel");
    }

}

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

public class BookGestor : MonoBehaviour
{
    private List<string> recetasTurno;

    [SerializeField] private FadeCanvas myCanvas;
    [SerializeField] private FadeCanvas ingredientsCanvas;
    [SerializeField] private Image recetaImage;


    [SerializeField] private ButtonAnimation leftButton;
    [SerializeField] private ButtonAnimation rightButton;
    [SerializeField] private ButtonAnimation seguirButton;
    [SerializeField] private ButtonAnimation ingredientsButton;
    [SerializeField] private ButtonAnimation volverButton;

    private int indiceActual = 0;

    [SerializeField] private Transform initialTransform;
    private Transform bookTargetTransform;
    private float moveDuration = 1.5f;
    [SerializeField] private FadeLight[] lights;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Draggable input = GetComponent<Draggable>();

        if (input != null)
        {
            input.onStartDragging.RemoveListener(OnBookClicked);
            input.onStartDragging.AddListener(OnBookClicked);
        }
        else
        {
            Debug.LogWarning("No se encontró el componente Draggable en libro.");
        }

        // Inicializa la lista
        recetasTurno = LevelKitchenManager.Instance
            .GetRecetasRestantes()             // Devuelve Dictionary<Receta, algo>
            .Select(par => par.Key.nombre)     // Saca solo el nombre (string)
            .OrderBy(nombre => nombre)         // Ordena alfabéticamente
            .ToList();                         // Convierte a lista

        foreach (var receta in recetasTurno)
        {
            Debug.Log(receta);
        }

        ActualizarImagen();

        // Suscribir botones
        leftButton.OnAnimationEnd += MostrarAnterior;
        rightButton.OnAnimationEnd += MostrarSiguiente;
        seguirButton.OnAnimationEnd += Salir;
        ingredientsButton.OnAnimationEnd += MostrarIngredientes;
        volverButton.OnAnimationEnd += Volver;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void MostrarAnterior()
    {
        Debug.Log("Left");
        indiceActual--;
        if (indiceActual < 0) indiceActual = recetasTurno.Count - 1; // Loop al final
        ActualizarImagen();
    }

    void MostrarSiguiente()
    {
        Debug.Log("Right");
        indiceActual++;
        if (indiceActual >= recetasTurno.Count) indiceActual = 0; // Loop al inicio
        ActualizarImagen();
    }

    void ActualizarImagen()
    {
        recetaImage.sprite = LevelKitchenManager.Instance.GetRecetasSprites()[recetasTurno[indiceActual]].spriteLibro;
    }

    void MostrarIngredientes()
    {
        Debug.Log("Lista de ingredientes");
        myCanvas.FadeOut();
        ingredientsCanvas.FadeIn();
    }

    void Volver()
    {
        
        ingredientsCanvas.FadeOut();
        myCanvas.FadeIn();
    }

    void Salir()
    {
        Debug.Log("Libro clickado para salir");
        
        AnimatorManager.Instance.ChangeAnimation(ObjetosAnim.Libro, "Close");

        foreach (FadeLight l in lights)
        {
            l.FadeOut();
        }

        myCanvas.FadeOut();

        LevelKitchenManager.Instance?.NotifyBookClosed(); // Para el tutorial

        // Iniciar el movimiento con rotación
        StartCoroutine(MoverLibro(transform, initialTransform.position, initialTransform.rotation, moveDuration, () =>
        {
            GetComponent<Draggable>().enabled = true;
        }));
    }


    private void OnBookClicked()
    {
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.KitchenConsultarLibro, ""));
        EventRegister.Instance.EvntToJson();
        GetComponent<Draggable>().enabled = false;

        LevelKitchenManager.Instance.SetNOpenedBook(LevelKitchenManager.Instance.GetNOpenedBook() + 1);

        bookTargetTransform = GameObject.Find("LibroPos").transform;
        AnimatorManager.Instance.PlayAndPauseAt(ObjetosAnim.Libro, "Open", 0.8f);

        // Iniciar el movimiento con rotación
        StartCoroutine(MoverLibro(transform, bookTargetTransform.position, bookTargetTransform.rotation, moveDuration, () =>
        {
            foreach (FadeLight l in lights)
            {
                l.FadeIn();
            }

            myCanvas.FadeIn();
        }));
        LevelKitchenManager.Instance.NotifyBookOpened();
    }

    private IEnumerator MoverLibro(Transform objeto, Vector3 destinoPos, Quaternion destinoRot, float duracion, Action onComplete = null)
    {
        Vector3 origenPos = objeto.position;
        Quaternion origenRot = objeto.rotation;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            float t = tiempo / duracion;

            // Easing SmoothStep (ease-in/ease-out)
            float e = t * t * (3f - 2f * t);

            objeto.position = Vector3.LerpUnclamped(origenPos, destinoPos, e);
            objeto.rotation = Quaternion.SlerpUnclamped(origenRot, destinoRot, e);

            tiempo += Time.deltaTime;
            yield return null;
        }

        // Asegurar posición/rotación final
        objeto.position = destinoPos;
        objeto.rotation = destinoRot;

        onComplete?.Invoke();
    }
}

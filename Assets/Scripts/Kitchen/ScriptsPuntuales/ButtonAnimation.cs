using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonAnimation : MonoBehaviour
{
    private Button boton;
    private bool animando = false;

    // Evento público que se ejecuta al finalizar la animación
    public event Action OnAnimationEnd;

    void Start()
    {
        boton = GetComponent<Button>();
        boton.onClick.AddListener(OnClickAnimacion);
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

        // Dispara el evento al terminar
        OnAnimationEnd?.Invoke();
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CalculateStats : MonoBehaviour
{
    [SerializeField] private FadeCanvas canvas;
    [SerializeField] private ButtonAnimation button;
    [SerializeField] private TMP_Text recetasCompletadas;
    [SerializeField] private TMP_Text tiempoTranscurrido;
    [SerializeField] private TMP_Text tiempoRestante;
    [SerializeField] private TMP_Text consultasLibro;

    public void Calculate(int recetas, int recetasTotales, int tiempoTotal, int tiempoRest, int consultas)
    {
        recetasCompletadas.text = recetas.ToString() + " / " + recetasTotales.ToString();

        int tiempoTrans = tiempoTotal - tiempoRest;
        int m1 = tiempoTrans / 60;
        int s1 = tiempoTrans % 60;
        tiempoTranscurrido.text = $"{m1:00}:{s1:00}";

        int m2 = tiempoRest / 60;
        int s2 = tiempoRest % 60;
        tiempoRestante.text = $"{m2:00}:{s2:00}";

        consultasLibro.text = consultas.ToString();

        canvas.FadeIn();
        canvas.gameObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
        canvas.gameObject.GetComponent<CanvasGroup>().interactable = true;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        button.OnAnimationEnd += Continuar;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void Continuar()
    {
        SceneManager.LoadScene("KitchenLevelSelector");
    }
}

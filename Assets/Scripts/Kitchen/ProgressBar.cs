using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

public class ProgressBar : MonoBehaviour
{
    [Header("Refs")]
    public Image fill;                 // Image con Fill Method = Horizontal o Radial
    public CanvasGroup cg;             // para fade
    [Header("Estética")]
    public Color colorBar = new Color(0.50f, 0.79f, 0.52f);
    public float fadeSpeed = 10f;

    private float timeStarted = 0;
    private float barTime = 1;

    bool visible;

    void Start()
    {
        if (fill)
        {
            fill.color = colorBar;    
            fill.fillAmount = 1f;
        }
    }

    public void HandleStart(float barTotalTime)
    {
        SetVisible(true);
        timeStarted = 0;
        barTime = barTotalTime;
    }
        
    public void HandleProgress(float t) {
        if (fill)
        {
            fill.fillAmount = Mathf.Clamp01(t);
        }
    }
    public void HandleEnd()
    {
        if (fill) fill.fillAmount = 1f;
        SetVisible(false);
    }

    void Update()
    {
        if (visible) {
            timeStarted += Time.deltaTime;
            HandleProgress(timeStarted / barTime);
        }
        if (cg)
        {
            float target = visible ? 1f : 0f;
            cg.alpha = Mathf.MoveTowards(cg.alpha, target, fadeSpeed * Time.deltaTime);
        }
    }

    void SetVisible(bool v, bool instant = false)
    {
        visible = v;
        if (instant && cg) cg.alpha = v ? 1f : 0f;
        gameObject.SetActive(v || (cg && cg.alpha > 0.01f)); // evita pop
    }
}

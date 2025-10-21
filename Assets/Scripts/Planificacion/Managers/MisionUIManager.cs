using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MisionUIManager : MonoBehaviour
{
    [SerializeField]
    GameObject _decisionGO;

    [SerializeField]
    TextMeshProUGUI _questionText;

    [SerializeField]
    Slider _timeSlider;

    [SerializeField]
    TextMeshProUGUI _time;

    [SerializeField]
    PlanificationList _planificationListComp;

    [SerializeField]
    Image _sleepImage;

    [SerializeField]
    TextMeshProUGUI _sleepText;

    [SerializeField]
    Image _sleepVignetteImage;

    [SerializeField]
    Image _topLid;
    [SerializeField]
    Image _bottomLid;

    [SerializeField]
    TextMeshProUGUI prefabTextLocation; //el mensajito que sale cuando tocas el boton de enviar ubicacion

    [SerializeField]
    Button locationButton; //lo queremos en principio solo por su posicion

    // Corutinas
    private Coroutine _vignetteFadeCoroutine;
    private Coroutine _blinkRoutine;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _decisionGO.SetActive(false);

        MisionLevelManager.Instance.RegisterUIManager(this);
        float answerTime = MisionLevelManager.Instance.GetAnswerTime();

        InitSlider(0, answerTime);
    }

    // Inicializa valores del slider
    void InitSlider(float min, float max)
    {
        _timeSlider.maxValue = max;
        _timeSlider.minValue = min;
        _timeSlider.value = max;
    }

    // Activa botones de respuesta
    public void ShowDecisionButtons()
    {
        _decisionGO.SetActive(true);
    }

    // Desactiva botones de respuesta
    public void HideDecisionButtons()
    {
        _decisionGO.SetActive(false);
    }

    // Respuesta si
    public void YesClicked()
    {
        Clicked(true);
    }

    // Respuesta no
    public void NoClicked()
    {
        Clicked(false);
    }

    // Desactiva pregunta y botones de respuesta
    void Clicked(bool yes)
    {
        string mensaje = "Pregunta final, se ha contestado";
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.MCTerminaDecision, mensaje));
        EventRegister.Instance.EvntToJson();

        _decisionGO.SetActive(false);
        MisionLevelManager.Instance.Answered(yes);
    }

    // Actualiza el valor del slider
    public void UpdateSlider(float value)
    {
        _timeSlider.value = value;
    }

    public void ChangeQuestion(string qText)
    {
        _questionText.text = qText;
    }

    // Cambia tiempo de la hora
    public void ChangeTime(HourMinSec newTime)
    {
        _time.text = newTime.GetHMString();
    }

    public void SetStartTime(HourMinSec newTime)
    {
        _planificationListComp.SetDepartureTime(newTime.GetHString());

        ChangeTime(newTime);
    }

    public void SetStops(List<string> stops)
    {
        _planificationListComp.SetStops(stops);
    }

    public void SetSleepHours(List<string> sleepHours)
    {
        _planificationListComp.SetSleepHours(sleepHours);
    }

    public void SetLocationHours(List<string> locationHours)
    {
        _planificationListComp.SetLocationHours(locationHours);
    }

    public void ExitLevel()
    {
        MisionLevelManager.Instance.ExitLevel();
    }

    private void SetAlpha(Graphic graphic, float alpha)
    {
        if (graphic == null) return;

        Color color = graphic.color;
        color.a = alpha;
        graphic.color = color;
    }

    public void SetSleepImageAlpha(float alpha)
    {
        SetAlpha(_sleepImage, alpha);
        SetAlpha(_sleepText, alpha);
    }

    public void SetSleepVignetteAlpha(float alpha)
    {
        SetAlpha(_sleepVignetteImage, alpha);
    }
    public float GetSleepVignetteAlpha()
    {
        return _sleepVignetteImage.color.a;

    }

    public void SetStopTick(int index, bool enabled)
    {
        _planificationListComp.SetStopTick(index, enabled);
    }

    public void SetSleepTick(int index, bool enabled)
    {
        _planificationListComp.SetSleepTick(index, enabled);
    }

    public void SetLocationTick(int index, bool enabled)
    {
        _planificationListComp.SetLocationTick(index, enabled);
    }

    public void SendLocation()
    {
        MisionLevelManager.Instance.SendLocation();
    }


    private IEnumerator FadeVignette(float targetAlpha)
    {
        float startAlpha = GetSleepVignetteAlpha();
        float durationCurrent = 0f;

        float duration = 1.5f;
        while (durationCurrent < duration)
        {
            durationCurrent += Time.deltaTime;
            float t = durationCurrent / duration;

            // interpolar con el lerpp
            float currentAlpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            SetSleepVignetteAlpha(currentAlpha);

            yield return null; //esto espera al siguiente frame antes de seguir, mejor que con seconds me parece
        }

        SetSleepVignetteAlpha(targetAlpha);
        _vignetteFadeCoroutine = null;
    }

    public void SetSleepVignette(float alpha)
    {
        alpha += 0.25f; //intensificar que si no no se ve mucho al principio
        if (alpha >= 1.0f) {
            alpha = 1.0f;
            Blink();
        
        }
        // _misionUIManager.SetSleepVignetteAlpha(alpha);
        if (_vignetteFadeCoroutine != null) // si ya existe una la paramos, solo deberia ocurrir una simultanea
            StopCoroutine(_vignetteFadeCoroutine);

        StartCoroutine(FadeVignette(alpha));
    }

    public void Blink()
    {
      

        if (_blinkRoutine == null) //solo empieza a hacer blink si no hay uno ocurriendo ya
            _blinkRoutine = StartCoroutine(BlinkRoutine());

    }

    private IEnumerator BlinkRoutine()
    {

        //okay la pantalla va a medir 2000x1200 y basamos todo en eso
        //como cambies la resolucion no va lol
        float screenHeight = 1200f;
        float lidHeight = 600f;
        float height = screenHeight/2 + lidHeight/2;

        Vector2 _topOpenPos = new Vector2(0, height);
        Vector2 _topClosedPos = new Vector2(0, height-lidHeight);

        Vector2 _bottomOpenPos = new Vector2(0, -height);
        Vector2 _bottomClosedPos = new Vector2(0, -height + lidHeight);

        // usamos anchored position porque me resulta mas comodo
        _topLid.rectTransform.anchoredPosition = _topOpenPos;
        _bottomLid.rectTransform.anchoredPosition = _bottomOpenPos;



        float duration = 0.8f;
        float currentDuration = 0f;
        // cerrar los ojos
        while (currentDuration < duration)
        {
            currentDuration += Time.deltaTime;
            float t = currentDuration / duration;

            _topLid.rectTransform.anchoredPosition = Vector2.Lerp(_topOpenPos, _topClosedPos, t);
            _bottomLid.rectTransform.anchoredPosition = Vector2.Lerp(_bottomOpenPos, _bottomClosedPos, t);

            yield return null; //se espera un frame antes de seguir
        }

        // abrir
        currentDuration = 0f;
        while (currentDuration < duration)
        {
            currentDuration += Time.deltaTime;
            float t = currentDuration / duration;

            _topLid.rectTransform.anchoredPosition = Vector2.Lerp(_topClosedPos, _topOpenPos, t);
            _bottomLid.rectTransform.anchoredPosition = Vector2.Lerp(_bottomClosedPos, _bottomOpenPos, t);

            yield return null;
        }

        _blinkRoutine = null;

    }
    public void ShowSentLocation()
    {

        Instantiate(prefabTextLocation, locationButton.transform);
    }

    private void Update() 
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Blink();
            Debug.Log("blink");
        }
    }

}
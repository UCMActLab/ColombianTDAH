using System.Collections;
using TMPro;
using UnityEngine;

public class ShowDialog : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private GameObject _endTextPointer;
    [SerializeField] private Animator _talkingObjectAnimator = null;

    private bool _textEnded;

    DialogSettings _settings;

    int _currentText;

    private const string HTML_ALPHA = "<color=#00000000>";

    public void SetSettings(DialogSettings settings) { this._settings = settings; }

    private void Start()
    {
        ShowText();
    }

    private void OnDestroy()
    {
        StopCoroutine("AnimText");
    }

    public void Update()
    {
        if (_textEnded && Input.GetMouseButtonUp(0))
        {
            ShowText();
        }
    }

    private void StartDialog()
    {
        _settings.onStartDialog.Invoke();   
    }

    private void EndDialog()
    {
        _settings.onFinishDialog.Invoke();
        Destroy(gameObject);
    }

    public void ShowText()
    {
        if(_currentText == 0)
        {
            StartDialog();
        }
        if(_currentText >= _settings.texts.Count)
        {
            EndDialog();
        }
        else
        {
            int initialIndex = 0;
            StartCoroutine(AnimText(initialIndex, _settings.texts[_currentText++]));
        }
    }

    IEnumerator AnimText(int initialIndex, string messageToShow)
    {
        _textEnded = false;
        _endTextPointer.SetActive(_textEnded);

        if (_talkingObjectAnimator != null) _talkingObjectAnimator.SetBool("isTalking", true);

        int alphaIndex = 0;
        string displayText = "";

        foreach (char c in messageToShow.ToCharArray())
        {
            alphaIndex++;
            _text.text = messageToShow;

            displayText = _text.text.Insert(alphaIndex, HTML_ALPHA);
            _text.text = displayText;
            
            yield return new WaitForSeconds(_settings.speed);
        }
        
        if (_talkingObjectAnimator != null) _talkingObjectAnimator.SetBool("isTalking", false);

        _textEnded = true;
        _endTextPointer.SetActive(_textEnded);
    }
}
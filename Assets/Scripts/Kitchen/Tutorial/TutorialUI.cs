using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TextMeshProUGUI txt;
    [SerializeField] private Button clickCatcher; // Botón para cambiar texto

    private Queue<string> lines = new();

    public void ShowLines(IEnumerable<string> msgs)
    {
        panel.SetActive(true);
        lines.Clear();
        foreach (var m in msgs) lines.Enqueue(m);
        Next();
    }

    public void Next()
    {
        if (lines.Count == 0) { panel.SetActive(false); return; }
        txt.text = lines.Dequeue();
    }

    void Awake()
    {
        if (clickCatcher != null)
            clickCatcher.onClick.AddListener(Next);
    }
}

using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
[CreateAssetMenu]
public class DialogSettings : ScriptableObject
{
    public List<string> texts;

    public float speed;

    [Header("Events")]
    public UnityEvent onStartDialog;
    public UnityEvent onFinishDialog;
}
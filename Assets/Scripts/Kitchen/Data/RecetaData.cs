using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecetaData", menuName = "Scriptable Objects/RecetaData")]
public class RecetaData : ScriptableObject
{
    public string nombre;
    public string categoria;
    public List<string> ingredientes = new();
    public List<string> puestos = new();
}

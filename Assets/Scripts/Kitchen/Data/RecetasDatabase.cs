using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecetasDatabase", menuName = "Scriptable Objects/RecetasDatabase")]
public class RecetasDatabase : ScriptableObject
{
    public List<RecetaData> recetas = new();
    public float factorDeTiempo;
}

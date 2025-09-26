using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NuevoNivel", menuName = "Scriptable Objects/NivelacionData")]
public class NivelacionData : ScriptableObject
{
    public List<JornadaData> jornadas = new();
}

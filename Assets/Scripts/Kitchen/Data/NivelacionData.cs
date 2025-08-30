using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NuevoNivel", menuName = "Scriptable Objects/NivelacionData")]
public class NivelacionData : ScriptableObject
{
    public string nombre_terapeuta;
    public string nombre_paciente;
    public List<JornadaData> jornadas = new();
}

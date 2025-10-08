using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NuevoNivel", menuName = "Scriptable Objects/NivelacionData")]
public class NivelacionData : ScriptableObject
{
    public int jornadaDesbloqueada = 0;
    public Turno turnoDesbloqueado = Turno.Mañana;
    public List<JornadaData> jornadas = new();
}

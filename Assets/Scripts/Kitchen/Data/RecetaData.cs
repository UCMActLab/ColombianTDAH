using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecetaData", menuName = "Scriptable Objects/RecetaData")]
public class RecetaData : ScriptableObject
{
    public string nombre;
    public string categoria;
    public List<Ingredientes> ingredientes = new();
    public List<PuestosDeTrabajo> puestos = new();
    public int tiempo_est_segs;
}

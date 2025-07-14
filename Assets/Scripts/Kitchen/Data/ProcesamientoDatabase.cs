using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName = "ProcesamientoDatabase", menuName = "Scriptable Objects/ProcesamientoDatabase")]
public class ProcesamientoDatabase : ScriptableObject
{
    public List<ProcesamientoData> processable;

    public ProcesamientoData GetProcesamiento(Ingredientes ingredient, PuestosDeTrabajo workstation)
    {
        return processable.FirstOrDefault(p => p.ingredient == ingredient && p.workstation == workstation);
    }
}

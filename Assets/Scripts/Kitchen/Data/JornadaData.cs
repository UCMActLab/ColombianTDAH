using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class JornadaData
{
    public string nombreJornada = "Jornada X";
    public List<PuestosDeTrabajo> puestosActivos = new();
    [Range(0f, 1f)]
    public float porcentajeVariacion = 1f; // 1 = 100%, esto indica la variacion de recetas dentro de las recetasAsignadasç
    [Range(1f, 3f)]
    public float margenDeError = 1f; // 2 = se multiplica por 2 el tiempo estimado en el que se tiene que cumplir la preparacion de las recetas

    [HideInInspector]
    public List<RecetaData> recetasAsignadas = new();
}

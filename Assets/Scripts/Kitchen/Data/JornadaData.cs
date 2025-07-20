using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

[System.Serializable]
public class JornadaData
{
    public string nombreJornada = "Jornada X";
    public List<PuestosDeTrabajo> puestosActivos = new();
    [Range(1f, 3f)]
    public float margenDeError = 1f; // 2 = se multiplica por 2 el tiempo estimado en el que se tiene que cumplir la preparacion de las recetas
    public List<RecetaData> recetasAsignadas = new();
}

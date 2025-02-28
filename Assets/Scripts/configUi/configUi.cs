using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Windows;

public class configUi : MonoBehaviour
{
    VisualElement fase1;
    VisualElement fase2;
    VisualElement fase3;

    IntegerField input_carrilesN;
    IntegerField input_delfinesN;
    Button input_fase1Complet;
    IntegerField input_obstacleMin;
    IntegerField input_obstacleMax;
    Toggle input_piruetasSimult;
    IntegerField input_jumpMin;
    IntegerField input_jumpMax;
    IntegerField input_piruetMin;
    IntegerField input_piruetMax;
    FloatField input_obstacleVel;
    IntegerField input_pointMax;
    IntegerField input_pointPirueta;
    IntegerField input_pointWrongGuess;
    IntegerField input_pointChoque;
    Button input_guardar;


    private void OnEnable()
    {
        VisualElement root = GetComponent<UIDocument>().rootVisualElement;

        fase1 = root.Q("Fase1");
        fase2 = root.Q("Fase2");
        fase3 = root.Q("Fase3");

        input_carrilesN = root.Q<IntegerField>("carrilesN");
        input_delfinesN = root.Q<IntegerField>("delfinesN");
        input_fase1Complet = root.Q<Button>("fase1Complet");
        input_obstacleMin = root.Q<IntegerField>("obstacleMin");
        input_obstacleMax = root.Q<IntegerField>("obstacleMax");
        input_piruetasSimult = root.Q<Toggle>("piruetasSimult");
        input_jumpMin = root.Q<IntegerField>("jumpMin");
        input_jumpMax = root.Q<IntegerField>("jumpMax");
        input_piruetMin = root.Q<IntegerField>("piruetMin");
        input_piruetMax = root.Q<IntegerField>("piruetMax");
        input_obstacleVel = root.Q<FloatField>("obstacleVel");
        input_pointMax = root.Q<IntegerField>("pointMax");
        input_pointPirueta = root.Q<IntegerField>("pointPirueta");
        input_pointWrongGuess = root.Q<IntegerField>("pointWrongGuess");
        input_pointChoque = root.Q<IntegerField>("pointChoque");
        input_guardar = root.Q<Button>("guardar");

        //
        //Faltan los toggles de la fase 2

        input_guardar.RegisterCallback<ClickEvent>(GuardarTodo);
        input_fase1Complet.RegisterCallback<ClickEvent>(Fase1Complet);
    }

    void GuardarTodo(ClickEvent e)
    {
        Debug.Log("Guardando: carrilesN = " + input_carrilesN.value);
    }

    void Fase1Complet(ClickEvent e)
    {
        Debug.Log("Fase 1 completada");
        for (int i = 0; i < input_carrilesN.value; i++)
        {
            //INVESTIGAR COMO INSTANCIAR TOGGLES
            // fase2.AñadirLineaDeToggles()
            // deberíamos tener un array de toggles??? o
            // iteramos en los hijos del VisualElement padre?
        }
    }

    void DelfinColocado(ClickEvent e)
    {
        //Cada vez que se coloca un delfín, se llama a esta función
            //llamamos cuando se clika en el elem padre? puede dar errores pero 
            //callbacks desde cada toggle?? pereza no??
        //TODO: Actualizar texto de toggles restantes
    }
}
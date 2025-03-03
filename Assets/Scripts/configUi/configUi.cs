using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Windows;

public class configUi : MonoBehaviour
{
    [SerializeField]
    GameObject environmentObject;
    [SerializeField]
    GameObject managerstObject;
    [SerializeField]
    GameObject canvasObject;

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
    VisualElement input_toggleGroup;
    Label text_delfinesRestantes;
    int delfinesColocados = 0;

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
        input_toggleGroup = root.Q<VisualElement>("toggleGroup");
        text_delfinesRestantes = root.Q<Label>("delfinesRestantes");

        input_guardar.RegisterCallback<ClickEvent>(GuardarTodo);
        input_fase1Complet.RegisterCallback<ClickEvent>(Fase1Complet);
        input_toggleGroup.RegisterCallback<ClickEvent>(DelfinColocado);

        // Desactiva Juego
        ActivateGame(false);
    }

    void GuardarTodo(ClickEvent e)
    {
        Debug.Log("Guardando: carrilesN = " + input_carrilesN.value);


        //List<VisualElement> lveizda = izda.Children().ToList()


        // Activa Juego
        ActivateGame(true);
    }

    void Fase1Complet(ClickEvent e)
    {
        Debug.Log("Fase 1 completada");
        delfinesColocados = 0;
        input_toggleGroup.Clear(); //Borramos los carriles
        int n = input_carrilesN.value;
        if (n > 6) n = 6;
        for (int i = 0; i < n; i++)
        {
            VisualTreeAsset uiAsset = Resources.Load<VisualTreeAsset>("UI/carrilToggles");
            Debug.Log(uiAsset);
            VisualElement ui = uiAsset.Instantiate();
            input_toggleGroup.Add(ui);
        }
    }

    void DelfinColocado(ClickEvent e)
    {
        delfinesColocados = 0;
        for (int i = 0; i < input_toggleGroup.childCount; i++)
        {
            for(int j = 0; j < input_toggleGroup[i].childCount; j++)
            {
                if (input_toggleGroup[i][j].Q<Toggle>().value == true)
                {
                    delfinesColocados++;
                }
            }
        }
        text_delfinesRestantes.text = "Ahora mismo tienes " + (input_delfinesN.value - delfinesColocados) + " delfines restantes en el \r\njuego, aparecerán buceando.";
    }

    void ActivateGame(bool enable)
    {
        environmentObject.SetActive(enable);
        managerstObject.SetActive(enable);
        canvasObject.SetActive(enable);
    }
}
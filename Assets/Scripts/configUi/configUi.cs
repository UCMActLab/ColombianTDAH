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

    [SerializeField]
    configData config;

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
        //RIVER CONFIG
        Debug.Log("Guardando: carrilesN = " + input_carrilesN.value);
        config.NumCarriles = input_carrilesN.value;
        config.NumDelfines = input_delfinesN.value;

        // Posiciones de delfines: Active Toggles to Vector2d
        Vector2[] posDelfines = new Vector2[input_delfinesN.value];
        int i = 0;
        int j = 0;
        int delfinesEncontrados = 0;

        while (i < input_toggleGroup.childCount)
        {
            while (j < input_toggleGroup[i].childCount)
            {
                if (input_toggleGroup[i][j].Q<Toggle>().value == true)
                {
                    Debug.Log("dolphin pos colcoado " + i + " " + j);
                    posDelfines[delfinesEncontrados] = new Vector2(i, j);
                    delfinesEncontrados++;
                }
                j++;
            }
            i++;
        }

        //delfinesColocados = 0;
        //for (int i = 0; i < input_toggleGroup.childCount; i++)
        //{
        //    for (int j = 0; j < input_toggleGroup[i].childCount; j++)
        //    {
        //        if (input_toggleGroup[i][j].Q<Toggle>().value == true)
        //        {
        //            posDelfines[delfinesEncontrados] = new Vector2(i, j);
        //            delfinesColocados++;
        //            delfinesEncontrados++;
        //            Debug.Log("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
        //        }
        //    }
        //}
        //text_delfinesRestantes.text = "Ahora mismo tienes " + (input_delfinesN.value - delfinesColocados) + " delfines restantes en el \r\njuego, aparecer�n buceando.";

        Debug.Log("delfinesColocados " + delfinesColocados);

        Debug.Log("delfinesEncontrados" + delfinesEncontrados);
        for (int d = delfinesEncontrados; d < config.NumDelfines; d++)
        {
            //Debug.Log("dolphin pos no colcoado " + i + " " + j);

            posDelfines[d] = new Vector2(-1, -1);
        }

        config.PosDelfines = posDelfines;

        //List<VisualElement> lveizda = izda.Children().ToList()

        //OBS
        config.MinObstacleSpawn = input_obstacleMin.value; Debug.Log("scroipt"+config.MinObstacleSpawn); Debug.Log("input"+input_obstacleMin.value);
        config.MaxObstacleSpawn = input_obstacleMax.value;
        config.ObstacleSpeed = input_obstacleVel.value;

        //JUMP
        config.canSpecialJumpSimultaneously = input_piruetasSimult.value;
        config.MinTimeBetweenJumps = input_jumpMin.value;
        config.MaxTimeBetweenJumps = input_jumpMax.value;
        config.MinTimeBetweenSpecialJumps = input_piruetMin.value;
        config.MaxTimeBetweenSpecialJumps = input_piruetMax.value;

        //POINTS
        config.LevelPoints = input_pointMax.value;
        config.RightGuessPoints = input_pointPirueta.value;
        config.WrongGuessPoints = input_pointWrongGuess.value;
        config.HitObstaclePoints = input_pointChoque.value;

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
        Debug.Log("i carriles " + input_toggleGroup.childCount);
        Debug.Log("j carriles (Del primero) " + input_toggleGroup[0].Q<VisualElement>().childCount);

        for (int i = 0; i < input_toggleGroup.childCount; i++)
        {
            for (int j = 0; j < input_toggleGroup[i].childCount; j++)
            {
                Debug.Log(j);
                if (input_toggleGroup[i][j].Q<Toggle>().value == true)
                {
                    delfinesColocados++;
                    Debug.Log("oui");
                }
            }
        }
        text_delfinesRestantes.text = "Ahora mismo tienes " + (input_delfinesN.value - delfinesColocados) + " delfines restantes en el \r\njuego, aparecer�n buceando.";
    }

    void ActivateGame(bool enable)
    {
        environmentObject.SetActive(enable);
        managerstObject.SetActive(enable);
        canvasObject.SetActive(enable);
    }
}
using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;
using UnityEngine.Windows;
using System.Linq;


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
    bool hayErrores = true;
    bool mensajeError = false;

    [SerializeField]
    configData config = null;

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
        if (hayErrores)
        {
            if (!mensajeError)
            {
                mensajeError = true;
                VisualTreeAsset uiAsset = Resources.Load<VisualTreeAsset>("UI/errores");
                VisualElement ui = uiAsset.Instantiate();
                ui.style.color = new Color(1, 0, 0, 1);
                fase3.Add(ui);
            }
        }
        else
        {
            //RIVER CONFIG
            config.NumCarriles = input_carrilesN.value;
            config.NumDelfines = input_delfinesN.value;

            // Posiciones de delfines: Active Toggles to Vector2d
            Vector2[] posDelfines = new Vector2[input_delfinesN.value];
            int i = 0;
            int delfinesEncontrados = 0;
            while (i < input_toggleGroup.childCount)
            {
                int j = 0;
                while (j < input_toggleGroup[i][0].childCount)
                {
                    //Debug.Log("J = " + j+ " Val = "+ input_toggleGroup[i][0][j].Q<Toggle>().value);
                    if (input_toggleGroup[i][0][j].Q<Toggle>().value)
                    {
                        if (delfinesEncontrados < input_delfinesN.value)
                        {
                            posDelfines[delfinesEncontrados] = new Vector2(i, j); //si se ha pasado se fastidian los ultimos :p
                        }
                        delfinesEncontrados++;
                        //Debug.Log("delfin encontrado en x = " + j + ", y = " + i);
                    }
                    j++;
                }
                i++;
            }

            Debug.Log("delfinesColocados " + delfinesColocados);

            Debug.Log("delfinesEncontrados " + delfinesEncontrados);
            for (int d = delfinesEncontrados; d < config.NumDelfines; d++)
            {
                //Debug.Log("dolphin pos no colcoado " + i + " " + j);

                posDelfines[d] = new Vector2(-1, -1);
            }

            config.PosDelfines = posDelfines;

            //OBS
            config.MinObstacleSpawn = input_obstacleMin.value; Debug.Log("scroipt" + config.MinObstacleSpawn); Debug.Log("input" + input_obstacleMin.value);
            config.MaxObstacleSpawn = input_obstacleMax.value;
            config.ObstacleSpeed = input_obstacleVel.value;

            //JUMP
            config.canSpecialJumpSimultaneously = input_piruetasSimult.value;
            config.MinTimeBetweenJumps = input_jumpMin.value;
            config.MaxTimeBetweenJumps = input_jumpMax.value;
            config.MinCountBetweenSpecialJumps = input_piruetMin.value;
            config.MaxCountBetweenSpecialJumps = input_piruetMax.value;

            //POINTS
            config.LevelPoints = input_pointMax.value;
            config.RightGuessPoints = input_pointPirueta.value;
            config.WrongGuessPoints = input_pointWrongGuess.value;
            config.HitObstaclePoints = input_pointChoque.value;

            // Activa Juego
            ActivateGame(true);

        }
    }

    void Update()
    {
        if (!hayErrores && mensajeError)
        {
            mensajeError = false;
            fase3.RemoveAt(fase3.childCount - 1);
        }
    }

    void Fase1Complet(ClickEvent e)
    {
        //Debug.Log("Fase 1 completada");
        delfinesColocados = 0;
        input_toggleGroup.Clear(); //Borramos los carriles
        int n = input_carrilesN.value;
        if (n > 6) n = 6;
        for (int i = 0; i < n; i++)
        {
            VisualTreeAsset uiAsset = Resources.Load<VisualTreeAsset>("UI/carrilToggles");
            //Debug.Log(uiAsset);
            VisualElement ui = uiAsset.Instantiate();
            //Debug.Log("uichildcount"+ui.name + ui.childCount);
            //Debug.Log("uichildcount" + ui[0].name + ui[0].childCount);

            for (int j = 0; j < ui.childCount; j++)
            {
                ui[j].name = "Toggle" + i.ToString() + j.ToString();
            }

            input_toggleGroup.Add(ui);
            //Debug.Log("misninosinputtoggle"+input_toggleGroup.childCount);

        }
    }

    void DelfinColocado(ClickEvent e)
    {
        delfinesColocados = 0;

        for (int i = 0; i < input_toggleGroup.childCount; i++)
        {
            for (int j = 0; j < input_toggleGroup[i][0].childCount; j++)
            {
                if (input_toggleGroup[i][0][j].Q<Toggle>().value)
                {
                    delfinesColocados++;
                    Debug.Log("delfin colocado en x = " + j + ", y = " + i);
                }
            }
        }
        int delfinesRestantes = input_delfinesN.value - delfinesColocados;
        if (delfinesRestantes < 0)
        {
            hayErrores = true;
            text_delfinesRestantes.style.color = new Color(1, 0, 0, 1);
            text_delfinesRestantes.text = "Por favor retire " + Mathf.Abs(delfinesRestantes) + " delfines. \r\n Como mucho puede tener " + input_delfinesN.value + " delfines en el río.";
        }
        else
        {
            hayErrores = false;
            text_delfinesRestantes.style.color = new Color(0, 0, 0, 1);
            text_delfinesRestantes.text = "Ahora mismo tienes " + delfinesRestantes + " delfines restantes en el \r\njuego, aparecerán buceando.";
        }
    }

    void ActivateGame(bool enable)
    {
        environmentObject.SetActive(enable);
        managerstObject.SetActive(enable);
        canvasObject.SetActive(enable);
        if(enable == true)DolphinLevelManager.Instance.InitLevel(config);
    }
}
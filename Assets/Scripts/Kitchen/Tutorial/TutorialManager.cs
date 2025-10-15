using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public enum Step { Intro, Tablon, Book, GrabGuayabaFake, DropOnSliceTableFake, DropInPot, Sponge, GrabGuayaba, DropOnSliceTable, MakeBocadillo, DeliverBocadillo, End }

    static private TutorialManager _instance;

    static public TutorialManager Instance { get { return _instance; } }

    [Header("Refs")]
    [SerializeField] private TutorialUI ui;
    [SerializeField] private HighlightManager highlighter;  
    [SerializeField] private RecetasDatabase recetasDb;
    [SerializeField] private RecetaData recetaObjetivo;

    [Header("Ingredientes&Puestos")]
    [SerializeField] private Ingredientes guayaba = Ingredientes.Guayaba;
    [SerializeField] private Ingredientes sugar = Ingredientes.Azucar;
    [SerializeField] private Ingredientes guayabaChopped = Ingredientes.Guayaba_picada;
    [SerializeField] private PuestosDeTrabajo sliceTable = PuestosDeTrabajo.Tabla_De_Picar;
    [SerializeField] private PuestosDeTrabajo pot = PuestosDeTrabajo.Olla;

    private Step current;
    [SerializeField] private GameObject sliceWs;
    [SerializeField] private GameObject potWs;
    private GameObject guayabaGO;
    private GameObject guayabaChoppedGO;
    private GameObject sugarGO;
    private GameObject sponge;
    private GameObject book;
    private GameObject tablonGO;
    private GameObject tablonButtonGO;
    private GameObject conveyorGO;

    private bool tablonOpened, tablonClosed, bookOpened, bookClosed, placedOnSliceTableFake, guayabaDropInPot, cleanPot, placedOnSliceTable, bocadilloReadyAtPot, bocadilloDelivered;

    private bool subscribed;

    #region methods
    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        TrySubscribe();
        if (!subscribed) StartCoroutine(SubscribeWhenReady());       
    }
    void OnDisable()
    {
        TryUnsubscribe();
        DraggableBlocker.ResetAll();
    }

    void Start()
    {
        if (!LevelKitchenManager.Instance || !LevelKitchenManager.Instance.GetTutorial())
        {
            gameObject.SetActive(false);
            return;
        }
        ResolveSceneRefs();
        StartCoroutine(Run());
    }

    private void ResolveSceneRefs()
    {
        guayabaGO = IngredientSpawnManager.Instance?.GetLiveInstance(guayaba);
        sugarGO = IngredientSpawnManager.Instance?.GetLiveInstance(sugar);

        book = LevelKitchenManager.Instance.GetBook();

        sponge = LevelKitchenManager.Instance.GetSponge();
        tablonGO = LevelKitchenManager.Instance.GetTablon();
        tablonButtonGO = LevelKitchenManager.Instance.GetTablonButton();
        conveyorGO = LevelKitchenManager.Instance.GetConveyor();

        DraggableBlocker.Block(DraggableBlocker.Source.Tutorial);
        DraggableBlocker.ClearAllowed();
    }


    private IEnumerator SubscribeWhenReady()
    {
        while (LevelKitchenManager.Instance == null) yield return null;
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribed) return;
       
        LevelKitchenManager.Instance.OnBookOpenedTutorial += HandleBookOpened;
        LevelKitchenManager.Instance.OnTablonOpenedTutorial += HandleTablonOpened;
        LevelKitchenManager.Instance.OnBookClosedTutorial += HandleBookClosed;
        LevelKitchenManager.Instance.OnTablonClosedTutorial += HandleTablonClosed;

        WorkstationProcessor.OnIngredientSpawnedGlobal += OnIngredientSpawned;
        WorkstationProcessor.OnItemPlacedGlobal += OnItemPlaced;
        WorkstationProcessor.OnClean += OnCleanPot;
        WorkstationProcessor.OnRecipeCraftedGlobal += OnRecipeCrafted;
        ConveyorBelt.OnDeliveredGlobal += OnDelivered;

        subscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!subscribed) return;

        LevelKitchenManager.Instance.OnBookOpenedTutorial -= HandleBookOpened;
        LevelKitchenManager.Instance.OnTablonOpenedTutorial -= HandleTablonOpened;
        LevelKitchenManager.Instance.OnBookClosedTutorial -= HandleBookClosed;
        LevelKitchenManager.Instance.OnTablonClosedTutorial -= HandleTablonClosed;

        WorkstationProcessor.OnIngredientSpawnedGlobal -= OnIngredientSpawned;
        WorkstationProcessor.OnItemPlacedGlobal -= OnItemPlaced;
        WorkstationProcessor.OnClean -= OnCleanPot;
        WorkstationProcessor.OnRecipeCraftedGlobal -= OnRecipeCrafted;
        ConveyorBelt.OnDeliveredGlobal -= OnDelivered;

        subscribed = false;
    }

    private void AllowOnly(params GameObject[] gos)
    {
        DraggableBlocker.AllowOnly(gos);
    }

    private IEnumerator Run()
    {
        // STEP: Intro
        current = Step.Intro;
        DraggableBlocker.ClearAllowed();
        ui.ShowLines(new[]
        {
            "¡Bienvenido a la cocina!",
            "Aquí aprenderás lo básico en pocos pasos.",
            "Tendrás que hacer uso de las estaciones de trabajo.", 
            "Y realizar las recetas en el tiempo estimado",
            "Primero, mira el tablón de tareas."
        });
        yield return WaitClickPanelClosed();
        Debug.Log("Terminado el 1º paso");

        // STEP: Tablon
        current = Step.Tablon;
        AllowOnly(tablonGO);
        highlighter.Highlight(LevelKitchenManager.Instance.GetTablon());         
        yield return WaitEvent(() => tablonOpened);
        Debug.Log("Tablon abierto(tutorial)");
        AllowOnly(tablonButtonGO);
        highlighter.ClearAll();
        yield return WaitEvent(() => tablonClosed);
        Debug.Log("Tablon cerrado(tutorial)");

        // STEP: Book
        current = Step.Book;
        ui.ShowLines(new[] { "Ahora, abre el libro de recetas." });
        yield return WaitClickPanelClosed();
        AllowOnly(book ? book.gameObject : null);
        highlighter.Highlight(book.gameObject);
        yield return WaitEvent(() => bookOpened);
        Debug.Log("Libro abierto(tutorial)");
        highlighter.ClearAll();
        yield return WaitEvent(() => bookClosed);
        Debug.Log("Libro cerrado(tutorial)");

        // STEP: GrabGuayabaFake
        current = Step.GrabGuayabaFake;
        RefreshIngredientRefs(); 
        ui.ShowLines(new[]
        {
            "¡Muy bien! Ya sabemos que necesitamos.",
            "Ahora agarra la guayaba y llévala a la tabla de cortar."
        });
        yield return WaitClickPanelClosed();
        AllowOnly(guayabaGO);
        if (guayabaGO) highlighter.Highlight(guayabaGO);
        yield return WaitUntilDragging(guayabaGO);
        Debug.Log("Guayaba agarrada para limpiar(tutorial)");

        // STEP: DropOnSliceTableFake
        current = Step.DropOnSliceTableFake;
        AllowOnly(guayabaGO, sliceWs ? sliceWs.gameObject : null);
        if (sliceWs) highlighter.Highlight(sliceWs.gameObject);
        yield return WaitEvent(() => placedOnSliceTableFake);
        DraggableBlocker.ClearAllowed();
        Debug.Log("Guayaba en tabla de cortar(tutorial)");
        highlighter.ClearAll();
        yield return WaitEvent(() => guayabaChoppedGO != null);

        // STEP: DropInPot
        current = Step.DropInPot;     
        ui.ShowLines(new[]
        {
            "Tras un tiempo...La guayaba se procesa.",
            "Vale, ahora lleva la guayaba picada a la olla."
        });
        yield return WaitClickPanelClosed();
        AllowOnly(guayabaChoppedGO, potWs);
        if (potWs) highlighter.Highlight(potWs);
        if (guayabaChoppedGO) highlighter.Highlight(guayabaChoppedGO);
        yield return WaitEvent(() => guayabaDropInPot);
        highlighter.ClearAll();
        Debug.Log("Guayaba en olla(tutorial)");

        // STEP: Sponge
        current = Step.Sponge; 
        ui.ShowLines(new[]
        {
            "Vamos a ver ahora como limpiar la olla.",
            "Esto sirve para quitar los ingredientes que hay dentro.",
            "Puedes comprobar lo que hay dentro pulsando sobre la estación de trabajo.",
            "Todo esto es útil por si te equivocas así que...",
            "Agarra la esponja y suéltala en la olla"
        });
        yield return WaitClickPanelClosed();
        AllowOnly(sponge, potWs);
        if (sponge) highlighter.Highlight(sponge);
        yield return WaitEvent(() => cleanPot);
        highlighter.ClearAll();
        Debug.Log("Limpieza finalizada(tutorial)");

        // STEP: GrabGuayaba
        current = Step.GrabGuayaba;    
        RefreshIngredientRefs();        
        ui.ShowLines(new[] 
        { 
            "¡Muy bien! Vamos a repetir el proceso anterior",
            "Agarra de nuevo la guayaba y llévala a la tabla de cortar." 
        });
        yield return WaitClickPanelClosed();
        AllowOnly(guayabaGO);
        if (guayabaGO) highlighter.Highlight(guayabaGO);
        yield return WaitUntilDragging(guayabaGO);
        Debug.Log("Guayaba agarrada(tutorial)");

        // STEP: DropOnSliceTable
        current = Step.DropOnSliceTable;
        AllowOnly(guayabaGO, sliceWs ? sliceWs.gameObject : null);
        if (guayabaGO && sliceWs) highlighter.Highlight(sliceWs.gameObject);                
        yield return WaitEvent(() => placedOnSliceTable);
        Debug.Log("Guayaba en tabla de cortar(tutorial)");
        highlighter.ClearAll();
        yield return WaitEvent(() => guayabaChoppedGO != null);

        // STEP: MakeBocadillo
        current = Step.MakeBocadillo;       
        ui.ShowLines(new[] 
        {
            "¡Es hora de hacer el bocadillo!",
            "Lleva la guayaba picada y el azúcar a la olla." 
        });
        yield return WaitClickPanelClosed();
        AllowOnly(potWs, sugarGO, guayabaChoppedGO);
        if (potWs) highlighter.Highlight(potWs);
        if (sugarGO) highlighter.Highlight(sugarGO);
        if (guayabaChoppedGO) highlighter.Highlight(guayabaChoppedGO);
        yield return WaitEvent(() => bocadilloReadyAtPot);
        highlighter.ClearAll();
        Debug.Log("Bocadillo hecho(tutorial)");

        // STEP: DeliverBocadillo
        current = Step.DeliverBocadillo; 
        ui.ShowLines(new[] { "¡Perfecto! Lleva el bocadillo a la cinta transportadora para entregarlo." });
        yield return WaitClickPanelClosed();
        AllowOnly(conveyorGO, GetCompletedRecipe());
        highlighter.Highlight(LevelKitchenManager.Instance.GetConveyor());
        yield return WaitEvent(() => bocadilloDelivered);
        highlighter.ClearAll();
        Debug.Log("Bocadillo puesto en la cinta(tutorial)");

        // STEP: End
        current = Step.End;
        ui.ShowLines(new[] 
        { 
            "¡Tutorial completado!",
            "Ya puedes empezar a cocinar por tu cuenta." 
        });
        yield return WaitClickPanelClosed();
        Debug.Log("Tutorial terminado");

        LevelKitchenManager.Instance.StopTutorialMode();

        SceneLoader.LoadScene("KitchenLevelSelector");
    }

    private IEnumerator WaitClickPanelClosed()
    { 
        while (ui != null && !ui.ClosedByUser)
        {
            if (LevelKitchenManager.Instance.IsPaused())
            {
                yield return null;
                continue;
            }
            yield return null;
        }
  
        if (ui != null) ui.Hide(false);
    }

    private IEnumerator WaitEvent(System.Func<bool> cond)
    {
        while (!cond()) yield return null;
    }

    private IEnumerator WaitUntilDragging(GameObject go)
    {
        if (!go) yield break;
        var d = go.GetComponent<Draggable>();
        while (d == null || !d.isDragging) yield return null;
    }

    private GameObject GetCompletedRecipe()
    {
        GameObject plate = null;
        var cr = FindFirstObjectByType<CompletedRecipe>();
        if (cr != null) plate = cr.gameObject;
        return plate;       
    }

    private void RefreshIngredientRefs()
    {
        if (!IngredientSpawnManager.HasInstance) return;
        if (guayabaGO == null)
            guayabaGO = IngredientSpawnManager.Instance.GetLiveInstance(guayaba);
        if (guayabaChoppedGO == null)
            guayabaChoppedGO = IngredientSpawnManager.Instance.GetLiveInstance(guayabaChopped);
        if (sugarGO == null) 
            sugarGO = IngredientSpawnManager.Instance.GetLiveInstance(sugar);
    }

    private void OnIngredientSpawned(Ingredientes ing, GameObject go)
    {
        if (ing == guayaba) 
            guayabaGO = go;       
        else if (ing == guayabaChopped)
            guayabaChoppedGO = go;
        if (ing == sugar)
            sugarGO = go;
    }

    private void OnItemPlaced(Ingredientes ing, PuestosDeTrabajo ws)
    {
        if (current == Step.DropOnSliceTable && ing == guayaba && ws == sliceTable)
            placedOnSliceTable = true;
        else if (current == Step.DropOnSliceTableFake && ing == guayaba && ws == sliceTable)
            placedOnSliceTableFake = true;
        else if (current == Step.DropInPot && ing == guayabaChopped && ws == pot)
            guayabaDropInPot = true;
    } 

    private void OnCleanPot(PuestosDeTrabajo ws)
    {
        if(current == Step.Sponge && ws == pot)
            cleanPot = true;
    }

    private void OnRecipeCrafted(RecetaData receta, PuestosDeTrabajo ws)
    {
        if (current == Step.MakeBocadillo && receta != null && receta == recetaObjetivo && ws == pot)
        {
            bocadilloReadyAtPot = true;
        }
    }

    private void OnDelivered(RecetaData receta)
    {
        if (current == Step.DeliverBocadillo && receta != null && !receta.esIntermedia)
            bocadilloDelivered = true;
    }
    
    private void HandleBookOpened() { bookOpened = true; }
    private void HandleBookClosed() { bookClosed = true; }
    private void HandleTablonOpened() { tablonOpened = true; }
    private void HandleTablonClosed() { tablonClosed = true; }

    void OnDestroy()
    {
        if (LevelKitchenManager.Instance != null)
        {
            LevelKitchenManager.Instance.OnBookOpenedTutorial -= HandleBookOpened;
            LevelKitchenManager.Instance.OnTablonOpenedTutorial -= HandleTablonOpened;
            LevelKitchenManager.Instance.OnBookClosedTutorial -= HandleBookClosed;
            LevelKitchenManager.Instance.OnTablonClosedTutorial -= HandleTablonClosed;
        }
        WorkstationProcessor.OnIngredientSpawnedGlobal -= OnIngredientSpawned;
        WorkstationProcessor.OnItemPlacedGlobal -= OnItemPlaced;
        WorkstationProcessor.OnClean -= OnCleanPot;
        WorkstationProcessor.OnRecipeCraftedGlobal -= OnRecipeCrafted;
        ConveyorBelt.OnDeliveredGlobal -= OnDelivered;

        DraggableBlocker.ResetAll();
    }
    #endregion
}


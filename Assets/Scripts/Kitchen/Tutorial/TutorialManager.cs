using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private Ingredientes guayabaChopped = Ingredientes.GuayabaPicada;
    [SerializeField] private PuestosDeTrabajo sliceTable = PuestosDeTrabajo.Tabla_De_Picar;
    [SerializeField] private PuestosDeTrabajo pot = PuestosDeTrabajo.Olla;

    private Step current;
    private WorkstationProcessor sliceWs;
    private WorkstationProcessor potWs;
    private GameObject guayabaGO;
    private GameObject guayabaChoppedGO;
    private GameObject sugarGO;
    private GameObject sponge;
    private BookGestor book;

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
        sliceWs = FindObjectsOfType<WorkstationProcessor>().FirstOrDefault(w => w.workstationType == sliceTable);
        potWs = FindObjectsOfType<WorkstationProcessor>().FirstOrDefault(w => w.workstationType == pot);

        guayabaGO = IngredientSpawnManager.Instance?.GetLiveInstance(guayaba);
        sugarGO = IngredientSpawnManager.Instance?.GetLiveInstance(sugar);

        book = FindObjectOfType<BookGestor>(true);

        sponge = LevelKitchenManager.Instance.GetSponge();
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

    private IEnumerator Run()
    {
        // STEP: Intro
        current = Step.Intro;
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
        highlighter.Highlight(LevelKitchenManager.Instance.GetTablon());         
        yield return WaitEvent(() => tablonOpened);
        Debug.Log("Tablon abierto(tutorial)");
        highlighter.ClearAll();
        yield return WaitEvent(() => tablonClosed);
        Debug.Log("Tablon cerrado(tutorial)");

        // STEP: Book
        current = Step.Book;
        ui.ShowLines(new[] { "Ahora, abre el libro de recetas." });
        yield return WaitClickPanelClosed();
        highlighter.Highlight(book.gameObject);
        yield return WaitEvent(() => bookOpened);
        Debug.Log("Libro abierto(tutorial)");
        highlighter.ClearAll();
        yield return WaitEvent(() => bookClosed);
        Debug.Log("Libro cerrado(tutorial)");

        // STEP: GrabGuayabaFake
        current = Step.GrabGuayabaFake;
        RefreshIngredientRefs(); // Por si había respawn
        ui.ShowLines(new[]
        {
            "¡Muy bien! Ya sabemos que necesitamos.",
            "Ahora agarra la guayaba y llévala a la tabla de cortar."
        });
        yield return WaitClickPanelClosed();
        if (guayabaGO) highlighter.Highlight(guayabaGO);
        yield return WaitUntilDragging(guayabaGO);
        Debug.Log("Guayaba agarrada para limpiar(tutorial)");
        highlighter.ClearAll();

        // STEP: DropOnSliceTableFake
        current = Step.DropOnSliceTableFake;
        if (sliceWs) highlighter.Highlight(sliceWs.gameObject);
        yield return WaitEvent(() => placedOnSliceTableFake);
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
        if (potWs) highlighter.Highlight(potWs.gameObject);
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
        if (sponge) highlighter.Highlight(sponge);
        yield return WaitEvent(() => cleanPot);
        highlighter.ClearAll();
        Debug.Log("Limpieza finalizada(tutorial)");

        // STEP: GrabGuayaba
        current = Step.GrabGuayaba;
        RefreshIngredientRefs(); // Por si había respawn
        ui.ShowLines(new[] 
        { 
            "¡Muy bien! Vamos a repetir el proceso anterior",
            "Agarra de nuevo la guayaba y llévala a la tabla de cortar." 
        });
        yield return WaitClickPanelClosed();
        if (guayabaGO) highlighter.Highlight(guayabaGO);
        yield return WaitUntilDragging(guayabaGO);
        Debug.Log("Guayaba agarrada(tutorial)");
        highlighter.ClearAll();

        // STEP: DropOnSliceTable
        current = Step.DropOnSliceTable;
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
        if (potWs) highlighter.Highlight(potWs.gameObject);
        if (sugarGO) highlighter.Highlight(sugarGO);
        if (guayabaChoppedGO) highlighter.Highlight(guayabaChoppedGO);
        yield return WaitEvent(() => bocadilloReadyAtPot);
        highlighter.ClearAll();
        Debug.Log("Bocadillo hecho(tutorial)");

        // STEP: DeliverBocadillo
        current = Step.DeliverBocadillo;
        ui.ShowLines(new[] { "¡Perfecto! Lleva el bocadillo a la cinta transportadora para entregarlo." });
        yield return WaitClickPanelClosed();
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
    }
    #endregion
}


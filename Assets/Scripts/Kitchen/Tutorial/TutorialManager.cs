using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public enum Step { Intro, Book, Tablon, GrabGuayaba, DropOnSliceTable, MakeBocadillo, DeliverBocadillo, End }

    static private TutorialManager _instance;

    static public TutorialManager Instance { get { return _instance; } }

    [Header("Refs")]
    [SerializeField] private TutorialUI ui;
    [SerializeField] private HighlightManager highlighter;
    [SerializeField] private GuideArrow arrow; 
    [SerializeField] private RecetasDatabase recetasDb;
    [SerializeField] private RecetaData recetaObjetivo;

    [Header("Ingredientes&Puestos")]
    [SerializeField] private Ingredientes guayaba = Ingredientes.Guayaba;
    [SerializeField] private PuestosDeTrabajo sliceTable = PuestosDeTrabajo.Tabla_De_Picar;
    [SerializeField] private PuestosDeTrabajo pot = PuestosDeTrabajo.Olla;

    private Step current;
    private WorkstationProcessor sliceWs;
    private WorkstationProcessor potWs;
    private GameObject guayabaGO;
    private GameObject sugarGO;
    private BookGestor book;

    private bool bookOpened, bookClosed, tablonOpened, tablonClosed, placedOnSliceTable, bocadilloReadyAtPot, bocadilloDelivered;

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
        sugarGO = IngredientSpawnManager.Instance?.GetLiveInstance(Ingredientes.Azucar);

        book = FindObjectOfType<BookGestor>(true);
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

        WorkstationProcessor.OnItemPlacedGlobal += OnItemPlaced;
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

        WorkstationProcessor.OnItemPlacedGlobal -= OnItemPlaced;
        ConveyorBelt.OnDeliveredGlobal -= OnDelivered;

        subscribed = false;
    }

    private IEnumerator Run()
    {
        arrow.Hide();
        // STEP: Intro
        current = Step.Intro;
        ui.ShowLines(new[]
        {
            "¡Bienvenido a la cocina!",
            "Aquí aprenderás lo básico en pocos pasos.",
            "Tendrás que realizar las recetas en el tiempo estimado"           
        });
        yield return WaitClickPanelClosed();
        Debug.Log("Terminado el 1º paso");

        // STEP: Book
        current = Step.Book;
        highlighter.Highlight(book.gameObject);
        ui.ShowLines(new[] { "Primero, abre el libro de recetas." });
        yield return WaitEvent(() => bookOpened);
        Debug.Log("Libro abierto(tutorial)");
        highlighter.Clear();
        yield return WaitEvent(() => bookClosed);
        Debug.Log("Libro cerrado(tutorial)");

        // STEP: Tablon
        current = Step.Tablon;
        //highlighter.Highlight(LevelKitchenManager.Instance.GetTablon());       
        ui.ShowLines(new[] { "Ahora, mira el tablón de tareas." });
        yield return WaitEvent(() => tablonOpened);
        Debug.Log("Tablon abierto(tutorial)");
        highlighter.Clear();
        yield return WaitEvent(() => tablonClosed);
        Debug.Log("Tablon cerrado(tutorial)");

        // STEP: GrabGuayaba
        current = Step.GrabGuayaba;
        RefreshIngredientRefs(); // Por si había respawn
        if (guayabaGO) highlighter.Highlight(guayabaGO);
        ui.ShowLines(new[] { "Agarra la guayaba." });
        yield return WaitUntilDragging(guayabaGO);
        highlighter.Clear();

        // STEP: DropOnSliceTable
        current = Step.DropOnSliceTable;
        if (guayabaGO && sliceWs)
        {
            highlighter.Highlight(sliceWs.gameObject);
            if (arrow) arrow.Set(GetAnchor(guayabaGO), sliceWs.spawnPoint);
        }
        ui.ShowLines(new[] { "Llévalo a la tabla de cortar." });
        yield return WaitEvent(() => placedOnSliceTable);
        highlighter.Clear();
        if (arrow) arrow.Hide();

        // STEP: MakeBocadillo
        current = Step.MakeBocadillo;
        RefreshIngredientRefs(); // La guayaba cortada y el azucar deberían existir
        if (potWs) highlighter.Highlight(potWs.gameObject);
        ui.ShowLines(new[] { "Ahora prepara un bocadillo en la olla. Necesitas guayaba picada y azúcar." });
        yield return WaitEvent(() => bocadilloReadyAtPot);
        highlighter.Clear();

        // STEP: DeliverJuice
        current = Step.DeliverBocadillo;
        ui.ShowLines(new[] { "¡Perfecto! Lleva el bocadillo a la cinta transportadora para entregarlo." });
        yield return WaitEvent(() => bocadilloDelivered);

        // STEP: End
        current = Step.End;
        ui.ShowLines(new[] { "¡Tutorial completado! Ya puedes empezar a cocinar por tu cuenta." });
        yield return WaitClickPanelClosed();

        // SceneManager.LoadScene("KitchenLevelSelector");
    }

    private IEnumerator WaitClickPanelClosed()
    {
        while (ui != null && ui.IsOpen)
            yield return null;
        ui?.Hide();
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
        if (guayabaGO == null) guayabaGO = IngredientSpawnManager.Instance.GetLiveInstance(guayaba);
        if (sugarGO == null) sugarGO = IngredientSpawnManager.Instance.GetLiveInstance(Ingredientes.Azucar);
    }

    private Transform GetAnchor(GameObject go)
    {
        var t = go.transform.Find("GrabAnchor");
        return t ? t : go.transform;
    }

    private void OnItemPlaced(Ingredientes ing, PuestosDeTrabajo ws)
    {
        if (current == Step.DropOnSliceTable && ing == guayaba && ws == sliceTable)
        {
            placedOnSliceTable = true;
        }
        if (current == Step.MakeBocadillo && ws == pot)
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
    }
    #endregion
}


using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    public enum Step { Intro, OpenBook, OpenTablon, GrabGuayaba, DropOnSliceTable, MakeBocadillo, DeliverBocadillo, End }

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

    private bool bookOpened, tablonOpened, placedOnSliceTable, bocadilloReadyAtPot, bocadilloDelivered;

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
    }


    private IEnumerator SubscribeWhenReady()
    {
        while (LevelKitchenManager.Instance == null) yield return null;
        TrySubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribed) return;
        var mgr = LevelKitchenManager.Instance;
        if (mgr == null) return;

        mgr.OnBookOpenedTutorial += HandleBookOpened;
        mgr.OnTablonOpenedTutorial += HandleTablonOpened;
        WorkstationProcessor.OnItemPlacedGlobal += OnItemPlaced;
        ConveyorBelt.OnDeliveredGlobal += OnDelivered;

        subscribed = true;
    }

    private void TryUnsubscribe()
    {
        if (!subscribed) return;
        var mgr = LevelKitchenManager.Instance;
        if (mgr != null)
        {
            mgr.OnBookOpenedTutorial -= HandleBookOpened;
            mgr.OnTablonOpenedTutorial -= HandleTablonOpened;
        }
        WorkstationProcessor.OnItemPlacedGlobal -= OnItemPlaced;
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
            "Tendrás que realizar las recetas en el tiempo estimado"           
        });
        yield return WaitClickPanelClosed();

        // STEP: OpenBook
        current = Step.OpenBook;
        //highlighter.Highlight(LevelKitchenManager.Instance.GetBook());
        ui.ShowLines(new[] { "Primero, abre el libro de recetas." });
        yield return WaitEvent(() => bookOpened);
        highlighter.Clear();

        // STEP: OpenTablon
        current = Step.OpenTablon;
        //highlighter.Highlight(LevelKitchenManager.Instance.GetTablon());       
        ui.ShowLines(new[] { "Ahora, mira el tablón de tareas." });
        yield return WaitEvent(() => tablonOpened);
        highlighter.Clear();

        // STEP: GrabGuayaba
        current = Step.GrabGuayaba;
        RefreshIngredientRefs(); // Por si había respawn
        if (guayabaGO) highlighter.Highlight(guayabaGO);
        ui.ShowLines(new[] { "Agarra la guayaba." });
        yield return WaitUntilDragging(guayabaGO);
        highlighter.Clear();

        // STEP: DropOnChop
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
        // Espera a que el panel se cierre
        while (true)
        {
            // Si el panel está inactivo, salimos
            var p = ui.gameObject.activeInHierarchy;
            // Usamos el propio panel como referencia
            yield return new WaitForSeconds(0.1f);
            if (!ui.gameObject.activeSelf) break;
        }
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
    private void HandleTablonOpened() { tablonOpened = true; }

    void OnDestroy()
    {
        if (LevelKitchenManager.Instance != null)
        {
            LevelKitchenManager.Instance.OnBookOpenedTutorial -= HandleBookOpened;
            LevelKitchenManager.Instance.OnTablonOpenedTutorial -= HandleTablonOpened;
        }
    }
    #endregion
}


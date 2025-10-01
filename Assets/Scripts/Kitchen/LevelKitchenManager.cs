using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

[Serializable]
public class IngredienteSpriteList
{
    public Ingredientes nombre;
    public Sprite sprite;
}

[Serializable]
public class RecetaSpriteList
{
    public string nombre;
    public RecetaSprites sprites;
}

[Serializable]
public struct RecetaSprites
{
    public Sprite spriteLibro;
    public Texture spriteTablon;
}

public enum TurnoEstado
{
    Bloqueado,
    Activo,
    Completado
}

public enum Turno
{
    Manana,
    Tarde,
    Noche
}
public enum PuestosDeTrabajo
{
    Tabla_De_Picar,
    Licuadora,
    Olla,
    Sarten,
    Mezcladora,
    Olla_A_Presion,
    Horno
}

public enum Ingredientes
{
    Agua, Aguacate, AguacatePicado, Aji, Arepa, Arroz, ArrozCocido, Azucar, Cafe, Canela, Carne, CarneCocinada, Cebolla, CebollaPicada, 
    Cilantro, Coco, CocoPicado, Frijoles, FrijolesCocidos, Fresa,FresaPicada, Guayaba, GuayabaPicada, Harina, Hojas, Huevo, Hueso, Leche, Limon, LimonPicado, Maiz, Mariscos,
    MasaArroz, MasaLeche, MasaMaíz, MasaQueso, Miel, MixVegetales, Panela, Papa, PapaPicada, Pez, PezFileteado, Platano, PlatanoPicado, Pollo, Queso, Tomate, 
    TomatePicado, Yuca, YucaPicada
}

public class LevelKitchenManager : MonoBehaviour
{
    private static LevelKitchenManager _instance = null;

    static public LevelKitchenManager Instance { get { return _instance; } }

    private string[] escenasPermitidas = { "KitchenLevel", "KitchenLevelSelector" };

    [Header("Configuración")]
    [SerializeField] private RecetasDatabase recetasDatabase;
    [SerializeField] private NivelacionData nivelacionData;
    [SerializeField] private RecetaData recetaTutorialObjetivo;

    [SerializeField] private List<RecetaSpriteList> recetasSpritesSerializable;   
    private Dictionary<string, RecetaSprites> recetasSprites;

    [SerializeField] private List<IngredienteSpriteList> ingredientesSpritesSerializable;
    private Dictionary<Ingredientes, Sprite> ingredientesSprites;

    private GameObject tablon;
    private GameObject recetasColgadas;
    private GameObject tablonButton;
    private RecipeBoard rec;
    private GameObject conveyor;
    private GameObject sponge;
    private GameObject pauseCollider;
    private GameObject endCollider;

    private Transform cameraInitPos;
    private Transform cameraTablonPos;

    private int jornadaMaxDesbloqueada = 0;
    private Turno turnoMaxDesbloqueado = Turno.Manana;

    private int jornadaActual = 0; //nivelacionData.jornadas[jornadaActual].recetasAsignadas
    private Turno turnoActual = Turno.Manana;
    private int tiempoPorTurnoTotal;
    private List<RecetaData> recetasToDo;

    private Reloj contador;

    private CalculateStats winStats;
    private CalculateStats loseStats;

    // Contador de recetas del turno: receta final -> cuántas faltan
    private Dictionary<RecetaData, int> recetasRestantes = new();
    private int recetasTotalesIniciales;

    private GameObject hand;          
    private bool hideHandWhenIdle = true;
    private Transform handFollowTarget;
    private Vector3 handOffset;        // Offset desde el centro del objeto al punto de agarre
    private float handRayDepth = 4.5f; 
    private Camera mainCam;

    private bool paused;

    private GameObject tutorialSystemRoot;
    private bool isTutorial = false;
    public event Action OnBookOpenedTutorial;
    public event Action OnTablonOpenedTutorial;
    public event Action OnBookClosedTutorial;
    public event Action OnTablonClosedTutorial;

    #region properties
    private int NOpenedBook = 0;
    #endregion

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
        //ActivateGame();
    }

    private void Start()
    {
        CalcularRecetasPorJornada();
        mainCam = Camera.main;
        recetasSprites = recetasSpritesSerializable.ToDictionary(m => m.nombre, m => m.sprites);
        ingredientesSprites = ingredientesSpritesSerializable.ToDictionary(m => m.nombre, m => m.sprite);
    }

    private void Update()
    {
        
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        bool escenaPermitida = false;

        foreach (string nombre in escenasPermitidas)
        {
            if (scene.name == nombre)
            {
                escenaPermitida = true;
                break;
            }
        }

        if (!escenaPermitida)
        {
            Destroy(gameObject);
            Destroy(AnimatorManager.Instance.gameObject);
            Destroy(KitchenSoundManager.Instance.gameObject);
            Destroy(IngredientSpawnManager.Instance.gameObject);
        }

        if (scene.name == escenasPermitidas[0]) // KitchenLevel
        {
            DraggableBlocker.ResetAll();     
            Draggable tab = tablon.GetComponent<Draggable>();
            Draggable tabButtonDrag = tablonButton.GetComponent<Draggable>();
            rec = recetasColgadas.GetComponent<RecipeBoard>();

            if (tab != null)
            {
                tab.onStartDragging.RemoveListener(OnTabClicked);
                tab.onStartDragging.AddListener(OnTabClicked);
            }
            else
            {
                Debug.LogWarning("No se encontró el componente Draggable en tablon.");
            }

            if (tabButtonDrag != null)
            {
                tabButtonDrag.onStartDragging.RemoveListener(ButtonTabClicked);
                tabButtonDrag.onStartDragging.AddListener(ButtonTabClicked);
            }
            else
            {
                Debug.LogWarning("No se encontró el componente Draggable en tablonButton.");
            }


            if (tutorialSystemRoot != null)
                tutorialSystemRoot.SetActive(isTutorial);

            if (isTutorial)
            {
                if (recetaTutorialObjetivo != null)
                {
                    // Lista para mostrar en el tablón/libro (si lo usas)
                    var listaTutorial = Enumerable.Repeat(recetaTutorialObjetivo, 1).ToList();

                    // Tablón: pinta SOLO la receta del tutorial
                    if (rec != null) rec.ShowRecipes(listaTutorial);

                    // Objetivos: solo cuenta esa receta
                    recetasRestantes = listaTutorial
                                       .Where(r => r != null && !r.esIntermedia)
                                       .GroupBy(r => r)
                                       .ToDictionary(g => g.Key, g => g.Count());

                    recetasTotalesIniciales = recetasRestantes.Values.Sum();
                }

                if (contador != null)
                {
                    contador.OnTimeEnd -= OnGameOver; 
                }

                
                //if (rec != null)
                //    rec.ShowRecipes(new List<RecetaData> { recetaObjetivo });
                // if (rec != null) rec.ShowTutorialLayout(); 
            }
            else
            {
                SetAmbienceMusic(turnoActual);
                recetasToDo = CalcularRecetasTurno(nivelacionData.jornadas[jornadaActual].recetasAsignadas, tiempoPorTurnoTotal, Mathf.CeilToInt(tiempoPorTurnoTotal * 0.1f));

                if (rec != null)
                {
                    rec.ShowRecipes(recetasToDo);
                }
                else
                {
                    Debug.LogWarning("No se encontró el componente RecipeBoard en recetascolgadas.");
                }

                recetasRestantes = recetasToDo
                                   .Where(r => r != null && !r.esIntermedia)
                                   .GroupBy(r => r)
                                   .ToDictionary(g => g.Key, g => g.Count());

                foreach (var kv in recetasRestantes)
                    Debug.Log($"[Objetivo] {kv.Key.nombre} x{kv.Value}");

                recetasTotalesIniciales = recetasRestantes.Values.Sum();

                contador.SetTiempoInicial(tiempoPorTurnoTotal);
                contador.Reanudar();
                contador.OnTimeEnd += OnGameOver;
            }
        }
    }

    private void ButtonTabClicked()
    {
        Debug.Log("TabButton clickado");
        
        tablonButton.GetComponent<Draggable>().enabled = false;

        // Iniciar el movimiento
        StartCoroutine(MoverCamara(Camera.main.transform, cameraInitPos, 1f, () =>
        {
            tablon.GetComponent<Draggable>().enabled = true;
        }));
        NotifyTablonClosed();
        Debug.Log("TabButton clickado fin");
    }

    private void OnTabClicked()
    {
        Debug.Log("Tab clickado");  
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.KitchenVerTablonComandas, ""));
        EventRegister.Instance.EvntToJson();
        tablon.GetComponent<Draggable>().enabled = false;

        // Iniciar el movimiento
        StartCoroutine(MoverCamara(Camera.main.transform, cameraTablonPos, 1f, () =>
        {
            tablonButton.GetComponent<Draggable>().enabled = true;
        }));
        NotifyTablonOpened();
        Debug.Log("Tab clickado fin");
    }

    private IEnumerator MoverCamara(Transform obj, Transform destino, float duracion, Action onComplete)
    {
        Vector3 origenPos = obj.position;
        Quaternion origenRot = obj.rotation;

        Vector3 destinoPos = destino.position;
        Quaternion destinoRot = destino.rotation;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            float t = tiempo / duracion;

            // Suavizado tipo SmoothStep (ease-in/out). Alternativas abajo.
            float e = t * t * (3f - 2f * t);

            obj.position = Vector3.LerpUnclamped(origenPos, destinoPos, e);
            obj.rotation = Quaternion.SlerpUnclamped(origenRot, destinoRot, e);

            tiempo += Time.deltaTime;
            yield return null;
        }

        obj.position = destinoPos;
        obj.rotation = destinoRot;

        onComplete?.Invoke();
    }


    // Funcion que elige las recetas que se van a tener que preparar en el turno seleccionado
    public List<RecetaData> CalcularRecetasTurno(List<RecetaData> recetasDisponibles, int tiempoTurno, int margenInicial)
    {
        if (isTutorial && recetaTutorialObjetivo != null)
            return Enumerable.Repeat(recetaTutorialObjetivo, 1).ToList();

        int maxIntentos = 1000;
        int margen = margenInicial;
        System.Random rng = new System.Random();

        List<RecetaData> solucionValida = new List<RecetaData>();

        //Filtrado de intermedias
        recetasDisponibles = recetasDisponibles
        .Where(r => r != null && !r.esIntermedia)
        .ToList();

        // Si no hay recetas que entren en el tiempo del turno, usar la más corta
        if (recetasDisponibles.All(r => r.tiempo_est_segs > tiempoTurno))
        {
            RecetaData recetaMasCorta = recetasDisponibles
                .OrderBy(r => r.tiempo_est_segs)
                .First();

            return new List<RecetaData> { recetaMasCorta };
        }

        while (tiempoTurno - margen > 0)
        {
            int i = 0;
            bool solucionEncontrada = false;
            while (i < maxIntentos && !solucionEncontrada)
            {
                List<RecetaData> seleccionadas = new List<RecetaData>();
                int tiempoTotal = 0;

                while (tiempoTotal < tiempoTurno)
                {
                    // Ponderar aleatoriamente las recetas para fomentar la variedad
                    List<float> pesos = recetasDisponibles
                        .Select(r => 1f / (1 + seleccionadas.Count(s => s == r)))
                        .ToList();

                    float sumaPesos = pesos.Sum();
                    float valorAleatorio = (float)(rng.NextDouble() * sumaPesos);
                    float acumulador = 0f;

                    int index = 0;
                    for (; index < pesos.Count; index++)
                    {
                        acumulador += pesos[index];
                        if (valorAleatorio <= acumulador)
                            break;
                    }

                    RecetaData recetaElegida = recetasDisponibles[index];
                    if (tiempoTotal + recetaElegida.tiempo_est_segs > tiempoTurno)
                        break;

                    seleccionadas.Add(recetaElegida);
                    tiempoTotal += recetaElegida.tiempo_est_segs;
                }

                if (tiempoTotal >= tiempoTurno - margen && tiempoTotal <= tiempoTurno)
                {
                    Debug.Log($"[RecetasTurno] Solución encontrada en {i + 1} intentos. Margen usado: {margen}");
                    solucionValida = seleccionadas;
                    solucionEncontrada = true;
                }

                i++;
            }

            if (solucionEncontrada)
            {
                return solucionValida;
            }

            // Si no se encuentra solución válida, ampliar margen
            margen = Mathf.CeilToInt(margen * 1.5f);
        }


        Debug.LogWarning("Algo ha fallado en la eleccion de recetas de juego");
        return solucionValida;
    }

    public void CalcularRecetasPorJornada()
    {
        if (recetasDatabase == null || nivelacionData == null)
        {
            Debug.LogError("Faltan referencias a recetasDatabase o nivelacionData.");
            return;
        }

        foreach (JornadaData jornada in nivelacionData.jornadas)
        {
            Debug.Log($"Jornada '{jornada.nombreJornada}': {jornada.recetasAsignadas.Count} recetas asignadas.");
            foreach (RecetaData receta in jornada.recetasAsignadas)
            {
                Debug.Log(receta.nombre);
            }
        }

        Debug.Log("Recetas calculadas correctamente para todas las jornadas.");
    }

    public void RegisterDelivery(RecetaData receta)
    {
        if (receta == null) return;
        if (receta.esIntermedia) return; // las intermedias NO cuentan para victoria
        if (recetasRestantes == null || recetasRestantes.Count == 0)
        {
            Debug.LogWarning("[LevelKitchenManager] No hay objetivos activos para este turno.");
            return;
        }

        if (recetasRestantes.TryGetValue(receta, out int restantes))
        {
            if (restantes > 0)
            {
                recetasRestantes[receta] = restantes - 1;

                rec.ChangeTexture(receta.nombre);

                Debug.Log($"Entregado: {receta.nombre}. Restan {recetasRestantes[receta]}.");
                KitchenSoundManager.Instance.PlaySound(ObjetosSound.RecetaEntregada, "RecipeDelivered");
                string s = receta.nombre;
                EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.KitchenRecetaEntregada, s));
                EventRegister.Instance.EvntToJson();
                // ¿hemos cumplido todos los objetivos?
                if (recetasRestantes.Values.All(v => v <= 0))
                {
                    OnVictory();
                }
            }
            else
            {
                // Entrega extra/no solicitada (puedes ignorar o dar puntos bonus)
                Debug.Log($"Entrega extra no requerida: {receta.nombre}");
            }
        }
        else
        {
            // No estaba en los objetivos del turno (receta no pedida)
            string s = receta.nombre;
            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.KitchenRecetaErronea, s));
            EventRegister.Instance.EvntToJson();
            Debug.Log($"Receta no pedida: {receta.nombre}");
        }
    }

    private void OnVictory()
    {
        if (!isTutorial)
        {
            DraggableBlocker.Block();
            endCollider.SetActive(true);    

            EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.KitchenFinTurno, ""));
            EventRegister.Instance.EvntToJson();

            StopAmbienceMusic(turnoActual);
            KitchenSoundManager.Instance.PlayOneShotRaw(ObjetosSound.Win, "KitchenWin", 10.0f);
            contador.Pausar();
            winStats.Calculate(recetasTotalesIniciales - recetasRestantes.Values.Sum(), recetasTotalesIniciales, tiempoPorTurnoTotal, contador.GetTiempo(), NOpenedBook);
            if (turnoActual == turnoMaxDesbloqueado && jornadaActual == jornadaMaxDesbloqueada)
            {
                if (turnoActual == Turno.Manana) turnoMaxDesbloqueado = Turno.Tarde;
                else if (turnoActual == Turno.Tarde) turnoMaxDesbloqueado = Turno.Noche;
                else if (turnoActual == Turno.Noche) {
                    turnoMaxDesbloqueado = Turno.Manana;
                    jornadaMaxDesbloqueada++;
                }
            }
           
        }
    }

    private void OnGameOver()
    {
        DraggableBlocker.Block();
        endCollider.SetActive(true);
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.KitchenFinTurnoTiempo, ""));
        EventRegister.Instance.EvntToJson();
        StopAmbienceMusic(turnoActual);
        KitchenSoundManager.Instance.PlayOneShotRaw(ObjetosSound.GameOver, "KitchenGameOver", 10.0f);
        loseStats.Calculate(recetasTotalesIniciales - recetasRestantes.Values.Sum(), recetasTotalesIniciales, tiempoPorTurnoTotal, contador.GetTiempo(), NOpenedBook);
    }

    

    public IEnumerable<RecetaData> GetRecetasSeleccionadasActuales()
    {
        if (isTutorial)
        {
            if (recetaTutorialObjetivo != null)
                return Enumerable.Repeat(recetaTutorialObjetivo, 1);
            return Enumerable.Empty<RecetaData>();
        }
        return nivelacionData.jornadas[jornadaActual].recetasAsignadas;
    }


    public void StartHandFollow(Transform target, Vector3 grabWorldPoint, float dragDepth)
    {
        handFollowTarget = target;
        handRayDepth = dragDepth;
        handOffset = (target != null) ? (grabWorldPoint - target.position) : Vector3.zero;

        if (hand != null)
        {
            if (hideHandWhenIdle && !hand.activeSelf) hand.SetActive(true);
            hand.transform.position = grabWorldPoint;
        }

        // Cambiamos animación 
        AnimatorManager.Instance.ChangeAnimation(ObjetosAnim.Mano, "Closed", 0.1f);        
    }

    public void StopHandFollow()
    {
        // Cambiamos animación 
        AnimatorManager.Instance.ChangeAnimation(ObjetosAnim.Mano, "Open", 0.1f);        

        if (hideHandWhenIdle && hand != null)
            StartCoroutine(HideHandAfter(0.5f));

        handFollowTarget = null;
    }

    public void Pause(bool pause)
    {
        string estado = pause ? "Pausado" : "Reanudado";
        EventRegister.Instance.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.Pause, estado));
        EventRegister.Instance.EvntToJson();

        paused = pause;
        DraggableBlocker.ConmuteBlock(paused);
        pauseCollider.SetActive(paused);
        if (paused) contador.Pausar();
        else contador.Reanudar();
    }

    //public void ActivateGame()
    //{
    //    if (EventRegister.Instance)
    //    {
    //        EventRegister.Instance.AddInitialEvent(EventRegister.EventosInfo.Inicio, "nivel " + SceneLoader.Instance.getCurrentLevelId(EventRegister.TipoJuego.Cocina).ToString("00"), EventRegister.TipoJuego.Cocina);
    //        Debug.Log("se pudo iniciar el evento Inicio en KitchenLevelManager.");
    //    }
    //    else
    //    {
    //        Debug.Log("No se pudo iniciar el evento Inicio en KitchenLevelManager.");
    //    }  
    //}

    public void SetAmbienceMusic(Turno t)
    {
        if (t == Turno.Manana) KitchenSoundManager.Instance.PlayLoopFaded(ObjetosSound.AmbienceMorning, "MorningAmbience");
        else if( t == Turno.Tarde) KitchenSoundManager.Instance.PlayLoopFaded(ObjetosSound.AmbienceAfternoon, "AfternoonAmbience");
        else if (t == Turno.Noche) KitchenSoundManager.Instance.PlayLoopFaded(ObjetosSound.AmbienceNight, "NightAmbience");
    }

    public void StopAmbienceMusic(Turno t)
    {
        if (t == Turno.Manana) KitchenSoundManager.Instance.StopLoopFaded(ObjetosSound.AmbienceMorning);
        else if (t == Turno.Tarde) KitchenSoundManager.Instance.StopLoopFaded(ObjetosSound.AmbienceAfternoon);
        else if (t == Turno.Noche) KitchenSoundManager.Instance.StopLoopFaded(ObjetosSound.AmbienceNight);
    }

    private IEnumerator HideHandAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (hand != null) hand.SetActive(false);
    }

    void LateUpdate()
    {
        if (hand == null) return;

        if (handFollowTarget != null)
        {
            // Sigue al objeto con el mismo punto de agarre
            var targetPos = handFollowTarget.position + handOffset;
            hand.transform.position = targetPos;
        }
        else if (hand.activeSelf && mainCam != null)
        {             
            var ray = mainCam.ScreenPointToRay(Input.mousePosition);
            hand.transform.position = ray.GetPoint(handRayDepth);
        }
    }

    #region SettersGetters
    public void SetJornada(int newValue)
    {
        jornadaActual = newValue;
    }

    public void SetTurno(Turno newValue)
    {
        turnoActual = newValue;
    }

    public Turno GetTurno() { return turnoActual; }

    public void SetTiempoPorTurno(int newValue)
    {
        tiempoPorTurnoTotal = newValue;
    }


    public void SetTablon(GameObject t)
    {
        tablon = t;
    }

    public GameObject GetTablon() { return tablon; }

    public void SetTablonButton(GameObject tb)
    {
        tablonButton = tb;
    }

    public void SetHand(GameObject h)
    {
        hand = h;
    }

    public NivelacionData GetNivelacionData() {
        return nivelacionData;
    }

    public void SetContador(Reloj cont)
    {
        contador = cont;
    }
    public void SetCameraTablonPos(Transform tr)
    {
        cameraTablonPos = tr;
    }

    public void SetCameraInitPos(Transform tr)
    {
        cameraInitPos = tr;
    }

    public void SetRecetasColgadas(GameObject rc)
    {
        recetasColgadas = rc;
    }

    public Dictionary<RecetaData, int> GetRecetasRestantes()
    {
        return recetasRestantes;
    }

    public void SetStatsWin(CalculateStats cs)
    {
        winStats = cs;
    }

    public void SetStatsLose(CalculateStats cs)
    {
        loseStats = cs;
    }

    public void SetNOpenedBook(int ob)
    {
        NOpenedBook = ob;
    }

    public int GetNOpenedBook() { return NOpenedBook; }

    public void SetConveyor(GameObject c)
    {
        conveyor = c;
    }

    public GameObject GetConveyor()
    {
        return conveyor;
    }

    public void SetSponge(GameObject s)
    {
        sponge = s;
    }
    public GameObject GetSponge() 
    {
        return sponge; 
    }

    public void SetPauseCollider(GameObject p)
    {
        pauseCollider = p;
    }
    public void SetEndCollider(GameObject e)
    {
        endCollider = e;
    }
    public GameObject GetPauseCollider()
    {
        return pauseCollider;
    }

    public void SetTutorial(GameObject t)
    {
        tutorialSystemRoot = t;
    }
    public void StartTutorialMode() => isTutorial = true;
    public void StopTutorialMode() => isTutorial = false;
    public bool GetTutorial() {  return isTutorial; }

    public bool IsPaused() { return paused; }

    public Dictionary<string, RecetaSprites> GetRecetasSprites() { return recetasSprites; }
    public Dictionary<Ingredientes, Sprite> GetIngredientesSprites() { return ingredientesSprites; }

    public int GetJornadaMaxDesbloqueada() { return jornadaMaxDesbloqueada; }
    public Turno GetTurnoMaxDesbloqueado() { return turnoMaxDesbloqueado; }

    public void NotifyBookOpened() => OnBookOpenedTutorial?.Invoke();
    public void NotifyTablonOpened() => OnTablonOpenedTutorial?.Invoke();
    public void NotifyBookClosed() => OnBookClosedTutorial?.Invoke();
    public void NotifyTablonClosed() => OnTablonClosedTutorial?.Invoke();
    #endregion
}
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

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

    private string[] escenasPermitidas = { "KitchenLevel", "KitchenLevelSelector", "KitchenBalanceTerapeuta" };

    [Header("Configuración")]
    [SerializeField] private RecetasDatabase recetasDatabase;
    [SerializeField] private NivelacionData nivelacionData;

    private GameObject libroDeRecetas;

    private int jornadaActual = 1; //nivelacionData.jornadas[jornadaActual].recetasAsignadas
    private Turno turnoActual = Turno.Manana;
    private int tiempoPorTurnoTotal;
    private List<RecetaData> recetasToDo;

    private Transform bookTargetTransform;
    private float moveDuration = 1.5f;
    private GameObject[] lights;

    private Reloj contador;


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
    }

    private void Start()
    {
        CalcularRecetasPorJornada();
    }

    private void Update()
    {
        
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
        }

        if (scene.name == escenasPermitidas[0]) // KitchenLevel
        {
            Draggable input = libroDeRecetas.GetComponent<Draggable>();

            if (input != null)
            {
                input.onStartDragging.AddListener(OnBookClicked);
            }
            else
            {
                Debug.LogWarning("No se encontró el componente OnMouseInputRecieved.");
            }

            recetasToDo = CalcularRecetasTurno(nivelacionData.jornadas[jornadaActual].recetasAsignadas, tiempoPorTurnoTotal, Mathf.CeilToInt(tiempoPorTurnoTotal * 0.1f));

            foreach (RecetaData receta in recetasToDo)
            {
                Debug.Log("RECETA TO DO: " + receta.nombre);
            }
            /*foreach (RecetaData receta in nivelacionData.jornadas[jornadaActual].recetasAsignadas)
            {
                Debug.Log("RECETA JORNADA: " + receta.nombre);
            }*/

            contador.SetTiempoInicial(tiempoPorTurnoTotal);
            contador.Reanudar();
        }
    }

    private void OnBookClicked()
    {
        Debug.Log("Libro clickado");
        libroDeRecetas.GetComponent<Draggable>().enabled = false;

        bookTargetTransform = GameObject.Find("LibroPos").transform;
        AnimatorManager.Instance.PlayAndPauseAt(ObjetosAnim.Libro, "Open", 0.8f);

        foreach (GameObject l in lights)
        {
            l.SetActive(true);
        }
        
        // Iniciar el movimiento con rotación
        StartCoroutine(MoverLibro(libroDeRecetas.transform, bookTargetTransform.position, bookTargetTransform.rotation, moveDuration));
    }

    private IEnumerator MoverLibro(Transform objeto, Vector3 destinoPos, Quaternion destinoRot, float duracion)
    {
        Vector3 origenPos = objeto.position;
        Quaternion origenRot = objeto.rotation;

        float tiempo = 0f;

        while (tiempo < duracion)
        {
            float t = tiempo / duracion;
            objeto.position = Vector3.Lerp(origenPos, destinoPos, t);
            objeto.rotation = Quaternion.Slerp(origenRot, destinoRot, t);
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Asegurar posición/rotación final
        objeto.position = destinoPos;
        objeto.rotation = destinoRot;
    }


    // Funcion que elige las recetas que se van a tener que preparar en el turno seleccionado
    public List<RecetaData> CalcularRecetasTurno(List<RecetaData> recetasDisponibles, int tiempoTurno, int margenInicial)
    {
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

    public IEnumerable<RecetaData> GetRecetasSeleccionadasActuales()
    {
        return nivelacionData.jornadas[jornadaActual].recetasAsignadas;
    }

    public void SetJornada(int newValue)
    {
        jornadaActual = newValue;
    }

    public void SetTurno(Turno newValue)
    {
        turnoActual = newValue;
    }

    public void SetTiempoPorTurno(int newValue)
    {
        tiempoPorTurnoTotal = newValue;
    }

    public GameObject GetLibro()
    {
        return libroDeRecetas;
    }

    public void SetLibro(GameObject l)
    {
        libroDeRecetas = l;
    }

    public void SetLights(GameObject[] ls)
    {
        lights = ls;
    }

    public NivelacionData GetNivelacionData() {
        return nivelacionData;
    }

    public void SetContador(Reloj cont)
    {
        contador = cont;
    }
}
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.Collections;

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
    Agua, Aguacate, Aji, Arepa, Arroz, Azucar, Cafe, Canela, Carne, CarneMolida, Cebolla, Cilantro, Coco, CremaDeLeche, Frijoles, Fresa, Frutas, Guayaba, Harina,
    Hojas, Huevo, Hueso, Leche, Limon, Maiz, Mariscos, Miel, MixVegetales, Panela, Papa, Pez, Platano, Pollo, Queso, Tomate, Viche, Yuca
}

public class LevelKitchenManager : MonoBehaviour
{
    private static LevelKitchenManager _instance = null;

    static public LevelKitchenManager Instance { get { return _instance; } }

    private string[] escenasPermitidas = { "KitchenLevel", "KitchenLevelSelector", "KitchenBalanceTerapeuta" };

    [Header("Configuración")]
    public RecetasDatabase recetasDatabase;
    public NivelacionData nivelacionData;

    private GameObject libroDeRecetas;

    private int jornadaActual = 1;

    private Transform bookTargetTransform;
    private float moveDuration = 1.5f;
    private string openAnimation = "Open";
    private GameObject[] lights;


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
            Destroy(SoundManager.Instance.gameObject);
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

            
        }
    }

    private void OnBookClicked()
    {
        Debug.Log("Libro clickado");
        libroDeRecetas.GetComponent<Draggable>().enabled = false;

        bookTargetTransform = GameObject.Find("LibroPos").transform;
        AnimatorManager.Instance.PlayAndPauseAt(ObjetosAnim.Libro, openAnimation, 0.8f);

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

    public void SetJornada(int newValue)
    {
        jornadaActual = newValue;
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
}
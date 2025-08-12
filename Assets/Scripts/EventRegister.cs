using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EventRegister : MonoBehaviour
{
    [Header("Escritura de los jsons")]
    [SerializeField] private string WriteDir = "writtenInfo";
    [SerializeField] private string WritePath = "test";
    private string WriteTo = "";
    private bool canWrite;

    private List<Tuple<EventRegister.EventosInfo, string>> auxEvntInfo;

    [SerializeField]
    int whitePixelsFramesDuation = 10;
    int frameCont = 0;
    bool whitePixelsActive;

    [SerializeField]
    private GameObject whitePixels = null;


    //esto es para guardar si has introducido ya el nombre del paciente/terapeuta en PacienteConfig.cs
    private bool pacientInfoIsRegistered;
    public bool PacientInfoIsRegistered
    {
        get => pacientInfoIsRegistered;
        set => pacientInfoIsRegistered = value;
    }

    public enum TipoJuego
    {
        DefaultGame,
        Delfines,
        MisionColombia
    }

    private InfoSesion infoSesion;

    public InfoSesion GetInfoSesion()
    {
        return infoSesion;
    }

    public void SetInfoSesion(InfoSesion value)
    {
        infoSesion = value;
        PacientInfoIsRegistered = true; //se pone a true el bool de que se ha registrado

    }

    //infosesion esta hecho para crear el nombre del archivo, que sera "{paciente}-{terapeuta}-{juego}-{fechaStr}.json"
    public struct InfoSesion
    {
        public string nombrePaciente;
        public string nombreTerapeuta;
        public TipoJuego nombreJuego;
        public DateTime fechaHora;

        public InfoSesion(string paciente, string terapeuta, TipoJuego nombreJuego)
        {
            this.nombrePaciente = paciente;
            this.nombreTerapeuta = terapeuta;
            this.nombreJuego = nombreJuego;
            this.fechaHora = DateTime.UtcNow;
        }

        
    }

    public enum EventosInfo
    {                       //implementado en...
        Inicio, //...DolphinLevelManager.InitLevel
        DEntraSuperficie, //...DolphinController.Float
        DSaleSuperficie, //...DolphinController.Dive
        DSaltoInit, //...DolphinControler.Jump
        DSaltoFin,//...DolphinControler.OnAnimationEnded
        DPiruetaInit, //...DolphinControler.SpecialJump
        DPiruetaFin, //...DolphinControler.OnAnimationEnded
        OEntraPantalla, //...RandomObjectSpawner.Spawn
        OSalePantalla,
        OColision, //...DolphinControler.OnCollisionEnter
        FEntraPantalla, //...RandomObjectSpawner.Spawn
        FSalePantalla,
        BEntraPantalla, //...BallSpawner.Spawn
        BTocado,
        BHundido,
        FColision,  //...DolphinControler.OnCollisionEnter
        RespuestaCorrecta, //...DolphinControler.TryClickDolphin y DolphinLevelManager.RightGuess
        RespuestaIncorrecta, //...DolphinControler.TryClickDolphin y DolphinLevelManager.WrongGuess
        NPuntos, //...DolphinControler.TryClickDolphin, DolphinLevelManager.RightGuess y DolphinLevelManager.WrongGuess
        Vel,
        Fin,
        PacienteInfo //...PacienteConfig.OnAceptarClicked o en este mismo usando addPacienteInfoEvent
    }

    static private EventRegister _instance;
    public static EventRegister Instance { get { return _instance; } }

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (whitePixels == null)
        {
            Debug.Log("No hay white pixels");
        }

        whitePixels.SetActive(false);
        whitePixelsActive = false;
    }

    private void Update()
    {
        if (whitePixelsActive)
        {
            if (frameCont < whitePixelsFramesDuation)
                frameCont++;
            else
            {
                whitePixels.SetActive(false);
                whitePixelsActive = false;
                frameCont = 0;
            }
        }
    }

    //hecho post juego porque convenia iniciar en otra parte
    public void AddInitialEventSafe(EventosInfo evento, string info)
    {
        if (!canWrite)
        {
            WriteStart(); // Inicia si no está iniciado
        }

        AddToEvnt(new Tuple<EventosInfo, string>(evento, info));
        EvntToJson(); // lo escribe ya directamente
    }

    //hecho post juego porque convenia iniciar en otra parte
    //ESTE ES EL METODO QUE HAY QUE USAR AL EMPEZAR TU JUEGO PARA HACER EL EVENTO DE INICIO
    public void AddInitialEvent(EventosInfo evento, string info, TipoJuego juego)
    {
        addInitialPacienteInfoEvent(juego); //mete la primera linea de la info paciente
        AddToEvnt(new Tuple<EventosInfo, string>(evento, info));
        EvntToJson(); // lo escribe ya directamente
    }

    //se va a usar al empezar a escribir (que tiene que ser cuando el jugador entra a un juego y se haga set del nombreJuego taambien)
    //para que este al principio del json
    public void addInitialPacienteInfoEvent(TipoJuego juego)
    {
        infoSesion.nombreJuego = juego;
        WritePath = GetFileNameFromInfoSesion(infoSesion);
        AddInitialEventSafe(EventosInfo.PacienteInfo, $"Paciente: {infoSesion.nombrePaciente}, Terapeuta: {infoSesion.nombreTerapeuta}");

    }

    public void WriteStart()
    {

        WriteEnd();//cerramos archivo si habia alguno abierto

        auxEvntInfo = new List<Tuple<EventRegister.EventosInfo, string>>();
        CreateDir();

        // Quitar extensión por si WritePath viene con .json de un uso anterior
        WritePath = System.IO.Path.GetFileNameWithoutExtension(WritePath);

        IncrementPath();
        WriteTo = System.IO.Path.Combine(WriteDir, WritePath);

        Debug.Log($"nuevo path: {WriteTo}");
        System.IO.StreamWriter file = new System.IO.StreamWriter(WriteTo);
        file.WriteLine("{ " + $"\"{WritePath}\": [");
        file.Close();
        canWrite = true;
    }
    private void CreateDir()
    {
        WriteDir = System.IO.Path.Combine(Application.persistentDataPath, WriteDir);
        if (!System.IO.Directory.Exists(WriteDir))
        {
            System.IO.Directory.CreateDirectory(WriteDir);
        }
    }

    private void IncrementPath()
    {
        int it = 1;

        // Nombre base sin extensión y sin corchetes
        string baseName = System.IO.Path.GetFileNameWithoutExtension(WritePath);
        int bracketIndex = baseName.IndexOf('[');
        if (bracketIndex >= 0)
            baseName = baseName.Substring(0, bracketIndex);

        string candidate = baseName + ".json";

        // Mientras exista, generamos [01], [02]...
        while (System.IO.File.Exists(System.IO.Path.Combine(WriteDir, candidate)) && it < 100)
        {
            candidate = $"{baseName}[{it:00}].json";
            it++;
        }

        WritePath = candidate; // Esto ya incluye la extensión .json
    }

    public void AddToEvnt(Tuple<EventRegister.EventosInfo, string> evntData)
    {
        auxEvntInfo.Add(evntData);
    }

    public void EvntToJson()
    {
        // White pixels
        whitePixels.SetActive(true);
        whitePixelsActive = true;

        // Events
        if (canWrite)
        {
            System.IO.FileStream fs = new System.IO.FileStream(WriteTo, System.IO.FileMode.Append, System.IO.FileAccess.Write);
            string text = "{\n" + "    \"Tiempo\": \"" + DateTime.UtcNow.AddHours(-5).ToString("yyyy-MM-dd HH:mm:ss.fff") + "\"";

            foreach (var evento in auxEvntInfo)
            {
                switch (evento.Item1)
                {
                    case EventosInfo.Inicio:
                        text += ", \n" + $"    \"Iniciando nivel\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.DEntraSuperficie:
                        text += ", \n" + $"    \"Delfin entrando en la superficie del rio (a flote)\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.DSaleSuperficie:
                        text += ", \n" + $"    \"Delfin saliendo de la superficie del rio (se hunde)\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.DSaltoInit:
                        text += ", \n" + $"    \"Delfin comienza el salto\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.DSaltoFin:
                        text += ", \n" + $"    \"Delfin finaliza el salto\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.DPiruetaInit:
                        text += ", \n" + $"    \"Delfin comienza la pirueta\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.DPiruetaFin:
                        text += ", \n" + $"    \"Delfin finaliza la pirueta\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.OEntraPantalla:
                        text += ", \n" + $"    \"Obstaculo entrando en pantalla\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.OSalePantalla:
                        text += ", \n" + $"    \"Obstaculo saliendo de pantalla\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.OColision:
                        text += ", \n" + $"    \"Delfin colisionando con obstaculo\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.FEntraPantalla:
                        text += ", \n" + $"    \"Flotador entrando en pantalla\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.FSalePantalla:
                        text += ", \n" + $"    \"Flotador saliendo de pantalla\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.FColision:
                        text += ", \n" + $"    \"Delfin colisionando con flotador\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.BEntraPantalla:
                        text += ", \n" + $"    \"Pelota entrando en pantalla.\"";
                        break;
                    case EventosInfo.BTocado:
                        text += ", \n" + $"    \"Jugador toca la pelota.\"";
                        break;
                    case EventosInfo.BHundido:
                        text += ", \n" + $"    \"Pelota hundida.\"";
                        break;
                    case EventosInfo.RespuestaCorrecta:
                        text += ", \n" + $"    \"Respuesta correcta\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.RespuestaIncorrecta:
                        text += ", \n" + $"    \"Respuesta incorrecta\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.NPuntos:
                        text += ", \n" + $"    \"Puntuacion actual\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.Vel:
                        text += ", \n" + $"    \"Velocidad actual\": \"{evento.Item2}\"";
                        break;
                    case EventosInfo.PacienteInfo:
                        text += ", \n" + $"    \"Paciente y terapeuta\": \"{evento.Item2}\"";
                        break;
                }
            }
                    
            text += " \n}, ";

            System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
            file.WriteLine(text);
            file.Close();
            fs.Close();
        }
        
        auxEvntInfo.Clear();
    }

    public void WriteEnd()
    {
        if (!canWrite) return; //si no está empezada la escritura que tampoco pueda finalizarse

        Debug.Log("Escribiendo fin del json.");
        System.IO.FileStream fs = new System.IO.FileStream(WriteTo, System.IO.FileMode.Append, System.IO.FileAccess.Write);
        string text = "{\n" + "    \"Time\": \"" + DateTime.UtcNow.AddHours(-5).ToString("yyyy-MM-dd HH:mm:ss.fff") + "\" , \n    \"Test\": \"Acabado\" } ]}";
        System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
        file.WriteLine(text);
        file.Close();
        fs.Close();
        canWrite = false;

        // Restaurar base sin extensión para el próximo uso
        WritePath = System.IO.Path.GetFileNameWithoutExtension(WritePath);
    }

    //para sacar el nombre del archivo segun los datos
    public string GetFileNameFromInfoSesion(InfoSesion infoS)
    {
        string pac = infoS.nombrePaciente.Replace(" ", "_");
        string ter = infoS.nombreTerapeuta.Replace(" ", "_");
        string juego = infoS.nombreJuego.ToString().Replace(" ", "_"); //enum a string

        string fechaStr = infoS.fechaHora.ToString("yyyy-MM-dd-HH-mm");

        return $"{pac}-{ter}-{juego}-{fechaStr}.json";
    }


    void OnApplicationQuit()
    {
        WriteEnd();
    }
}

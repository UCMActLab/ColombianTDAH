using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
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

    private TipoJuego currentGamePlaying;
    private bool pacientInfoIsRegistered;
    public TipoJuego CurrentGamePlaying { get => currentGamePlaying; set => currentGamePlaying = value; }
    public bool PacientInfoIsRegistered { get => pacientInfoIsRegistered; set => pacientInfoIsRegistered = value; }

    private InfoSesion infoSesion;

    public enum TipoJuego
    {
        DefaultGame,
        Delfines,
        MisionColombia
    }


    public InfoSesion GetInfoSesion()
    {
        return infoSesion;
    }

    public void SetInfoSesion(InfoSesion value)
    {
        infoSesion = value;
        PacientInfoIsRegistered = true; //se pone a true el bool de que se ha registrado
        //cleanstring
    }

    //infosesion esta hecho para crear el nombre del archivo, que sera "$"{id}_{juego}_{fechaStr}_{numSesion}.json"
    public struct InfoSesion
    {
        public string idPaciente;
        public string numeroSesion;
        public TipoJuego nombreJuego;
        public DateTime fechaHora;

        public InfoSesion(string paciente, string numSesion, TipoJuego nombreJuego)
        {
            this.idPaciente = paciente;
            this.numeroSesion = numSesion;
            this.nombreJuego = nombreJuego;
            this.fechaHora = DateTime.UtcNow.AddHours(-5);
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
        PacienteInfo, //...PacienteConfig.OnAceptarClicked o en este mismo usando addPacienteInfoEvent
        Pause,  //...DolphinLevelManager.Pause
        ChangeDifficulty //...DolphinLevelManager.ActivateIncreasedSpeed/DeactivateIncreasedSpeed
    }

    private Dictionary<EventosInfo, string> EventoMensajes = new()
    {
        { EventosInfo.Inicio, "Iniciando nivel" },
        { EventosInfo.DEntraSuperficie, "Delfin entrando en la superficie del rio (a flote)" },
        { EventosInfo.DSaleSuperficie, "Delfin saliendo de la superficie del rio (se hunde)" },
        { EventosInfo.DSaltoInit, "Delfin comienza el salto" },
        { EventosInfo.DSaltoFin, "Delfin finaliza el salto" },
        { EventosInfo.DPiruetaInit, "Delfin comienza la pirueta" },
        { EventosInfo.DPiruetaFin, "Delfin finaliza la pirueta" },
        { EventosInfo.OEntraPantalla, "Obstaculo entrando en pantalla" },
        { EventosInfo.OSalePantalla, "Obstaculo saliendo de pantalla" },
        { EventosInfo.OColision, "Delfin colisionando con obstaculo" },
        { EventosInfo.FEntraPantalla, "Flotador entrando en pantalla" },
        { EventosInfo.FSalePantalla, "Flotador saliendo de pantalla" },
        { EventosInfo.FColision, "Delfin colisionando con flotador" },
        { EventosInfo.BEntraPantalla, "Pelota entrando en pantalla." },
        { EventosInfo.BTocado, "Jugador toca la pelota." },
        { EventosInfo.BHundido, "Pelota hundida." },
        { EventosInfo.RespuestaCorrecta, "Respuesta correcta" },
        { EventosInfo.RespuestaIncorrecta, "Respuesta incorrecta" },
        { EventosInfo.NPuntos, "Puntuacion actual" },
        { EventosInfo.Vel, "Velocidad actual" },
        { EventosInfo.PacienteInfo, "Paciente y numero de sesion" },
        { EventosInfo.Pause, "Juego ha sido pausado o reanudado" },
        { EventosInfo.ChangeDifficulty, "Cambio de dificultad" },
    };


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
    //ESTE ES EL METODO QUE HAY QUE USAR AL EMPEZAR TU JUEGO PARA HACER EL EVENTO DE INICIO
    public void AddInitialEvent(EventosInfo evento, string info, TipoJuego juego)
    {
        currentGamePlaying = juego;
        AddInitialPacienteInfoEvent(juego); //mete la primera linea de la info paciente
        AddToEvnt(new Tuple<EventosInfo, string>(evento, info));
        EvntToJson(); // lo escribe ya directamente
    }

    //se va a usar al empezar a escribir (que tiene que ser cuando el jugador entra a un juego y se haga set del nombreJuego taambien)
    //para que este al principio del json
    private void AddInitialPacienteInfoEvent(TipoJuego juego)
    {
        infoSesion.nombreJuego = juego;
        WritePath = GetFileNameFromInfoSesion(infoSesion);

        if (!canWrite)
        {
            WriteStart(); // Inicia si no está iniciado
        }
        else
            return; //si ya esta escribiendo que no vuelva a poner este evento

        AddToEvnt(new Tuple<EventosInfo, string>(EventosInfo.PacienteInfo, $"Paciente: {infoSesion.idPaciente}, Numero sesion: {infoSesion.numeroSesion}"));
        EvntToJson(); // lo escribe ya directamente
    }

    public void WriteStart()
    {

        WriteEnd();//cerramos archivo si habia alguno abierto

        auxEvntInfo = new List<Tuple<EventRegister.EventosInfo, string>>();
        CreateDir();

        // Quitar extensión por si WritePath viene con .json de un uso anterior
        WritePath = System.IO.Path.GetFileNameWithoutExtension(WritePath);

        IncrementPath();
        //  extensión .json 
       // WritePath += ".json";


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

        //string candidate = baseName + ".json";
        string candidate = $"{baseName}.json"; // Empieza directamente en [01]

        // Mientras exista, generamos [01], [02]...
        while (System.IO.File.Exists(System.IO.Path.Combine(WriteDir, candidate)) && it < 100)
        {
            candidate = $"{baseName}_{it:00}.json";
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


            string text = "{\n" +
               $"    \"Tiempo\": \"{DateTime.UtcNow.AddHours(-5):yyyy-MM-dd HH:mm:ss.fff}\",\n" +
               "    \"Eventos\": [\n        "; //vamos a poner los eventos en un array por si hay dos o mas eventos del mismo tipo a la vez no tener claves duplicadas

            List<string> eventosJson = new List<string>();

            foreach (var evento in auxEvntInfo)
            {
                if (EventoMensajes.TryGetValue(evento.Item1, out string mensaje))
                {
                    eventosJson.Add($"{{ \"{mensaje}\": \"{evento.Item2}\" }}"); //algunos item2 (mensaje extra) estan vacios pero no afecta
                }
            }

            text += string.Join(",\n        ", eventosJson);
            text += "]\n},";

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
        string id = infoS.idPaciente;
        string numSesion = infoS.numeroSesion;
        string juego = infoS.nombreJuego.ToString(); // enum a string
        string fechaStr = infoS.fechaHora.ToString("yyyy-MM-dd");


        return $"{id}_{juego}_{fechaStr}_{numSesion}.json";
    }

    //no sé si prefiero avisar de que no pongan cosas raras porque sera nombre de archivo o hacer esto xd
    private string CleanString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        // 1. Quitar tildes y acentos
        string normalized = input.Normalize(System.Text.NormalizationForm.FormD);
        StringBuilder sb = new StringBuilder();

        foreach (char c in normalized)
        {
            System.Globalization.UnicodeCategory uc = System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c);
            if (uc != System.Globalization.UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        string cleanInput = sb.ToString();

        // 2. Sustituir ñ por n
        cleanInput = cleanInput.Replace('ñ', 'n').Replace('Ñ', 'N');

        // 3. Quitar caracteres invalidos del sistema de archivos
        char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
        foreach (char invalidChar in invalidChars)
        {
            cleanInput = cleanInput.Replace(invalidChar.ToString(), "");
        }

        // 4. Reemplazar espacios y puntos por "_"
        cleanInput = cleanInput.Replace(" ", "_").Replace(".", "_");

        cleanInput = cleanInput.Replace("@", "a");

        // 5. Filtrar solo letras, numeros y "_-"
        StringBuilder finalSb = new StringBuilder();
        foreach (char c in cleanInput)
        {
            //  @, +, `, ^, &, etc se descarta, que esos no cuentan como caracteres raros en windows y todavia siguen
            if (char.IsLetterOrDigit(c) || c == '_' || c == '-')
            {
                finalSb.Append(c);
            }
        }

        return finalSb.ToString();
    }

    void OnApplicationQuit()
    {
        WriteEnd();
    }


    public string GetPatientID()
    {
        return infoSesion.idPaciente;
    }
}

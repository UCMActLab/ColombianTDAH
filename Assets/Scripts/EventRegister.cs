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
    private bool firstEventWritten = false;

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
        MisionColombia,
        Cocina
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
        public string nombrePaciente;
        public string idPaciente;
        public string numeroSesion;
        public TipoJuego nombreJuego;
        public DateTimeOffset fechaHora;

        public InfoSesion(string nombrePaciente, string id, string numSesion, TipoJuego nombreJuego)
        {
            this.nombrePaciente = nombrePaciente;
            this.idPaciente = id;
            this.numeroSesion = numSesion;
            this.nombreJuego = nombreJuego;
            this.fechaHora = NowBogota();
        }

        
    }

    public enum EventosInfo
    {                       //implementado en...
        Inicio, //...DolphinLevelManager.InitLevel , en MisionLevelManager.ActivateGame y en LevelKitchenManager.ActivateGame

        //delfines
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

        // generales
        PacienteInfo, //...PacienteConfig.OnAceptarClicked o en este mismo usando addPacienteInfoEvent
        Pause,  //...DolphinLevelManager.Pause, LevelKitchenManager.Pause
        ChangeDifficulty, //...DolphinLevelManager.ActivateIncreasedSpeed/DeactivateIncreasedSpeed

        // MC
        MCEmpiezaDecision, //misionlevelmanager.showdecisionsbuttons
        MCTerminaDecision, //misionlevelmanager.hidedecisionsbuttons yes click, no click
        MCPlanifcacionGuardada,
        MCHoraSalidaSeleccionada,
        MCPreguntaParada,
        MCRespuestaParadaSi,
        MCRespuestaParadaNo,
        MCPreguntaDormir,
        MCRespuestaDormirSi,
        MCRespuestaDormirNo,
        MCComienzoDormir,
        MCTerminoDormir,
        MCUbicacionEnviada,
        MCUbicacionEnviadaBien,
        MCUbicacionOmisionEnvio,
        MCPreguntaSinRespuesta,
        MCReglasCumplidas,
        MCReglasIncumplidas,

        // Cocina
        KitchenGuardarConfig,
        KitchenInicioTurno,
        KitchenFinTurno,
        KitchenVerTablonComandas,
        KitchenSeleccionarIngrediente,
        KitchenRegistrarAccion,
        KitchenConsultarLibro,
        KitchenRecetaCompletada,
        KitchenRecetaEntregada,
        KitchenRecetaErronea,
        KitchenFinTurnoTiempo,
        KitchenSeleccionJornadaTurno,
        KitchenLimpiezaEstacion
    }

    private Dictionary<EventosInfo, string> EventoMensajes = new()
    {
        { EventosInfo.Inicio, "Iniciando juego" },
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
        { EventosInfo.MCEmpiezaDecision, "Empieza pregunta y decision" },
        { EventosInfo.MCTerminaDecision, "Termina pregunta y decision" },
        { EventosInfo.MCPlanifcacionGuardada, "Planificacion guardada" },
        { EventosInfo.MCHoraSalidaSeleccionada, "Hora de salda seleccionada" },
        { EventosInfo.MCPreguntaParada, "Solicitud de parada" },
        { EventosInfo.MCRespuestaParadaSi, "Respuesta de parada SI" },
        { EventosInfo.MCRespuestaParadaNo, "Respuesta de parada NO" },
        { EventosInfo.MCPreguntaDormir, "Solicitud de dormir" },
        { EventosInfo.MCRespuestaDormirSi, "Respuesta de dormir SI" },
        { EventosInfo.MCRespuestaDormirNo, "Respuesta de dormir NO" },
        { EventosInfo.MCComienzoDormir, "Inicio de dormir" },
        { EventosInfo.MCTerminoDormir, "Final de dormir" },
        { EventosInfo.MCUbicacionEnviada, "Ubcacion enviada" },
        { EventosInfo.MCUbicacionEnviadaBien, "Ubicacion bien enviada" },
        { EventosInfo.MCUbicacionOmisionEnvio, "Envio de ubicación omitido" },
        { EventosInfo.MCPreguntaSinRespuesta, "Tiempo de respuesta excedido" },
        { EventosInfo.MCReglasCumplidas, "Se cumplen las reglas" },
        { EventosInfo.MCReglasIncumplidas, "Se incumplen las reglas" },
        { EventosInfo.KitchenGuardarConfig, "Configuración de partida guardada" },
        { EventosInfo.KitchenInicioTurno, "Inicio del turno" },
        { EventosInfo.KitchenFinTurno, "Fin del turno" },
        { EventosInfo.KitchenVerTablonComandas, "Consulta del tablón de comandas" },
        { EventosInfo.KitchenSeleccionarIngrediente, "Ingrediente agarrado" },
        { EventosInfo.KitchenRegistrarAccion, "Acción completada" },
        { EventosInfo.KitchenConsultarLibro, "Consulta del libro de recetas" },
        { EventosInfo.KitchenRecetaCompletada, "Receta completada" },
        { EventosInfo.KitchenRecetaEntregada, "Receta entregada correctamente" },
        { EventosInfo.KitchenRecetaErronea, "Receta entregada erroneamente" },
        { EventosInfo.KitchenFinTurnoTiempo, "Turno finalizado por tiempo" },
        { EventosInfo.KitchenSeleccionJornadaTurno, "Jornada/Turno seleccionados" },
        { EventosInfo.KitchenLimpiezaEstacion, "Estación limpiada" }

    };

    //array de bools con los eventos que activan los whitepixels
    private static bool[] whitePixelEvents = new bool[Enum.GetValues(typeof(EventosInfo)).Length];
    private void SetWhitePixelEvents()
    {
        whitePixelEvents[(int)EventosInfo.Inicio] = false;
        whitePixelEvents[(int)EventosInfo.DEntraSuperficie] = true;
        whitePixelEvents[(int)EventosInfo.DSaleSuperficie] = true;
        whitePixelEvents[(int)EventosInfo.DSaltoInit] = true;
        whitePixelEvents[(int)EventosInfo.DSaltoFin] = true;
        whitePixelEvents[(int)EventosInfo.DPiruetaInit] = true;
        whitePixelEvents[(int)EventosInfo.DPiruetaFin] = true;
        whitePixelEvents[(int)EventosInfo.OEntraPantalla] = true;
        whitePixelEvents[(int)EventosInfo.OSalePantalla] = true;
        whitePixelEvents[(int)EventosInfo.OColision] = true;
        whitePixelEvents[(int)EventosInfo.FEntraPantalla] = true;
        whitePixelEvents[(int)EventosInfo.FSalePantalla] = true;
        whitePixelEvents[(int)EventosInfo.BEntraPantalla] = true;
        whitePixelEvents[(int)EventosInfo.BTocado] = true;
        whitePixelEvents[(int)EventosInfo.BHundido] = true;
        whitePixelEvents[(int)EventosInfo.FColision] = true;
        whitePixelEvents[(int)EventosInfo.RespuestaCorrecta] = true;
        whitePixelEvents[(int)EventosInfo.RespuestaIncorrecta] = true;
        whitePixelEvents[(int)EventosInfo.NPuntos] = true;
        whitePixelEvents[(int)EventosInfo.Vel] = true;
        whitePixelEvents[(int)EventosInfo.Fin] = false;
        whitePixelEvents[(int)EventosInfo.PacienteInfo] = false;
        whitePixelEvents[(int)EventosInfo.Pause] = true;
        whitePixelEvents[(int)EventosInfo.ChangeDifficulty] = true;
        whitePixelEvents[(int)EventosInfo.MCEmpiezaDecision] = true;
        whitePixelEvents[(int)EventosInfo.MCPlanifcacionGuardada] = true;
        whitePixelEvents[(int)EventosInfo.MCHoraSalidaSeleccionada] = true;
        whitePixelEvents[(int)EventosInfo.MCPreguntaParada] = true;
        whitePixelEvents[(int)EventosInfo.MCRespuestaParadaSi] = true;
        whitePixelEvents[(int)EventosInfo.MCRespuestaParadaNo] = true;
        whitePixelEvents[(int)EventosInfo.MCPreguntaDormir] = true;
        whitePixelEvents[(int)EventosInfo.MCRespuestaDormirSi] = true;
        whitePixelEvents[(int)EventosInfo.MCRespuestaDormirNo] = true;
        whitePixelEvents[(int)EventosInfo.MCComienzoDormir] = true;
        whitePixelEvents[(int)EventosInfo.MCTerminoDormir] = true;
        whitePixelEvents[(int)EventosInfo.MCUbicacionEnviada] = true;
        whitePixelEvents[(int)EventosInfo.MCUbicacionEnviadaBien] = true;
        whitePixelEvents[(int)EventosInfo.MCUbicacionOmisionEnvio] = true;
        whitePixelEvents[(int)EventosInfo.MCPreguntaSinRespuesta] = true;
        whitePixelEvents[(int)EventosInfo.MCReglasCumplidas] = true;
        whitePixelEvents[(int)EventosInfo.MCReglasIncumplidas] = true;
        whitePixelEvents[(int)EventosInfo.KitchenGuardarConfig] = true;
        whitePixelEvents[(int)EventosInfo.KitchenInicioTurno] = true;
        whitePixelEvents[(int)EventosInfo.KitchenFinTurno] = true;
        whitePixelEvents[(int)EventosInfo.KitchenVerTablonComandas] = true;
        whitePixelEvents[(int)EventosInfo.KitchenSeleccionarIngrediente] = true;
        whitePixelEvents[(int)EventosInfo.KitchenRegistrarAccion] = true;
        whitePixelEvents[(int)EventosInfo.KitchenConsultarLibro] = true;
        whitePixelEvents[(int)EventosInfo.KitchenRecetaCompletada] = true;
        whitePixelEvents[(int)EventosInfo.KitchenRecetaEntregada] = true;
        whitePixelEvents[(int)EventosInfo.KitchenRecetaErronea] = true;
        whitePixelEvents[(int)EventosInfo.KitchenFinTurnoTiempo] = true;
        whitePixelEvents[(int)EventosInfo.KitchenSeleccionJornadaTurno] = true;
        whitePixelEvents[(int)EventosInfo.KitchenLimpiezaEstacion] = true;

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

        SetWhitePixelEvents();
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



    //ESTE ES EL METODO QUE HAY QUE USAR AL EMPEZAR TU JUEGO PARA HACER EL EVENTO DE INICIO y que se haga el de inicio y del paciente a la vez
    //ejemplo: EventRegister.Instance.AddInitialEvent(EventRegister.EventosInfo.Inicio, "nivel " + levelId.ToString("00"), EventRegister.TipoJuego.Delfines);
    public void AddInitialEvent(EventosInfo evento, string info, TipoJuego juego)
    {

        if (currentGamePlaying == juego && canWrite) //si se puede escribir y estamos en el mismo juego
        {
            //pone el evento de inicio del nivel
            AddToEvnt(new Tuple<EventosInfo, string>(evento, info));
            EvntToJson(); 
        }
        else
        {
          
            WriteEnd();//si entramos a un juego distinto terminamos la escritura y empezamos otro archivo
            currentGamePlaying = juego;
            AddInitialPacienteInfoEvent(juego); //mete la primera linea de la info paciente
            AddToEvnt(new Tuple<EventosInfo, string>(evento, info)); //esto es el evento de inicio que le tienes que pasar
            EvntToJson(); // lo escribe ya directamente
        }


            

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

        WriteEnd();// Cerramos archivo si habia alguno abierto

        auxEvntInfo = new List<Tuple<EventRegister.EventosInfo, string>>();
        CreateDir();

        // Quitar extension por si WritePath viene con .json de un uso anterior
        WritePath = System.IO.Path.GetFileNameWithoutExtension(WritePath);

        IncrementPath();

        WriteTo = System.IO.Path.Combine(WriteDir, WritePath);

        Debug.Log($"nuevo path: {WriteTo}");
        using (var file = new System.IO.StreamWriter(WriteTo))
        {
            string nombre = infoSesion.nombrePaciente ?? string.Empty;
            string docId = infoSesion.idPaciente ?? string.Empty; // documentoIdentidad
            string sesion = infoSesion.numeroSesion ?? string.Empty;
            string juego = infoSesion.nombreJuego.ToString();

            // fechaInicio = infoSesion.fechaHora (UTC)
            string fechaIni = infoSesion.fechaHora.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");

            file.WriteLine("{");
            file.WriteLine($"  \"nombrePaciente\": \"{Escape(nombre)}\",");
            file.WriteLine($"  \"documentoIdentidad\": \"{Escape(docId)}\",");
            file.WriteLine($"  \"numeroSesion\": \"{Escape(sesion)}\",");
            file.WriteLine($"  \"tipoJuego\": \"{Escape(juego)}\",");
            file.WriteLine($"  \"fechaInicio\": \"{fechaIni}\",");
            file.WriteLine($"  \"interacciones\": [");
        }

        firstEventWritten = false;
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

        // Nombre base sin extension y sin corchetes, ya se supone que no hay corchetes
        string baseName = System.IO.Path.GetFileNameWithoutExtension(WritePath);
        int bracketIndex = baseName.IndexOf('[');
        if (bracketIndex >= 0)
            baseName = baseName.Substring(0, bracketIndex);

        string candidate = $"{baseName}.json"; 

        // Mientras exista generamos 01, 02...
        while (System.IO.File.Exists(System.IO.Path.Combine(WriteDir, candidate)) && it < 100)
        {
            candidate = $"{baseName}_{it:00}.json";
            it++;
        }

        WritePath = candidate; // incluye la extension .json
    }

    public void AddToEvnt(Tuple<EventRegister.EventosInfo, string> evntData)
    {
        auxEvntInfo.Add(evntData);
        if (whitePixelEvents[(int)evntData.Item1]) // Si su correspondiente evento está a true activa los pixels
        {
            ActivateWhitePixels();
        }
    }

    public void EvntToJson()
    {

        if (!canWrite || auxEvntInfo == null || auxEvntInfo.Count == 0) return;

        using (var fs = new System.IO.FileStream(WriteTo, System.IO.FileMode.Append, System.IO.FileAccess.Write))
        using (var file = new System.IO.StreamWriter(fs))
        {
            // si ya hay eventos escritos, añadimos coma antes de los nuevos
            if (firstEventWritten)
                file.WriteLine(",");

            // todos los que haya en auxEvntInfo se escriben en bloque separados por coma
            for (int i = 0; i < auxEvntInfo.Count; i++)
            {
                var ev = auxEvntInfo[i];
                string tiempoIso = NowBogotaIso();
                string evento = EventoMensajes.TryGetValue(ev.Item1, out var msg) ? msg : ev.Item1.ToString();
                string detalle = ev.Item2 ?? string.Empty;

                file.Write("    {");
                file.Write($"\"tiempo\":\"{tiempoIso}\",\"evento\":\"{Escape(evento)}\",\"detalle\":\"{Escape(detalle)}\"");
                file.Write("}");

                if (i < auxEvntInfo.Count - 1)
                    file.WriteLine(",");
            }

            firstEventWritten = true;
        }

        auxEvntInfo.Clear();
    }

    void ActivateWhitePixels()
    {
        
        whitePixels.SetActive(true);
        whitePixelsActive = true;
    }
    public void WriteEnd()
    {
        if (!canWrite) return; // Si no está empezada la escritura que tampoco pueda finalizarse

        using (var fs = new System.IO.FileStream(WriteTo, System.IO.FileMode.Append, System.IO.FileAccess.Write))
        using (var file = new System.IO.StreamWriter(fs))
        {
            file.WriteLine();                      
            file.WriteLine("  ],");                
            string fechaFin = NowBogotaIso();
            file.WriteLine($"  \"fechaFin\": \"{fechaFin}\"");
            file.WriteLine("}");
        }
        canWrite = false;

        // volvemos a sin .json
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

    // Ahora mismo no se limpia la string del nombre del archivo para dar libertad
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

        // 2. Sustituir enye por n
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

        // 5. Quedarnos solo con letras, numeros y "_-"
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

    private static string Escape(string s)
    {
        return string.IsNullOrEmpty(s) ? string.Empty : s.Replace("\\", "\\\\").Replace("\"", "\\\"");
    }

    // Hora Colombia (Bogotá) 
    private static TimeZoneInfo _bogotaTz;

    private static TimeZoneInfo GetBogotaTz()
    {
        if (_bogotaTz != null) return _bogotaTz;

        // Intentamos primero ID de Unix (Android, macOS, Linux)
        try { _bogotaTz = TimeZoneInfo.FindSystemTimeZoneById("America/Bogota"); return _bogotaTz; } catch { }

        // Luego ID de Windows
        try { _bogotaTz = TimeZoneInfo.FindSystemTimeZoneById("SA Pacific Standard Time"); return _bogotaTz; } catch { }

        // Fallback: usamos UTC 
        _bogotaTz = TimeZoneInfo.Utc;
        return _bogotaTz;
    }

    private static DateTimeOffset NowBogota()
    {
        var tz = GetBogotaTz();
        return TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, tz);
    }

    private static string NowBogotaIso() => NowBogota().ToString("o"); // ISO 8601 con offset -05:00
}

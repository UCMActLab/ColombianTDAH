using UnityEngine;
using System;
using System.Collections.Generic;

public class EventRegister : MonoBehaviour
{
    [Header("Escritura de los jsons")]
    [SerializeField] private string WriteDir = "writtenInfo";
    [SerializeField] private string WritePath = "test";
    private string WriteTo = "";

    private List<Tuple<EventRegister.EventosInfo, string>> auxEvntInfo;

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
        FColision,  //...DolphinControler.OnCollisionEnter
        RespuestaCorrecta, //...DolphinControler.TryClickDolphin y DolphinLevelManager.RightGuess
        RespuestaIncorrecta, //...DolphinControler.TryClickDolphin y DolphinLevelManager.WrongGuess
        NPuntos, //...DolphinControler.TryClickDolphin, DolphinLevelManager.RightGuess y DolphinLevelManager.WrongGuess
        Vel,
        Fin
    }

    private static EventRegister _instance;

    public static EventRegister Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject instance = new GameObject("Event Register");
                instance.AddComponent<EventRegister>();
            }

            return _instance;
        }
    }

    void Awake()
    {
        _instance = this;
    }

    void WriteStart()
    {
        auxEvntInfo = new List<Tuple<EventRegister.EventosInfo, string>>();
        CreateDir();
        IncrementPath();
        WriteTo = System.IO.Path.Combine(WriteDir, WritePath);
        Debug.Log($"nuevo path: {WriteTo}");
        System.IO.StreamWriter file = new System.IO.StreamWriter(WriteTo);
        file.WriteLine("{ " + $"\"{WritePath}\": [");
        file.Close();
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
        string ogPath = WritePath;
        while (System.IO.File.Exists(System.IO.Path.Combine(WriteDir, WritePath) + ".json") && it < 100)
        {
            if (!WritePath.EndsWith("]"))
            {
                WritePath += "[01]";
            }
            else
            {
                WritePath = ogPath + "[" + it.ToString("00") + "]";
            }
            it++;
        }
        WritePath += ".json";
    }

    public void AddToEvnt(Tuple<EventRegister.EventosInfo, string> evntData)
    {
        auxEvntInfo.Add(evntData);
    }
    
    public void EvntToJson()
    {
        System.IO.FileStream fs = new System.IO.FileStream(WriteTo, System.IO.FileMode.Append, System.IO.FileAccess.Write);
        string text = "{\n" + "    \"Tiempo\": \"" + Time.realtimeSinceStartup.ToString("0.000") + "\"";

        foreach (var evento in auxEvntInfo)
        {
            switch (evento.Item1) { 
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
            }
        }

        auxEvntInfo.Clear();
        text += " \n}, ";
        System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
        file.WriteLine(text);
        file.Close();
        fs.Close();
    }

    public void WriteEnd()
    {
        System.IO.FileStream fs = new System.IO.FileStream(WriteTo, System.IO.FileMode.Append, System.IO.FileAccess.Write);
        string text = "{\n" + "    \"Time\": \"" + Time.realtimeSinceStartup.ToString("0.000") + "\" , \n    \"Test\": \"Acabado\" } ]}";
        System.IO.StreamWriter file = new System.IO.StreamWriter(fs);
        file.WriteLine(text);
        file.Close();
        fs.Close();
    }

    // Update is called once per frame
    void Update()
    {
    }
}

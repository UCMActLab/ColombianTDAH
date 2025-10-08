using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UINivelacionData : MonoBehaviour
{
    public NivelacionData nivelacionData;
    public RecetasDatabase recetasDatabase;

    private VisualElement root;
    private JornadaData jornadaActual;
    private int jornadaIndex = 0;

    private VisualElement puestosContainer;
    private Slider sliderTiempoManual;
    private Label labelTiempoManual;

    private Label labelDificultad;
    private ScrollView recetasScroll;
    private VisualElement toolbarJornadas;
    //private TextField fieldTerapeuta;
    //private TextField fieldPaciente;
    private Label labelTiempoTotal;
    private SliderInt sliderJornadasDesbloqueadas;
    private DropdownField dropdownTurnoDesbloqueado;
    private Label labelJornadasDesbloqueadas;

    private Toggle toggleFacil, toggleNormal, toggleDificil, toggleMuyDificil;
    private float[] valueToggles = { 1.5f, 1.0f, 0.75f, 0.5f };

    private bool useBase64 = true;


    private void OnEnable()
    {
        ActivateGame();

        root = GetComponent<UIDocument>().rootVisualElement;

        puestosContainer = root.Q<VisualElement>("puestos-container");
        sliderTiempoManual = root.Q<Slider>("slider-tiempo-manual");
        labelTiempoManual = root.Q<Label>("label-tiempo-manual");
        labelDificultad = root.Q<Label>("label-dificultad");
        recetasScroll = root.Q<ScrollView>("recetas-scroll");
        toolbarJornadas = root.Q<VisualElement>("toolbar-jornadas");
        //fieldTerapeuta = root.Q<TextField>("field-terapeuta");
        //fieldPaciente = root.Q<TextField>("field-paciente");
        labelTiempoTotal = root.Q<Label>("label-tiempo-total");
        sliderJornadasDesbloqueadas = root.Q<SliderInt>("levels-unlocked-slider");
        dropdownTurnoDesbloqueado = root.Q<DropdownField>("turno-dropdown");
        labelJornadasDesbloqueadas = root.Q<Label>("levels-unlocked-value");
        toggleFacil = root.Q<Toggle>("toggle-facil");
        toggleNormal = root.Q<Toggle>("toggle-normal");
        toggleDificil = root.Q<Toggle>("toggle-dificil");
        toggleMuyDificil = root.Q<Toggle>("toggle-muydificil");

        jornadaActual = nivelacionData.jornadas[0];

        for (int i = 0; i < 5; i++)
        {
            int index = i;
            var btn = toolbarJornadas.Q<Button>($"btn-j{i + 1}");
            btn.clickable.clicked += () =>
            {
                GetComponent<AudioSource>().Play();
                SeleccionarJornada(index);
            };
        }

        root.Q<Button>("btn-comenzar").clicked += () =>
        {
            for (int i = 0; i < 6; i++)
            {
                int x = i;
                if (i == 5) x = 0;
                SeleccionarJornada(x);

            }
            GetComponent<AudioSource>().Play();

            SendSavedConfig();           

            SceneLoader.LoadScene("KitchenLevelSelector");
        };


        sliderTiempoManual.RegisterValueChangedCallback(evt =>
        {
            jornadaActual.tiempoBaseManual = Mathf.RoundToInt(evt.newValue);
            ActualizarEtiquetasTiempo();
        });


        Action<Toggle, float> configurarDificultad = (toggle, factor) =>
        {
            toggle.RegisterValueChangedCallback(evt =>
            {

                if (evt.newValue)
                {
                    GetComponent<AudioSource>().Play();
                    jornadaActual.margenDeError = factor;
                    toggleFacil.value = (toggle == toggleFacil);
                    toggleNormal.value = (toggle == toggleNormal);
                    toggleDificil.value = (toggle == toggleDificil);
                    toggleMuyDificil.value = (toggle == toggleMuyDificil);
                    ActualizarEtiquetasTiempo();
                }
            });
        };

        configurarDificultad(toggleFacil, valueToggles[0]);
        configurarDificultad(toggleNormal, valueToggles[1]);
        configurarDificultad(toggleDificil, valueToggles[2]);
        configurarDificultad(toggleMuyDificil, valueToggles[3]);
        /*fieldTerapeuta.value = nivelacionData.nombre_terapeuta;
        fieldPaciente.value = nivelacionData.nombre_paciente;

        fieldTerapeuta.RegisterValueChangedCallback(evt =>
        {
            nivelacionData.nombre_terapeuta = evt.newValue;
        });

        fieldPaciente.RegisterValueChangedCallback(evt =>
        {
            nivelacionData.nombre_paciente = evt.newValue;
        });*/

        jornadaActual = nivelacionData.jornadas[0];

        for (int i = 0; i < 5; i++)
        {
            var btn = toolbarJornadas.Q<Button>($"btn-j{i + 1}");
            btn.text = (i == 0) ? $"Jornada {i + 1}" : $"J{i + 1}";
            btn.style.fontSize = (i == 0) ? 35 : 30;
        }

        if (sliderJornadasDesbloqueadas != null)
        {
            sliderJornadasDesbloqueadas.lowValue = 1;
            sliderJornadasDesbloqueadas.highValue = 5;
            sliderJornadasDesbloqueadas.value = Mathf.Clamp(0, 1, 5);
            nivelacionData.jornadaDesbloqueada = sliderJornadasDesbloqueadas.value - 1;
            if (labelJornadasDesbloqueadas != null) labelJornadasDesbloqueadas.text = sliderJornadasDesbloqueadas.value.ToString();

            sliderJornadasDesbloqueadas.RegisterValueChangedCallback(evt =>
            {
                nivelacionData.jornadaDesbloqueada = evt.newValue - 1;
                if (labelJornadasDesbloqueadas != null) labelJornadasDesbloqueadas.text = evt.newValue.ToString();
            });
        }

        if (dropdownTurnoDesbloqueado != null)
        {
            // Rellenar opciones desde el enum
            var nombres = Enum.GetNames(typeof(Turno));
            dropdownTurnoDesbloqueado.choices = new List<string>(nombres);

            // Selección inicial
            dropdownTurnoDesbloqueado.value = nivelacionData.turnoDesbloqueado.ToString();

            dropdownTurnoDesbloqueado.RegisterValueChangedCallback(evt =>
            {
                if (Enum.TryParse<Turno>(evt.newValue, out var t))
                {
                    nivelacionData.turnoDesbloqueado = t;
                }
            });
        }

        ActualizarUI();
    }

    public void ActivateGame()
    {
        if (EventRegister.Instance)
        {
            EventRegister.Instance.AddInitialEvent(EventRegister.EventosInfo.Inicio, "nivel " + SceneLoader.Instance.getCurrentLevelId(EventRegister.TipoJuego.Cocina).ToString("00"), EventRegister.TipoJuego.Cocina);
            Debug.Log("se pudo iniciar el evento Inicio en KitchenLevelManager.");
        }
        else
        {
            Debug.Log("No se pudo iniciar el evento Inicio en KitchenLevelManager.");
        }
    }


    private void SeleccionarJornada(int index)
    {
        GuardarRecetasSeleccionadas();

        jornadaIndex = index;
        jornadaActual = nivelacionData.jornadas[index];

        for (int i = 0; i < 5; i++)
        {
            var btn = toolbarJornadas.Q<Button>($"btn-j{i + 1}");
            btn.text = (i == index) ? $"Jornada {i + 1}" : $"J{i + 1}";
            btn.style.fontSize = (i == index) ? 35 : 30;
        }

        ActualizarUI();
    }

    private void ActualizarUI()
    {
        sliderTiempoManual.SetValueWithoutNotify(jornadaActual.tiempoBaseManual);

        toggleFacil.SetValueWithoutNotify(jornadaActual.margenDeError == valueToggles[0]);
        toggleNormal.SetValueWithoutNotify(jornadaActual.margenDeError == valueToggles[1]);
        toggleDificil.SetValueWithoutNotify(jornadaActual.margenDeError == valueToggles[2]);
        toggleMuyDificil.SetValueWithoutNotify(jornadaActual.margenDeError == valueToggles[3]);

        ActualizarEtiquetasTiempo();

        // Puestos activos
        puestosContainer.Clear();
        foreach (var puesto in System.Enum.GetValues(typeof(PuestosDeTrabajo)).Cast<PuestosDeTrabajo>())
        {
            // Si es el puesto "Tabla_De_Picar", lo omitimos de la UI pero lo dejamos siempre activo
            if (puesto == PuestosDeTrabajo.Tabla_De_Picar)
            {
                if (!jornadaActual.puestosActivos.Contains(puesto))
                {
                    jornadaActual.puestosActivos.Add(puesto);
                    ActualizarRecetasPuestos(puesto, true);
                }
                continue; // saltamos a la siguiente iteración
            }

            var toggle = new Toggle(puesto.ToString().Replace("_", " "));
            toggle.AddToClassList("toggle");

            toggle.value = jornadaActual.puestosActivos.Contains(puesto);
            toggle.RegisterValueChangedCallback(evt =>
            {
                GetComponent<AudioSource>().Play();
                if (evt.newValue && !jornadaActual.puestosActivos.Contains(puesto))
                {
                    jornadaActual.puestosActivos.Add(puesto);
                    ActualizarRecetasPuestos(puesto, true);
                }
                else if (!evt.newValue)
                {
                    jornadaActual.puestosActivos.Remove(puesto);
                    ActualizarRecetasPuestos(puesto, false);
                }

                GuardarRecetasSeleccionadas();
            });
            puestosContainer.Add(toggle);
        }

        // Recetas disponibles
        ActualizarRecetasPuestos();
    }


    private void ActualizarRecetasPuestos(PuestosDeTrabajo puesto = PuestosDeTrabajo.Tabla_De_Picar, bool added = false)
    {
        recetasScroll.Clear();

        List<PuestosDeTrabajo> puestosActivosAntes = jornadaActual.puestosActivos
            .Where(p => p != puesto)
            .ToList();

        var recetasFiltradasAntes = recetasDatabase.recetas
            .Where(r => !r.esIntermedia)
            .Where(r => r.puestos.All(p => puestosActivosAntes.Contains(p)))
            .ToList();

        var recetasFiltradas = recetasDatabase.recetas
            .Where(r => !r.esIntermedia)
            .Where(r => r.puestos.All(p => jornadaActual.puestosActivos.Contains(p)))
            .ToList();

        VisualElement col1 = new VisualElement();
        VisualElement col2 = new VisualElement();
        col1.style.flexDirection = FlexDirection.Column;
        col2.style.flexDirection = FlexDirection.Column;

        for (int i = 0; i < recetasFiltradas.Count; i++)
        {
            var receta = recetasFiltradas[i];
            var toggle = new Toggle(receta.nombre);
            toggle.AddToClassList("toggle");
            if (added)
            {
                if (!recetasFiltradasAntes.Contains(receta))
                {
                    toggle.value = true;
                }
                else toggle.value = jornadaActual.recetasAsignadas.Contains(receta);
            }
            else toggle.value = jornadaActual.recetasAsignadas.Contains(receta);

            toggle.RegisterValueChangedCallback(evt =>
            {
                GetComponent<AudioSource>().Play();
                if (evt.newValue && !jornadaActual.recetasAsignadas.Contains(receta))
                    jornadaActual.recetasAsignadas.Add(receta);
                else if (!evt.newValue && jornadaActual.recetasAsignadas.Contains(receta))
                    jornadaActual.recetasAsignadas.Remove(receta);
                GuardarRecetasSeleccionadas();
            });

            if (i % 2 == 0) col1.Add(toggle);
            else col2.Add(toggle);
        }

        VisualElement row = new VisualElement();
        row.style.flexDirection = FlexDirection.Row;
        row.style.justifyContent = Justify.SpaceBetween;
        row.Add(col1);
        row.Add(col2);

        recetasScroll.Add(row);
    }

    // No hace falta hacerlo para todas las jornadas ya que al cambiar de jornadas tambien se guarda
    private void GuardarRecetasSeleccionadas()
    {
        if (recetasScroll == null || jornadaActual == null) return;

        var toggles = recetasScroll.Query<Toggle>().ToList();
        jornadaActual.recetasAsignadas.Clear();

        foreach (var toggle in toggles)
        {
            if (toggle.value)
            {
                var receta = recetasDatabase.recetas.FirstOrDefault(r => r.nombre == toggle.label);
                if (receta != null)
                    jornadaActual.recetasAsignadas.Add(receta);
            }
        }

        // Si no se seleccionó ninguna, usar la primera del scroll
        if (jornadaActual.recetasAsignadas.Count == 0 && toggles.Count > 0)
        {
            var primerToggle = toggles.First();
            var recetaPorDefecto = recetasDatabase.recetas.FirstOrDefault(r => r.nombre == primerToggle.label);
            if (recetaPorDefecto != null)
            {
                jornadaActual.recetasAsignadas.Add(recetaPorDefecto);
                Debug.Log($"[GuardarRecetasSeleccionadas] Ninguna seleccionada, se asignó la primera del scroll: {recetaPorDefecto.nombre}");
            }
        }

        // Si no hay toggles (ningún puesto seleccionado) -> usa la primera del database
        if (jornadaActual.recetasAsignadas.Count == 0 && toggles.Count == 0)
        {
            var recetaDBPorDefecto = recetasDatabase.recetas.FirstOrDefault();
            if (recetaDBPorDefecto != null)
            {
                jornadaActual.recetasAsignadas.Add(recetaDBPorDefecto);
                Debug.Log($"[GuardarRecetasSeleccionadas] No hay puestos seleccionados, se asignó la primera del database: {recetaDBPorDefecto.nombre}");
            }
            else
            {
                Debug.LogWarning("[GuardarRecetasSeleccionadas] Database de recetas vacía. No se pudo asignar receta por defecto.");
            }
        }
    }

    private void ActualizarEtiquetasTiempo()
    {
        int baseSegundos = jornadaActual.tiempoBaseManual;
        float dificultad = jornadaActual.margenDeError;

        int totalSegundos = Mathf.CeilToInt(baseSegundos * dificultad);

        int minutosBase = baseSegundos / 60;
        int segundosBase = baseSegundos % 60;

        int minutosTotal = totalSegundos / 60;
        int segundosTotal = totalSegundos % 60;

        labelTiempoManual.text = $"Tiempo de recetas: {minutosBase}m {segundosBase}s";
        labelTiempoTotal.text = $"Tiempo real por turno: {minutosTotal}m {segundosTotal}s";
    }

    [Serializable]
    private class ConfigSnapshot
    {   
        public string jornada;
        public int tiempo;
        public string dificultad;
        public string[] puestosActivos;
        public string[] recetasHabilitadas;  
    }

    private Dictionary<string, object> BuildConfigDict()
    {
        GuardarRecetasSeleccionadas();

        return new Dictionary<string, object>
        {
            ["jornada"] = $"Jornada {jornadaIndex + 1}",
            ["duración"] = jornadaActual.tiempoBaseManual,
            ["dificultad"] = GetDificultadNombre(jornadaActual.margenDeError),
            ["puestosActivos"] = jornadaActual.puestosActivos.Select(p => p.ToString()).ToArray(),
            ["recetasHabilitadas"] = jornadaActual.recetasAsignadas.Where(r => r != null).Select(r => r.nombre).ToArray()  
        };
    }

    private static string DictToInlineText(Dictionary<string, object> d)
    {
        string FormatVal(object v)
        {
            if (v is Array arr) return "[" + string.Join(",", arr.Cast<object>()) + "]";
            return v?.ToString() ?? "null";
        }
        return string.Join("; ", d.Select(kvp => $"{kvp.Key}={FormatVal(kvp.Value)}"));
    }

    private void SendSavedConfig()
    {
        var er = EventRegister.Instance;
        if (er == null)
        {
            Debug.LogError("[Config] EventRegister.Instance es null.");
            return;
        }

        var dict = BuildConfigDict();
        string payload = DictToInlineText(dict);

        er.AddToEvnt(Tuple.Create(EventRegister.EventosInfo.KitchenGuardarConfig, payload));
        er.EvntToJson();

        Debug.Log("[Config] Guardada: " + payload);
    }

    private string GetDificultadNombre(float factor)
    {
        if (Mathf.Approximately(factor, 1.5f)) return "Fácil";
        if (Mathf.Approximately(factor, 1.0f)) return "Normal";
        if (Mathf.Approximately(factor, 0.75f)) return "Difícil";
        if (Mathf.Approximately(factor, 0.5f)) return "Muy Difícil";
        return factor.ToString("0.##");
    }
}

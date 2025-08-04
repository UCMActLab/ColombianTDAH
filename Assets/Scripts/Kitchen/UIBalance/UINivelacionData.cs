using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;
using System;
using System.Collections.Generic;

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

    private Toggle toggleFacil, toggleNormal, toggleDificil, toggleMuyDificil;
    private float[] valueToggles = { 1.5f, 1.0f, 0.75f, 0.5f };


    private void OnEnable()
    {
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
        toggleFacil = root.Q<Toggle>("toggle-facil");
        toggleNormal = root.Q<Toggle>("toggle-normal");
        toggleDificil = root.Q<Toggle>("toggle-dificil");
        toggleMuyDificil = root.Q<Toggle>("toggle-muydificil");

        jornadaActual = nivelacionData.jornadas[0];

        for (int i = 0; i < 5; i++)
        {
            int index = i;
            var btn = toolbarJornadas.Q<Button>($"btn-j{i + 1}");
            btn.clickable.clicked += () => SeleccionarJornada(index);
        }

        root.Q<Button>("btn-comenzar").clicked += () =>
        {
            GuardarRecetasSeleccionadas();
            SceneManager.LoadScene("KitchenLevelSelector");
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
            btn.style.fontSize = (i == 0) ? 30 : 26;
        }

        ActualizarUI();
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
            btn.style.fontSize = (i == index) ? 30 : 26;
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
            var toggle = new Toggle(puesto.ToString().Replace("_", " "));

            toggle.value = jornadaActual.puestosActivos.Contains(puesto);
            toggle.RegisterValueChangedCallback(evt =>
            {
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
            .Where(r => r.puestos.All(p => puestosActivosAntes.Contains(p)))
            .ToList();

        var recetasFiltradas = recetasDatabase.recetas
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

        labelTiempoManual.text = $"Tiempo: {minutosBase}m {segundosBase}s";
        labelTiempoTotal.text = $"Tiempo aproximado por turno: {minutosTotal}m {segundosTotal}s";
    }
}

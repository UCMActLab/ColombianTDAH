using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using System.Linq;

public class UINivelacionData : MonoBehaviour
{
    public NivelacionData nivelacionData;
    public RecetasDatabase recetasDatabase;

    private VisualElement root;
    private JornadaData jornadaActual;
    private int jornadaIndex = 0;

    private VisualElement puestosContainer;
    private Slider sliderMargen;
    private Label labelMargen;
    private ScrollView recetasScroll;
    private VisualElement toolbarJornadas;

    private void OnEnable()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        puestosContainer = root.Q<VisualElement>("puestos-container");
        sliderMargen = root.Q<Slider>("slider-margen");
        labelMargen = root.Q<Label>("label-margen");
        recetasScroll = root.Q<ScrollView>("recetas-scroll");
        toolbarJornadas = root.Q<VisualElement>("toolbar-jornadas");

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

        SeleccionarJornada(0);
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
            btn.style.fontSize = (i == index) ? 18 : 14;
        }

        ActualizarUI();
    }

    private void ActualizarUI()
    {
        // Slider margen
        sliderMargen.SetValueWithoutNotify(jornadaActual.margenDeError);
        labelMargen.text = jornadaActual.margenDeError.ToString("F1") + "x";

        sliderMargen.RegisterValueChangedCallback(evt =>
        {
            jornadaActual.margenDeError = evt.newValue;
            labelMargen.text = evt.newValue.ToString("F1") + "x";
        });

        // Puestos activos
        puestosContainer.Clear();
        foreach (var puesto in System.Enum.GetValues(typeof(PuestosDeTrabajo)).Cast<PuestosDeTrabajo>())
        {
            var toggle = new Toggle(puesto.ToString());
            toggle.value = jornadaActual.puestosActivos.Contains(puesto);
            toggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue && !jornadaActual.puestosActivos.Contains(puesto))
                    jornadaActual.puestosActivos.Add(puesto);
                else if (!evt.newValue)
                    jornadaActual.puestosActivos.Remove(puesto);

                ActualizarRecetas();
            });
            puestosContainer.Add(toggle);
        }

        // Recetas disponibles
        ActualizarRecetas();
    }

    private void ActualizarRecetas()
    {
        recetasScroll.Clear();

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
            toggle.value = jornadaActual.recetasAsignadas.Contains(receta);

            toggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue && !jornadaActual.recetasAsignadas.Contains(receta))
                    jornadaActual.recetasAsignadas.Add(receta);
                else if (!evt.newValue && jornadaActual.recetasAsignadas.Contains(receta))
                    jornadaActual.recetasAsignadas.Remove(receta);
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
}

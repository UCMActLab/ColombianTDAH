using UnityEngine;
using UnityEngine.UI;
public class OptionsVolumeMenu : MonoBehaviour 
    //juego de los delfines, para el menu de opciones, controla el volumen
{
    [SerializeField] Slider volumeSlider;
    void Start()
    {
        //solo queremos que se quede el cambio dentro de la escena porque si no seria raro
        //que se cambiara todo el volumen del juego desde el menu especifico de los delfines
        ResetVolumeChanges();

    }

    //cambia el volumen del juego al valor que tenga el slider
    public void ChangeVolume()
    {
        AudioListener.volume = volumeSlider.value;

        //SaveVolumeSettings() //no hace falta de momento
    }

    public void ResetVolumeChanges()
    {
        volumeSlider.value = 1;
        AudioListener.volume = volumeSlider.value;
    }

    //por si quisieramos guardar el volumen y cargarlo
    public void LoadVolumeSettings()
    {
        //volumeSlider.value = PlayerPrefs.GetFloat("DolphinVolume");
    }

    //por si quisieramos guardar el volumen
    public void SaveVolumeSettings()
    {
        //PlayerPrefs.SetFloat("DolphinVolume", volumeSlider.value);
    }
}

using UnityEngine;

public class EscenaKitchenLevelSetup : MonoBehaviour
{

    [SerializeField] private GameObject[] lights;
    [SerializeField] private GameObject libroDeRecetas;
    [SerializeField] private AudioSource tablaSource;
    [SerializeField] private AudioClip tablaClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LevelKitchenManager.Instance.SetLibro(libroDeRecetas);
        LevelKitchenManager.Instance.SetLights(lights);
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Libro, libroDeRecetas.GetComponent<Animator>());
        SoundManager.Instance.SetAudioSource(ObjetosSound.Tabla, tablaSource);
        SoundManager.Instance.SetClip(ObjetosSound.Tabla, tablaClip);
        SoundManager.Instance.PlaySound(ObjetosSound.Tabla, tablaClip.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

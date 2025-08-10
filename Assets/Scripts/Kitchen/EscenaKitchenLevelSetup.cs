using UnityEngine;

public class EscenaKitchenLevelSetup : MonoBehaviour
{
    [Header("Luces y libro")]
    [SerializeField] private GameObject[] lights;
    [SerializeField] private GameObject libroDeRecetas;

    [Header("Workstations")]
    [SerializeField] private GameObject mixer;
    [SerializeField] private GameObject slice_table;
    [SerializeField] private GameObject pot;
    [SerializeField] private GameObject blender;
    [SerializeField] private GameObject pressure_cooker;
    [SerializeField] private GameObject pan;
    [SerializeField] private GameObject oven;

    [Header("Sounds")]
    [SerializeField] private AudioSource tablaSource;
    [SerializeField] private AudioClip tablaClip;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LevelKitchenManager.Instance.SetLibro(libroDeRecetas);
        LevelKitchenManager.Instance.SetLights(lights);

        // Animators
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Libro, libroDeRecetas.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Tabla_De_Picar, slice_table.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Mezcladora, mixer.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Olla, pot.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Licuadora, blender.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Olla_A_Presion, pressure_cooker.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Sarten, pan.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Horno, oven.GetComponent<Animator>());

        // Sonidos
        SoundManager.Instance.SetAudioSource(ObjetosSound.Tabla_De_Picar, tablaSource);
        SoundManager.Instance.SetClip(ObjetosSound.Tabla_De_Picar, tablaClip);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

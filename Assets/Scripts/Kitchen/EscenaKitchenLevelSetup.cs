using UnityEngine;

public class EscenaKitchenLevelSetup : MonoBehaviour
{
    [Header("Luces y libro")]
    [SerializeField] private GameObject[] lights;
    [SerializeField] private GameObject libroDeRecetas;

    [Header("Workstations")]
    [SerializeField] private GameObject mixer;
    [SerializeField] private GameObject sliceTable;
    [SerializeField] private GameObject pot;
    [SerializeField] private GameObject blender;
    [SerializeField] private GameObject pressureCooker;
    [SerializeField] private GameObject pan;
    [SerializeField] private GameObject oven;

    [Header("Sounds")]
    [SerializeField] private AudioSource mixerSource;
    [SerializeField] private AudioClip mixerClip;
    [SerializeField] private AudioSource sliceTableSource;
    [SerializeField] private AudioClip sliceTableClip;
    [SerializeField] private AudioSource potSource;
    [SerializeField] private AudioClip potClip;
    [SerializeField] private AudioSource blenderSource;
    [SerializeField] private AudioClip blenderClip;
    [SerializeField] private AudioSource pressureCookerSource;
    [SerializeField] private AudioClip pressureCookerClip;
    [SerializeField] private AudioSource panSource;
    [SerializeField] private AudioClip panClip;
    [SerializeField] private AudioSource ovenSource;
    [SerializeField] private AudioClip ovenClip;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LevelKitchenManager.Instance.SetLibro(libroDeRecetas);
        LevelKitchenManager.Instance.SetLights(lights);

        // Animators
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Libro, libroDeRecetas.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Tabla_De_Picar, sliceTable.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Mezcladora, mixer.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Olla, pot.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Licuadora, blender.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Olla_A_Presion, pressureCooker.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Sarten, pan.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Horno, oven.GetComponent<Animator>());

        // Sonidos
        SoundManager.Instance.SetAudioSource(ObjetosSound.Mezcladora, mixerSource);
        SoundManager.Instance.SetClip(ObjetosSound.Mezcladora, mixerClip);
        SoundManager.Instance.SetAudioSource(ObjetosSound.Tabla_De_Picar, sliceTableSource);
        SoundManager.Instance.SetClip(ObjetosSound.Tabla_De_Picar, sliceTableClip);
        SoundManager.Instance.SetAudioSource(ObjetosSound.Olla, potSource);
        SoundManager.Instance.SetClip(ObjetosSound.Olla, potClip);
        SoundManager.Instance.SetAudioSource(ObjetosSound.Licuadora, blenderSource);
        SoundManager.Instance.SetClip(ObjetosSound.Licuadora, blenderClip);
        SoundManager.Instance.SetAudioSource(ObjetosSound.Olla_A_Presion, pressureCookerSource);
        SoundManager.Instance.SetClip(ObjetosSound.Olla_A_Presion, pressureCookerClip);
        SoundManager.Instance.SetAudioSource(ObjetosSound.Sarten, panSource);
        SoundManager.Instance.SetClip(ObjetosSound.Sarten, panClip);
        SoundManager.Instance.SetAudioSource(ObjetosSound.Horno, ovenSource);
        SoundManager.Instance.SetClip(ObjetosSound.Horno, ovenClip);

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

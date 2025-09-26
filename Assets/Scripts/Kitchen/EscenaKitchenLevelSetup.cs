using TMPro;
using UnityEngine;

public class EscenaKitchenLevelSetup : MonoBehaviour
{
    [Header("Objetos generales de la escena")]
    [SerializeField] private Reloj contador;
    [SerializeField] private GameObject libroDeRecetas;
    [SerializeField] private GameObject tablon;
    [SerializeField] private GameObject tablonButton;
    [SerializeField] private Transform tablonPos;
    [SerializeField] private Transform cameraPos;
    [SerializeField] private GameObject recetasColgadas;
    [SerializeField] private GameObject hand;
    [SerializeField] private CalculateStats statsWin;
    [SerializeField] private CalculateStats statsLose;
    [SerializeField] private GameObject tutorialSystem;
    [SerializeField] private GameObject conveyor;
    [SerializeField] private GameObject sponge;
    [SerializeField] private GameObject pauseCollider;

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
    [SerializeField] private AudioSource spongeSource;
    [SerializeField] private AudioClip spongeClip;
    [SerializeField] private AudioSource recipeDeliveredSource;
    [SerializeField] private AudioClip recipeDeliveredClip;
    [SerializeField] private AudioSource handGrabSource;
    [SerializeField] private AudioClip handGrabClip;
    [SerializeField] private AudioSource handDropSource;
    [SerializeField] private AudioClip handDropClip;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        LevelKitchenManager.Instance.SetContador(contador);
        LevelKitchenManager.Instance.SetTablon(tablon);
        LevelKitchenManager.Instance.SetCameraTablonPos(tablonPos);
        LevelKitchenManager.Instance.SetCameraInitPos(cameraPos);
        LevelKitchenManager.Instance.SetRecetasColgadas(recetasColgadas);
        LevelKitchenManager.Instance.SetTablonButton(tablonButton);
        LevelKitchenManager.Instance.SetHand(hand);
        LevelKitchenManager.Instance.SetStatsWin(statsWin); 
        LevelKitchenManager.Instance.SetStatsLose(statsLose);
        LevelKitchenManager.Instance.SetTutorial(tutorialSystem);
        LevelKitchenManager.Instance.SetConveyor(conveyor);
        LevelKitchenManager.Instance.SetSponge(sponge);
        LevelKitchenManager.Instance.SetPauseCollider(pauseCollider);

        // Animators
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Libro, libroDeRecetas.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Tabla_De_Picar, sliceTable.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Mezcladora, mixer.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Olla, pot.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Licuadora, blender.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Olla_A_Presion, pressureCooker.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Sarten, pan.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Horno, oven.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Mano, hand.GetComponent<Animator>());
        AnimatorManager.Instance.SetAnimator(ObjetosAnim.Esponja, sponge.GetComponentInChildren<Animator>());

        // Sonidos
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.Mezcladora, mixerSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.Mezcladora, mixerClip);
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.Tabla_De_Picar, sliceTableSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.Tabla_De_Picar, sliceTableClip);
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.Olla, potSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.Olla, potClip);
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.Licuadora, blenderSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.Licuadora, blenderClip);
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.Olla_A_Presion, pressureCookerSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.Olla_A_Presion, pressureCookerClip);
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.Sarten, panSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.Sarten, panClip);
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.Horno, ovenSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.Horno, ovenClip);
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.Esponja, spongeSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.Esponja, spongeClip);
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.RecetaEntregada, recipeDeliveredSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.RecetaEntregada, recipeDeliveredClip);
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.HandGrab, handGrabSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.HandGrab, handGrabClip);
        KitchenSoundManager.Instance.SetAudioSource(ObjetosSound.HandDrop, handDropSource);
        KitchenSoundManager.Instance.SetClip(ObjetosSound.HandDrop, handDropClip);


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

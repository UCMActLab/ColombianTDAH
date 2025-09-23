using UnityEngine;

public class TutorialButton : MonoBehaviour
{
    [SerializeField] private string kitchenScene = "KitchenLevel";

    private ButtonAnimation botonAnim;

    private void Start()
    {
        botonAnim = GetComponent<ButtonAnimation>();
        botonAnim.OnAnimationEnd += StartTutorial;
    }
    public void StartTutorial()
    {
        LevelKitchenManager.Instance.StartTutorialMode(); // Activa el flag
        Debug.Log("Tutorial activado");
        SceneLoader.LoadScene(kitchenScene); // Carga la escena de cocina
    }
}

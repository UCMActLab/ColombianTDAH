using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialButton : MonoBehaviour
{
    [SerializeField] private GameObject tutorial;
    [SerializeField] private string kitchenScene = "KitchenLevel";

    private ButtonAnimation botonAnim;


    private void Start()
    {
       
        botonAnim = GetComponent<ButtonAnimation>();
        botonAnim.OnAnimationEnd += StartTutorial;
    }
    public void StartTutorial()
    {
        tutorial.SetActive(true);
        LevelKitchenManager.Instance.StartTutorialMode(); // Activa el flag
        Debug.Log("Tutorial activado");
        SceneManager.LoadScene(kitchenScene); // Carga la escena de cocina
    }
}

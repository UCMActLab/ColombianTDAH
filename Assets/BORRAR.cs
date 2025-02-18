using UnityEngine;

public class BORRAR : MonoBehaviour
{
    [SerializeField]
    DialogSettings _firstDialogToShow;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        DialogManager.Instance.StartDialog(_firstDialogToShow);       
    }
}

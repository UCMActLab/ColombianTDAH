using UnityEngine;

public class StartDialogs : MonoBehaviour
{
    [SerializeField]
    DialogSettings _startingDialogue = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if(_startingDialogue == null)
        {
            Debug.LogError("No starting Dialogue selected in gameObject '" + gameObject.name + "'");
        }
        else
        {
            DialogManager.Instance.StartDialog(_startingDialogue);
        }

        Debug.Log("Destroying script 'StartDialogs' because its function is completed");
        Destroy(gameObject);
    }
}

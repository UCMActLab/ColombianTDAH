using UnityEngine;

public class MisionUIManager : MonoBehaviour
{
    [SerializeField]
    GameObject decisionButtonsGO;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        decisionButtonsGO.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowDecisionButtons()
    {
        decisionButtonsGO.SetActive(true);
    }

    public void YesClicked()
    {
        Debug.Log("YEEES");
        Clicked();
    }

    public void NoClicked()
    {
        Debug.Log("NOOO");
        Clicked();
    }

    void Clicked()
    {
        decisionButtonsGO.SetActive(false);
    }
}
